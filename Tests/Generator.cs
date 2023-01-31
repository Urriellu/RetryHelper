using System;
using System.Threading.Tasks;

namespace Tests
{
    class Generator
    {
        private readonly int _trueAfterTimes;
        private readonly bool _throwsException;

        public bool RandomExceptionType { get; init; }

        public int TriedTimes { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Generator" /> class.
        /// </summary>
        /// <param name="trueAfterTimes">The result will become <c>true</c> after the specified times.</param>
        /// <param name="throwsException">If set to <c>true</c>, an exception will be thrown for each failure.</param>
        public Generator(int trueAfterTimes, bool throwsException = false)
        {
            _trueAfterTimes = trueAfterTimes;
            _throwsException = throwsException;
        }

        public bool Next()
        {
            bool result =  (TriedTimes = TriedTimes + 1) > _trueAfterTimes;
            if (result || !_throwsException) return result;
            if (RandomExceptionType && TriedTimes % 2 == 0)
            {
                Console.WriteLine("Throwing InvalidOperationException");
                throw new InvalidOperationException();
            }
            else
            {
                Console.WriteLine("Throwing ApplicationException");
                throw new ApplicationException();
            }
        }

        public Task<bool> NextAsync()
        {
            return Task.FromResult(Next());
        }
    }
}