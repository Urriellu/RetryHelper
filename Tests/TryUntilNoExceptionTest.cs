using System;
using NUnit.Framework;
using Retry;

namespace Tests
{
    [TestFixture]
    public class TryUntilNoExceptionTest
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
        public void TestTryUntilNoExceptionAfterFiveTimes()
        {
            var times = 10;
            var generator = new Generator(times, true)
            {
                RandomExceptionType = true
            };
            bool result = false;
            Assert.That(RetryHelperTest.MeasureTime(() =>
                result = _target.Try(() => generator.Next()).UntilNoException()),
                Is.EqualTo(RetryHelperTest.Interval * times).Within(RetryHelperTest.Tolerance));
            Assert.That(generator.TriedTimes, Is.EqualTo(times + 1));
            Assert.That(result, Is.True);
        }

        [Test]
        public void TestTryUntilNoExceptionSuccessFirstTime()
        {
            var times = 0;
            var generator = new Generator(times);
            bool result = false;
            Assert.That(RetryHelperTest.MeasureTime(() =>
                result = _target.Try(() => generator.Next()).Until(t => t)),
                Is.EqualTo(RetryHelperTest.Interval * times).Within(RetryHelperTest.Tolerance));
            Assert.That(generator.TriedTimes, Is.EqualTo(times + 1));
            Assert.That(result, Is.True);
        }

        [Test]
        public void TestTryUntilNoExceptionOfTypeAfterFiveTimes()
        {
            var times = 10;
            var generator = new Generator(times, true);
            bool result = false;
            Assert.That(RetryHelperTest.MeasureTime(() =>
                result = _target.Try(() => generator.Next()).UntilNoException<ApplicationException>()),
                Is.EqualTo(RetryHelperTest.Interval * times).Within(RetryHelperTest.Tolerance));
            Assert.That(generator.TriedTimes, Is.EqualTo(times + 1));
            Assert.That(result, Is.True);
        }

        [Test]
        public void TestTryUntilNoExceptionOfTypePassedAsParameterAfterFiveTimes()
        {
            var times = 10;
            var generator = new Generator(times, true);
            bool result = false;
            Assert.That(RetryHelperTest.MeasureTime(() =>
                result = _target.Try(() => generator.Next()).UntilNoException(typeof(ApplicationException))),
                Is.EqualTo(RetryHelperTest.Interval * times).Within(RetryHelperTest.Tolerance));
            Assert.That(generator.TriedTimes, Is.EqualTo(times + 1));
            Assert.That(result, Is.True);
        }

        [Test]
        public void TestTryUntilNoExceptionOfTypeHavingOtherException()
        {
            var times = 10;
            var generator = new Generator(times, true)
            {
                RandomExceptionType = true
            };
            bool result = false;
            Assert.That(() =>
                result = _target.Try(() => generator.Next()).UntilNoException<ApplicationException>(),
                Throws.TypeOf<InvalidOperationException>());
            Assert.That(result, Is.False);
        }

        [Test]
        public void TestTryUntilNoExceptionOfTypePassedAsParameterHavingOtherException()
        {
            var times = 10;
            var generator = new Generator(times, true)
            {
                RandomExceptionType = true
            };
            bool result = false;
            Assert.That(() =>
                result = _target.Try(() => generator.Next()).UntilNoException(typeof(ApplicationException)),
                Throws.TypeOf<InvalidOperationException>());
            Assert.That(result, Is.False);
        }
    }
}
