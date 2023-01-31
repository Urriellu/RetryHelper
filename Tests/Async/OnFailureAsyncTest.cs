using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using Retry;

namespace Tests
{
    [TestFixture]
    public sealed class OnFailureTestAsync
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
        public async Task TestOnFailureWithNoParameter()
        {
            int times = 5;
            var generator = new Generator(times);
            int onFailureTriggered = 0;
            await _target.TryAsync(() => generator.Next())
                .OnFailure(() => onFailureTriggered++)
                .Until(t => t);
            Assert.That(onFailureTriggered, Is.EqualTo(times));
        }

        [Test]
        public async Task TestOnFailureAfterFiveTimesAsync()
        {
            int times = 5;
            var generator = new Generator(times);
            int onFailureTriggered = 0;
            await _target.TryAsync(() => generator.Next())
                .OnFailure(t =>
                {
                    Assert.That(t, Is.False);
                    onFailureTriggered++;
                })
                .Until(t => t);
            Assert.That(onFailureTriggered, Is.EqualTo(times));
        }

        [Test]
        public async Task TestOnFailureAsyncAfterFiveTimesAsync()
        {
            int times = 5;
            var generator = new Generator(times);
            int onFailureTriggered = 0;
            await _target.TryAsync(() => generator.Next())
                .OnFailure(async (t, count) =>
                {
                    Assert.That(t, Is.False);
                    onFailureTriggered++;
                    // Perform some delay. If the task is not awaited, onFailureTriggered would be
                    // increased by subsequent tries and the assertion below would fail
                    await Task.Delay(RetryHelperTest.Interval * 2);
                    Assert.That(onFailureTriggered, Is.EqualTo(count));
                })
                .Until(t => t);
            Assert.That(onFailureTriggered, Is.EqualTo(times));
        }

        [Test]
        public async Task TestOnFailureAsyncWithNoParameterAsync()
        {
            int times = 5;
            var generator = new Generator(times);
            int onFailureTriggered = 0;
            await _target.TryAsync(() => generator.Next())
                .OnFailure(async () => await Task.Run(() => onFailureTriggered++))
                .Until(t => t);
            Assert.That(onFailureTriggered, Is.EqualTo(times));
        }

        [Test]
        public async Task TestOnFailureShouldNotFireIfSucceedAtFirstTimeAsync()
        {
            int times = 0;
            var generator = new Generator(times);
            int onFailureTriggered = 0;
            await _target.TryAsync(() => generator.Next())
                .OnFailure(() => onFailureTriggered++)
                .Until(t => t);
            Assert.That(onFailureTriggered, Is.EqualTo(0));
        }

        [Test]
        public async Task TestCircularReadStreamAsync()
        {
            const int len = 100;
            var stream = new MemoryStream();
            for (int i = 0; i < len; i++)
            {
                stream.WriteByte((byte)i);
            }
            stream.Seek(0, SeekOrigin.Begin);
            var binaryReader = new BinaryReader(stream);
            for (int i = 0; i < len * 3; i++)
            {
                byte b = await RetryHelper.Instance
                    .TryAsync(() => binaryReader.ReadByte())
                    .WithTryInterval(0)
                    .OnFailure(t => stream.Seek(0, SeekOrigin.Begin))
                    .UntilNoException<EndOfStreamException>();
                Console.Write("{0} ", b);
            }
        }

        [Test]
        public async Task TestOnFailureWithTryCountAsync()
        {
            int times = 5;
            var generator = new Generator(times);
            int onFailureTriggered = 0;
            await _target.TryAsync(() => generator.Next())
                .OnFailure((t, count) =>
                {
                    Assert.That(t, Is.False);
                    Assert.That(count, Is.EqualTo(++onFailureTriggered));
                })
                .Until(t => t);
            Assert.That(onFailureTriggered, Is.EqualTo(times));
        }

        [Test]
        public async Task TestMultipleOnFailureAsync()
        {
            int times = 5;
            var generator = new Generator(times);
            int onFailureTriggered1 = 0;
            int onFailureTriggered2 = 0;
            await _target.TryAsync(() => generator.Next())
                .OnFailure(async t =>
                {
                    await Task.Delay(500);
                    Assert.That(t, Is.False);
                    onFailureTriggered1++;
                })
                .OnFailure(async t =>
                {
                    await Task.Delay(50);
                    Assert.That(t, Is.False);
                    onFailureTriggered2++;
                })
                .Until(t => t);
            Assert.That(onFailureTriggered1, Is.EqualTo(times));
            Assert.That(onFailureTriggered2, Is.EqualTo(times));
        }
    }
}
