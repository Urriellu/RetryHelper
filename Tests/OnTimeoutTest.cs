using System;
using NUnit.Framework;
using Retry;

namespace Tests
{
    [TestFixture]
    public class OnTimeoutTest
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
        public void TestOnTimeoutShouldNotFire()
        {
            int times = 5;
            var generator = new Generator(times);
            _target.Try(() => generator.Next())
                   .OnTimeout(t => Assert.Fail())
                   .Until(t => t);
        }

        [Test]
        public void TestOnTimeoutWithNoParameter()
        {
            int times = 5;
            var generator = new Generator(times);
            bool onTimeoutTriggered = false;
            Assert.That(() =>
                _target.Try(() => generator.Next())
                       .WithMaxTryCount(times - 1)
                       .OnTimeout(() => onTimeoutTriggered = true)
                       .Until(t => t),
                Throws.TypeOf<TimeoutException>());
            Assert.That(onTimeoutTriggered, Is.True);
        }

        [Test]
        public void TestOnTimeoutAfterFiveTimes()
        {
            int times = 5;
            var generator = new Generator(times);
            bool onTimeoutTriggered = false;
            Assert.That(() =>
                _target.Try(() => generator.Next())
                       .WithMaxTryCount(times - 1)
                       .OnTimeout(t =>
                       {
                           Assert.IsFalse(t);
                           onTimeoutTriggered = true;
                       })
                       .Until(t => t),
                Throws.TypeOf<TimeoutException>());
            Assert.That(onTimeoutTriggered, Is.True);
        }

        [Test]
        public void TestOnTimeoutWithTriedCount()
        {
            int times = 5;
            var generator = new Generator(times);
            bool onTimeoutTriggered = false;
            int maxTryCount = times - 1;
            Assert.That(() =>
                _target.Try(() => generator.Next())
                       .WithMaxTryCount(maxTryCount)
                       .OnTimeout((t, count) =>
                       {
                           Assert.That(count, Is.EqualTo(maxTryCount));
                           onTimeoutTriggered = true;
                       })
                       .Until(t => t),
                Throws.TypeOf<TimeoutException>());
            Assert.That(onTimeoutTriggered, Is.True);
        }

        [Test]
        public void TestMultipleOnTimeout()
        {
            int times = 5;
            var generator = new Generator(times);
            bool onTimeoutTriggered1 = false;
            bool onTimeoutTriggered2 = false;
            Assert.That(() =>
                _target.Try(() => generator.Next())
                       .WithMaxTryCount(times - 1)
                       .OnTimeout(t => onTimeoutTriggered1 = true)
                       .OnTimeout(t => onTimeoutTriggered2 = true)
                       .Until(t => t),
                Throws.TypeOf<TimeoutException>());
            Assert.That(onTimeoutTriggered1, Is.True);
            Assert.That(onTimeoutTriggered2, Is.True);
        }
    }
}