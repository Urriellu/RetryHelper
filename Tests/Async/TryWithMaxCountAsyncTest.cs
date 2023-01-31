using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Retry;

namespace Tests
{
    [TestFixture]
    public class TryWithMaxCountAsyncTest
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
        public async Task TestTryUntilWithMaxTryCountAsync()
        {
            var times = 5;
            var generator = new Generator(times);
            bool result = false;
            Assert.That(await RetryHelperTest.MeasureTime(async () =>
                result = await _target.Try(async () => await generator.NextAsync()).WithMaxTryCount(times + 1).Until(t => t)),
                Is.EqualTo(RetryHelperTest.Interval * times).Within(RetryHelperTest.Tolerance));
            Assert.That(result, Is.True);
        }

        [Test]
        public void TestTryUntilWithMaxTryCountExceededAsync()
        {
            var times = 5;
            var generator = new Generator(times);
            bool result = false;
            Assert.That(RetryHelperTest.MeasureTime(() =>
                Assert.ThrowsAsync<TimeoutException>(async () =>
                    result = await _target.Try(async () => await generator.NextAsync()).WithMaxTryCount(times).Until(t => t))),
                Is.EqualTo(RetryHelperTest.Interval * (times - 1)).Within(RetryHelperTest.Tolerance));
            Assert.That(result, Is.False);
        }
    }
}