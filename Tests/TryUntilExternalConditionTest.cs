using System;
using NUnit.Framework;
using Retry;

namespace Tests
{
    [TestFixture]
    public class TryUntilExternalConditionTest
    {
        private RetryHelper _target;

        [SetUp]
        public void SetUp()
        {
            _target = new RetryHelper
            {
                DefaultTryInterval = TimeSpan.FromMilliseconds(100)
            };
        }

        [Test]
        public void TestTryUntilExpectedTime()
        {
            var expectedStopTime = DateTime.Now.AddSeconds(1);
            _target.Try(() => { }).Until(() => DateTime.Now >= expectedStopTime);
            Assert.That(DateTime.Now, Is.EqualTo(expectedStopTime).Within(TimeSpan.FromMilliseconds(200)));
        }
    }
}