using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Retry;

namespace Tests
{
    [TestFixture]
    public class OnSuccessAsyncTest
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
        public async Task TestOnSuccessWithNoParameterAsync()
        {
            int times = 5;
            var generator = new Generator(times);
            bool onSuccessTriggered = false;
            await _target.TryAsync(() => generator.Next())
                .OnSuccess(() => onSuccessTriggered = true)
                .Until(t => t);
            Assert.That(onSuccessTriggered, Is.True);
        }

        [Test]
        public async Task TestOnSuccessAfterFiveTimesAsync()
        {
            int times = 5;
            var generator = new Generator(times);
            bool onSuccessTriggered = false;
            await _target.TryAsync(() => generator.Next())
                .OnSuccess(t => {
                    Assert.IsTrue(t);
                    onSuccessTriggered = true;
                })
                .Until(t => t);
            Assert.That(onSuccessTriggered, Is.True);
        }

        [Test]
        public async Task TestOnSuccessAsyncWithNoParameterAsync()
        {
            int times = 5;
            var generator = new Generator(times);
            bool onSuccessTriggered = false;
            await _target.TryAsync(() => generator.Next())
                .OnSuccess(async () =>
                {
                    await Task.Delay(100);
                    onSuccessTriggered = true;
                })
                .Until(t => t);
            Assert.That(onSuccessTriggered, Is.True);
        }

        [Test]
        public async Task TestOnSuccessAsyncAfterFiveTimesAsync()
        {
            int times = 5;
            var generator = new Generator(times);
            bool onSuccessTriggered = false;
            await _target.TryAsync(() => generator.Next())
                .OnSuccess(async t =>
                {
                    await Task.Delay(100);
                    Assert.IsTrue(t);
                    onSuccessTriggered = true;
                })
                .Until(t => t);
            Assert.That(onSuccessTriggered, Is.True);
        }

        [Test]
        public void TestOnSuccessShouldNotFireAsync()
        {
            int times = 5;
            var generator = new Generator(times);
            bool onSuccessTriggered = false;
            Assert.ThrowsAsync<TimeoutException>(() =>
                _target.TryAsync(() => generator.Next())
                    .WithMaxTryCount(times - 1)
                    .OnSuccess(t => onSuccessTriggered = true)
                    .Until(t => t));
            Assert.That(onSuccessTriggered, Is.False);
        }

        [Test]
        public async Task TestOnSuccessWithTriedCountAsync()
        {
            int times = 5;
            var generator = new Generator(times);
            bool onSuccessTriggered = false;
            await _target.TryAsync(() => generator.Next())
                .OnSuccess((t, count) =>
                {
                    Assert.That(count, Is.EqualTo(times + 1));
                    onSuccessTriggered = true;
                })
                .Until(t => t);
            Assert.That(onSuccessTriggered, Is.True);
        }

        [Test]
        public async Task TestMultipleOnSuccessAsync()
        {
            int times = 5;
            var generator = new Generator(times);
            bool onSuccessTriggered1 = false;
            bool onSuccessTriggered2 = false;
            await _target.TryAsync(() => generator.Next())
                   .OnSuccess(async t =>
                   {
                       await Task.Delay(400);
                       onSuccessTriggered1 = true;
                   })
                   .OnSuccess(async t =>
                   {
                       await Task.Delay(50);
                       onSuccessTriggered2 = true;
                   })
                   .Until(t => t);
            Assert.That(onSuccessTriggered1, Is.True);
            Console.WriteLine("AA");
            Assert.That(onSuccessTriggered2, Is.True);
        }
    }
}
