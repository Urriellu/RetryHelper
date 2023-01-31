using System;
using System.IO;
using NUnit.Framework;
using Retry;

namespace Tests
{
    [TestFixture]
    public sealed class OnFailureTest
    {
        private RetryHelper _target;

        [SetUp]
        public void SetUp()
        {
            _target = new RetryHelper
            {
                DefaultTryInterval = TimeSpan.FromMilliseconds(RetryHelperTest.Interval)
            };
        }

        [Test]
        
        public void TestOnFailureWithNoParameter()
        {
            int times = 5;
            var generator = new Generator(times);
            int onFailureTriggered = 0;
            _target.Try(() => generator.Next())
                .OnFailure(() =>
                {
                    onFailureTriggered++;
                })
                .Until(t => t);
            Assert.That(onFailureTriggered, Is.EqualTo(times));
        }

        [Test]
        public void TestOnFailureAfterFiveTimes()
        {
            int times = 5;
            var generator = new Generator(times);
            int onFailureTriggered = 0;
            _target.Try(() => generator.Next())
                .OnFailure(t =>
                {
                    Assert.That(t, Is.False);
                    onFailureTriggered++;
                })
                .Until(t => t);
            Assert.That(onFailureTriggered, Is.EqualTo(times));
        }

        [Test]
        public void TestOnFailureShouldNotFireIfSucceedAtFirstTime()
        {
            int times = 0;
            var generator = new Generator(times);
            int onFailureTriggered = 0;
            _target.Try(() => generator.Next())
                   .OnFailure(t => onFailureTriggered++)
                   .Until(t => t);
            Assert.That(onFailureTriggered, Is.EqualTo(0));
        }

        [Test]
        public void TestCircularReadStream()
        {
            const int len = 100;
            var stream = new MemoryStream();
            for(int i = 0; i < len; i++)
            {
                stream.WriteByte((byte)i);
            }
            stream.Seek(0, SeekOrigin.Begin);
            var binaryReader = new BinaryReader(stream);
            for(int i = 0; i < len * 3; i++)
            {
                byte b = RetryHelper.Instance
                                   .Try(() => binaryReader.ReadByte())
                                   .WithTryInterval(0)
                                   .OnFailure(t => stream.Seek(0, SeekOrigin.Begin))
                                   .UntilNoException<EndOfStreamException>();
                Console.Write("{0} ", b);
            }
        }

        [Test]
        public void TestOnFailureWithTryCount()
        {
            int times = 5;
            var generator = new Generator(times);
            int onFailureTriggered = 0;
            _target.Try(() => generator.Next())
                   .OnFailure((t, count) =>
                   {
                       Assert.That(t, Is.False);
                       Assert.That(count, Is.EqualTo(++onFailureTriggered));
                   })
                   .Until(t => t);
            Assert.That(onFailureTriggered, Is.EqualTo(times));
        }

        [Test]
        public void TestMultipleOnFailure()
        {
            int times = 5;
            var generator = new Generator(times);
            int onFailureTriggered1 = 0;
            int onFailureTriggered2 = 0;
            _target.Try(() => generator.Next())
                .OnFailure(t =>
                {
                    Assert.That(t, Is.False);
                    onFailureTriggered1++;
                })
                .OnFailure(t =>
                {
                    Assert.That(t, Is.False);
                    onFailureTriggered2++;
                })
                .Until(t => t);
            Assert.That(onFailureTriggered1, Is.EqualTo(times));
            Assert.That(onFailureTriggered2, Is.EqualTo(times));
        }
    }
}
