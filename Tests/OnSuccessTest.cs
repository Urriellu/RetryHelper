﻿using System;
using NUnit.Framework;
using Retry;

namespace Tests
{
    [TestFixture]
    public class OnSuccessTest
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
        
        public void TestOnSuccessWithNoParameter()
        {
            var times = 5;
            var generator = new Generator(times);
            var onSuccessTriggered = false;
            _target.Try(() => generator.Next())
                   .OnSuccess(() => onSuccessTriggered = true)
                   .Until(t => t);
            Assert.That(onSuccessTriggered, Is.True);
        }

        [Test]
        public void TestOnSuccessAfterFiveTimes()
        {
            var times = 5;
            var generator = new Generator(times);
            var onSuccessTriggered = false;
            _target.Try(() => generator.Next())
                   .OnSuccess(t =>
                   {
                       Assert.IsTrue(t);
                       onSuccessTriggered = true;
                   })
                   .Until(t => t);
            Assert.That(onSuccessTriggered, Is.True);
        }

        [Test]
        public void TestOnSuccessShouldNotFire()
        {
            var times = 5;
            var generator = new Generator(times);
            Assert.That(() =>
                _target.Try(() => generator.Next())
                       .WithMaxTryCount(times - 1)
                       .OnSuccess(t => Assert.Fail())
                       .Until(t => t),
                Throws.TypeOf<TimeoutException>());
        }

        [Test]
        public void TestOnSuccessWithTriedCount()
        {
            var times = 5;
            var generator = new Generator(times);
            var onSuccessTriggered = false;
            _target.Try(() => generator.Next())
                   .OnSuccess((t, count) =>
                   {
                       Assert.That(count, Is.EqualTo(times + 1));
                       onSuccessTriggered = true;
                   })
                   .Until(t => t);
            Assert.That(onSuccessTriggered, Is.True);
        }

        [Test]
        public void TestMultipleOnSuccess()
        {
            var times = 5;
            var generator = new Generator(times);
            var onSuccessTriggered1 = false;
            var onSuccessTriggered2 = false;
            _target.Try(() => generator.Next())
                   .OnSuccess(t => onSuccessTriggered1 = true)
                   .OnSuccess(t => onSuccessTriggered2 = true)
                   .Until(t => t);
            Assert.That(onSuccessTriggered1, Is.True);
            Assert.That(onSuccessTriggered2, Is.True);
        }
    }
}