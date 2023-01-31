using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Retry;

namespace Tests
{
    [TestFixture]
    public class OnTimeoutAsyncTest
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
        public async Task TestOnTimeoutShouldNotFireAsync()
        {
            int times = 5;
            var generator = new Generator(times);
            await _target.TryAsync(() => generator.Next())
                .OnTimeout(Assert.Fail)
                .Until(t => t);
        }

        [Test]
        public void TestOnTimeoutWithNoParameterAsync()
        {
            int times = 5;
            var generator = new Generator(times);
            bool onTimeoutTriggered = false;
            Assert.ThrowsAsync<TimeoutException>(() =>
                _target.TryAsync(() => generator.Next())
                    .WithMaxTryCount(times - 1)
                    .OnTimeout(() => onTimeoutTriggered = true)
                    .Until(t => t));
            Assert.That(onTimeoutTriggered, Is.True);
        }

        [Test]
        public void TestOnTimeoutAfterFiveTimesAsync()
        {
            int times = 5;
            var generator = new Generator(times);
            bool onTimeoutTriggered = false;
            Assert.ThrowsAsync<TimeoutException>(() =>
                _target.TryAsync(() => generator.Next())
                    .WithMaxTryCount(times - 1)
                    .OnTimeout(t =>
                    {
                        Assert.IsFalse(t);
                        onTimeoutTriggered = true;
                    })
                    .Until(t => t));
            Assert.That(onTimeoutTriggered, Is.True);
        }

        [Test]
        public void TestOnTimeoutAsyncAfterFiveTimesAsync()
        {
            int times = 5;
            var generator = new Generator(times);
            bool onTimeoutTriggered = false;
            Assert.ThrowsAsync<TimeoutException>(() =>
                _target.TryAsync(() => generator.Next())
                    .WithMaxTryCount(times - 1)
                    .OnTimeout(async () =>
                    {
                        await Task.Delay(100);
                        onTimeoutTriggered = true;
                    })
                    .Until(t => t));
            Assert.That(onTimeoutTriggered, Is.True);
        }

        [Test]
        public void TestOnTimeoutAsyncWithNoParameterAsync()
        {
            int times = 5;
            var generator = new Generator(times);
            bool onTimeoutTriggered = false;
            Assert.ThrowsAsync<TimeoutException>(() =>
                _target.TryAsync(() => generator.Next())
                    .WithMaxTryCount(times - 1)
                    .OnTimeout(async t =>
                    {
                        await Task.Delay(100);
                        Assert.IsFalse(t);
                        onTimeoutTriggered = true;
                    })
                    .Until(t => t));
            Assert.That(onTimeoutTriggered, Is.True);
        }

        [Test]
        public void TestOnTimeoutWithTriedCountAsync()
        {
            int times = 5;
            var generator = new Generator(times);
            bool onTimeoutTriggered = false;
            int maxTryCount = times - 1;
            Assert.ThrowsAsync<TimeoutException>(() =>
                _target.TryAsync(() => generator.Next())
                    .WithMaxTryCount(maxTryCount)
                    .OnTimeout((t, count) =>
                    {
                        Assert.That(count, Is.EqualTo(maxTryCount));
                        onTimeoutTriggered = true;
                    })
                    .Until(t => t));
            Assert.That(onTimeoutTriggered, Is.True);
        }

        [Test]
        public void TestMultipleOnTimeoutAsync()
        {
            int times = 5;
            var generator = new Generator(times);
            bool onTimeoutTriggered1 = false;
            bool onTimeoutTriggered2 = false;
            Assert.ThrowsAsync<TimeoutException>(() =>
                _target.TryAsync(() => generator.Next())
                    .WithMaxTryCount(times - 1)
                    .OnTimeout(async () =>
                    {
                        await Task.Delay(400);
                        onTimeoutTriggered1 = true;
                    })
                    .OnTimeout(async () =>
                    {
                        await Task.Delay(50);
                        onTimeoutTriggered2 = true;
                    })
                    .Until(t => t));
            Assert.That(onTimeoutTriggered1, Is.True);
            Assert.That(onTimeoutTriggered2, Is.True);
        }
    }
}
