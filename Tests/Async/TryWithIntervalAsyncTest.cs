﻿using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Retry;

namespace Tests
{
    [TestFixture]
    public class TryWithIntervalAsyncTest
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
        public async Task TestTryUntilWithTryIntervalAsync()
        {
            var times = 5;
            var generator = new Generator(times);
            bool result = false;
            Assert.That(await RetryHelperTest.MeasureTime(async () =>
                result = await _target.Try(() => generator.NextAsync()).WithTryInterval(RetryHelperTest.Interval * 2).Until(t => t)),
                Is.EqualTo(RetryHelperTest.Interval * 2 * times).Within(RetryHelperTest.Tolerance));
            Assert.That(result, Is.True);
        }
    }
}