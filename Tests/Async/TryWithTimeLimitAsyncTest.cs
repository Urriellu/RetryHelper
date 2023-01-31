using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Retry;

namespace Tests
{
    [TestFixture]
    public class TryWithTimeLimitAsyncTest
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
        public async Task TestTryUntilWithTimeLimitAsync()
        {
            int times = 5;
            var generator = new Generator(times);
            bool result = false;
            Assert.That(await RetryHelperTest.MeasureTime(async () =>
                result = await _target.Try(async () => await generator.NextAsync()).WithTimeLimit(RetryHelperTest.Interval * times + RetryHelperTest.Tolerance).Until(t => t)),
                Is.EqualTo(RetryHelperTest.Interval * times).Within(RetryHelperTest.Tolerance));
            Assert.That(result, Is.True);
        }

        [Test]
        public void TestTryUntilWithTimeLimitExceededAsync()
        {
            int times = 5;
            var generator = new Generator(times + 1);
            Assert.ThrowsAsync<TimeoutException>(async () =>
                await _target
                    .Try(async () => await generator.NextAsync())
                    .WithTimeLimit(RetryHelperTest.Interval * times)
                    .Until(t => t));
        }
    }
}