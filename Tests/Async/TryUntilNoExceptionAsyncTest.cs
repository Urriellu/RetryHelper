using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Retry;

namespace Tests
{
    [TestFixture]
    public class TryUntilNoExceptionAsyncTest
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
        public async Task TestTryUntilNoExceptionAfterFiveTimesAsync()
        {
            var times = 10;
            var generator = new Generator(times, true)
            {
                RandomExceptionType = true
            };
            bool result = false;
            Assert.That(await RetryHelperTest.MeasureTime(async () =>
                result = await _target.Try(async () => await generator.NextAsync()).UntilNoException()),
                Is.EqualTo(RetryHelperTest.Interval * times).Within(RetryHelperTest.AsyncTolerance));
            Assert.That(generator.TriedTimes, Is.EqualTo(times + 1));
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task TestTryUntilNoExceptionSuccessFirstTimeAsync()
        {
            var times = 0;
            var generator = new Generator(times);
            bool result = false;
            Assert.That(await RetryHelperTest.MeasureTime(async () =>
                result = await _target.Try(async () => await generator.NextAsync()).Until(t => t)),
                Is.EqualTo(RetryHelperTest.Interval * times).Within(RetryHelperTest.AsyncTolerance));
            Assert.That(generator.TriedTimes, Is.EqualTo(times + 1));
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task TestTryUntilNoExceptionOfTypeAfterFiveTimesAsync()
        {
            var times = 10;
            var generator = new Generator(times, true);
            bool result = false;
            Assert.That(await RetryHelperTest.MeasureTime(async () =>
                result = await _target.Try(async () => await generator.NextAsync()).UntilNoException<ApplicationException>()),
                Is.EqualTo(RetryHelperTest.Interval * times).Within(RetryHelperTest.AsyncTolerance));
            Assert.That(generator.TriedTimes, Is.EqualTo(times + 1));
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task TestTryUntilNoExceptionOfTypePassedAsParameterAfterFiveTimesAsync()
        {
            var times = 10;
            var generator = new Generator(times, true);
            bool result = false;
            Assert.That(await RetryHelperTest.MeasureTime(async () =>
                result = await _target.Try(() => generator.NextAsync()).UntilNoException(typeof(ApplicationException))),
                Is.EqualTo(RetryHelperTest.Interval * times).Within(RetryHelperTest.AsyncTolerance));
            Assert.That(generator.TriedTimes, Is.EqualTo(times + 1));
            Assert.That(result, Is.True);
        }

        [Test]
        public void TestTryUntilNoExceptionOfTypeHavingOtherExceptionAsync()
        {
            var times = 10;
            var generator = new Generator(times, true)
            {
                RandomExceptionType = true
            };
            bool result = false;
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                result = await _target.Try(async () => await generator.NextAsync()).UntilNoException<ApplicationException>());
            Assert.That(result, Is.False);
        }

        [Test]
        public void TestTryUntilNoExceptionOfTypePassedAsParameterHavingOtherExceptionAsync()
        {
            var times = 10;
            var generator = new Generator(times, true)
            {
                RandomExceptionType = true
            };
            bool result = false;
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                result = await _target.Try(async () => await generator.NextAsync()).UntilNoException(typeof(ApplicationException)));
            Assert.That(result, Is.False);
        }
    }
}
