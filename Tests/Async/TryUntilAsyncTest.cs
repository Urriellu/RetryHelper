﻿using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Retry;

namespace Tests
{
    [TestFixture]
    public class TryUntilAsyncTest
    {
        private RetryHelper _target;

        [SetUp]
        public void SetUp()
        {
            _target = new RetryHelper();
            _target.DefaultTryInterval = TimeSpan.FromMilliseconds(RetryHelperTest.Interval);
        }

        [Test]
        public async Task TestTryUntilAfterFiveTimesAsync()
        {
            var times = 5;
            var generator = new Generator(times);
            bool result = false;
            Assert.That(
                await RetryHelperTest.MeasureTime(async () =>
                    result = await _target
                        .Try(async () => await generator.NextAsync())
                        .Until(t => t)),
                Is.EqualTo(RetryHelperTest.Interval * times).Within(RetryHelperTest.Tolerance));
            Assert.That(generator.TriedTimes, Is.EqualTo(times + 1));
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task TestTryUntilSuccessFirstTimeAsync()
        {
            var times = 0;
            var generator = new Generator(times);
            bool result = false;
            Assert.That(
                await RetryHelperTest.MeasureTime(async () =>
                    result = await _target
                        .Try(async () => await generator.NextAsync())
                        .Until(t => t)),
                Is.EqualTo(RetryHelperTest.Interval * times).Within(RetryHelperTest.Tolerance));
            Assert.That(generator.TriedTimes, Is.EqualTo(times + 1));
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task TestTryAsyncUntilAfterFiveTimesAsync()
        {
            var times = 5;
            var generator = new Generator(times);
            bool result = false;
            Assert.That(
                await RetryHelperTest.MeasureTime(async () =>
                    result = await _target
                        .TryAsync(() => generator.Next())
                        .Until(t => t)),
                Is.EqualTo(RetryHelperTest.Interval * times).Within(RetryHelperTest.Tolerance));
            Assert.That(generator.TriedTimes, Is.EqualTo(times + 1));
            Assert.That(result, Is.True);
        }
    }
}
