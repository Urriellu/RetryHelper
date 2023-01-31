using System;
using NUnit.Framework;
using Retry;

namespace Tests
{
    [TestFixture]
    public class TryUntilTest
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
        public void TestTryUntilAfterFiveTimes()
        {
            var times = 5;
            var generator = new Generator(times);
            bool result = false;
            Assert.That(RetryHelperTest.MeasureTime(() =>
                result = _target.Try(() => generator.Next()).Until(t => t)),
                Is.EqualTo(RetryHelperTest.Interval * times).Within(RetryHelperTest.Tolerance));
            Assert.That(generator.TriedTimes, Is.EqualTo(times + 1));
            Assert.That(result, Is.True);
        }

        [Test]
        public void TestTryUntilSuccessFirstTime()
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
    }
}