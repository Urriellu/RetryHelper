using System;
using NUnit.Framework;
using Retry;

namespace Tests
{
    [TestFixture]
    public class TryWithIntervalTest
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
        public void TestTryUntilWithTryInterval()
        {
            int times = 5;
            var generator = new Generator(times);
            bool result = false;
            Assert.That(RetryHelperTest.MeasureTime(() =>
                result = _target.Try(() => generator.Next()).WithTryInterval(RetryHelperTest.Interval * 2).Until(t => t)),
                Is.EqualTo(RetryHelperTest.Interval * 2 * times).Within(RetryHelperTest.Tolerance));
            Assert.That(result, Is.True);
        }
    }
}