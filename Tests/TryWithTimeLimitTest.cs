using System;
using NUnit.Framework;
using Retry;

namespace Tests
{
    [TestFixture]
    public class TryWithTimeLimitTest
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
        public void TestTryUntilWithTimeLimit()
        {
            var times = 5;
            var generator = new Generator(times);
            bool result = false;
            Assert.That(RetryHelperTest.MeasureTime(() =>
                result = _target.Try(() => generator.Next()).WithTimeLimit(RetryHelperTest.Interval * times + RetryHelperTest.Tolerance).Until(t => t)),
                Is.EqualTo(RetryHelperTest.Interval * times).Within(RetryHelperTest.Tolerance));
            Assert.That(result, Is.True);
        }

        [Test]
        public void TestTryUntilWithTimeLimitExceeded()
        {
            var times = 5;
            var generator = new Generator(times + 1);
            Assert.That(() =>
                _target.Try(() => generator.Next()).WithTimeLimit(RetryHelperTest.Interval * times).Until(t => t),
                Throws.TypeOf<TimeoutException>());
        }
    }
}