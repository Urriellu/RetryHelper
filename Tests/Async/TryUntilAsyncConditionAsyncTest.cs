using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Retry;

namespace Tests
{
    [TestFixture]
    public class TryUntilAsyncConditionAsyncTest
    {
        private RetryHelper _target;

        [SetUp]
        public void SetUp()
        {
            _target = new RetryHelper
            {
                DefaultTryInterval = TimeSpan.FromMilliseconds(50)
            };
        }

        [Test]
        public async Task TestTryUntilAsyncCondition()
        {
            var times = 5;
            var generator = new Generator(times);
            var result = await _target.Try(() => Task.FromResult(true)).Until(async () => await generator.NextAsync());
            Assert.That(generator.TriedTimes, Is.EqualTo(times + 1));
            Assert.IsTrue(result);
        }
    }
}