using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;

namespace Retry
{
    /// <summary>
    /// Represents the task to be retried.
    /// </summary>
    /// <typeparam name="T">The type of result returned by the retried delegate.</typeparam>
    public class AsyncRetryTask<T>
    {
        /// <summary>
        /// Represents a completed task.
        /// </summary>
        protected static readonly Task CompletedTask = Task.FromResult(false);

        protected readonly Func<Task<T>> TaskToTry;
        protected Func<T, Task<bool>> EndCondition;
        protected bool RetryOnException;

        protected int MaxTryCount;
        protected TimeSpan MaxTryTime;
        protected TimeSpan TryInterval;

        protected Stopwatch Stopwatch;
        protected string TimeoutErrorMsg;
        protected TraceSource TraceSource;
        protected int TriedCount;

        protected Type ExpectedExceptionType = typeof(Exception);
        protected Exception LastException;

        protected Func<T, int, Task> OnTimeoutAction = (result, tryCount) => CompletedTask;
        protected Func<T, int, Task> OnSuccessAction = (result, tryCount) => CompletedTask;
        protected Func<T, int, Task> OnFailureAction = (result, tryCount) => CompletedTask;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncRetryTask{T}"/> class.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="traceSource">The trace source.</param>
        public AsyncRetryTask(Func<Task<T>> task, TraceSource traceSource)
            : this(task, traceSource, RetryTask.DefaultMaxTryTime, RetryTask.DefaultMaxTryCount, RetryTask.DefaultTryInterval)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncRetryTask{T}"/> class.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="traceSource">The trace source.</param>
        /// <param name="maxTryTime">The max try time.</param>
        /// <param name="maxTryCount">The max try count.</param>
        /// <param name="tryInterval">The try interval.</param>
        public AsyncRetryTask(Func<Task<T>> task, TraceSource traceSource,
            TimeSpan maxTryTime, int maxTryCount, TimeSpan tryInterval)
        {
            TaskToTry = task;
            TraceSource = traceSource;
            MaxTryTime = maxTryTime;
            MaxTryCount = maxTryCount;
            TryInterval = tryInterval;
        }

        /// <summary>
        ///   Retries the task until the specified end condition is satisfied, 
        ///   or the max try time/count is exceeded, or an exception is thrown druing task execution.
        ///   Then returns the value returned by the task.
        /// </summary>
        /// <param name = "endCondition">The end condition.</param>
        /// <returns></returns>
        [DebuggerNonUserCode]
        public async Task<T> Until(Func<T, bool> endCondition)
        {
            EndCondition = (t) => Task.FromResult(endCondition(t));
            return await TryImplAsync();
        }

        /// <summary>
        ///   Retries the task until the specified end condition is satisfied, 
        ///   or the max try time/count is exceeded, or an exception is thrown druing task execution.
        ///   Then returns the value returned by the task.
        /// </summary>
        /// <param name = "endCondition">The end condition.</param>
        /// <returns></returns>
        [DebuggerNonUserCode]
        public async Task<T> Until(Func<T, Task<bool>> endCondition)
        {
            EndCondition = endCondition;
            return await TryImplAsync();
        }

        /// <summary>
        ///   Retries the task until the specified end condition is satisfied, 
        ///   or the max try time/count is exceeded, or an exception is thrown druing task execution.
        ///   Then returns the value returned by the task.
        /// </summary>
        /// <param name = "endCondition">The end condition.</param>
        /// <returns></returns>
        [DebuggerNonUserCode]
        public async Task<T> Until(Func<Task<bool>> endCondition)
        {
            EndCondition = (t) => endCondition();
            return await TryImplAsync();
        }

        /// <summary>
        ///   Retries the task until the specified end condition is satisfied, 
        ///   or the max try time/count is exceeded, or an exception is thrown druing task execution.
        ///   Then returns the value returned by the task.
        /// </summary>
        /// <param name = "endCondition">The end condition.</param>
        /// <returns></returns>
        [DebuggerNonUserCode]
        public async Task<T> Until(Func<bool> endCondition)
        {
            EndCondition = (t) => Task.FromResult(endCondition());
            return await TryImplAsync();
        }

        /// <summary>
        ///   Retries the task until no exception is thrown during the task execution.
        /// </summary>
        /// <returns></returns>
        [DebuggerNonUserCode]
        public async Task<T> UntilNoException()
        {
            RetryOnException = true;
            EndCondition = t => Task.FromResult(true);
            return await TryImplAsync();
        }

        /// <summary>
        ///   Retries the task until the specified exception or any derived exception is not thrown during the task execution.
        ///   Any other exception thrown is re-thrown.
        /// </summary>
        /// <returns></returns>
        [DebuggerNonUserCode]
        public async Task<T> UntilNoException<TException>()
        {
            return await UntilNoException(typeof(TException));
        }

        /// <summary>
        ///   Retries the task until the specified exception or any derived exception is not thrown during the task execution.
        ///   Any other exception thrown is re-thrown.
        /// </summary>
        /// <returns></returns>
        [DebuggerNonUserCode]
        public async Task<T> UntilNoException(Type exceptionType)
        {
            if (!typeof(Exception).IsAssignableFrom(exceptionType))
            {
                throw new ArgumentException($"Parameter {nameof(exceptionType)} must be a type that is assignable to type System.Exception.", nameof(exceptionType));
            }
            ExpectedExceptionType = exceptionType;
            return await UntilNoException();
        }

        /// <summary>
        ///   Configures the max try time limit in milliseconds.
        /// </summary>
        /// <param name = "milliseconds">The max try time limit in milliseconds.</param>
        /// <returns></returns>
        public AsyncRetryTask<T> WithTimeLimit(int milliseconds)
        {
            return WithTimeLimit(TimeSpan.FromMilliseconds(milliseconds));
        }

        /// <summary>
        ///   Configures the max try time limit.
        /// </summary>
        /// <param name = "maxTryTime">The max try time limit.</param>
        /// <returns></returns>
        public AsyncRetryTask<T> WithTimeLimit(TimeSpan maxTryTime)
        {
            var retryTask = Clone();
            retryTask.MaxTryTime = maxTryTime;
            return retryTask;
        }

        /// <summary>
        ///   Configures the try interval time in milliseconds.
        /// </summary>
        /// <param name = "milliseconds">The try interval time in milliseconds.</param>
        /// <returns></returns>
        public AsyncRetryTask<T> WithTryInterval(int milliseconds)
        {
            return WithTryInterval(TimeSpan.FromMilliseconds(milliseconds));
        }

        /// <summary>
        ///   Configures the try interval time.
        /// </summary>
        /// <param name = "tryInterval">The try interval time.</param>
        /// <returns></returns>
        public AsyncRetryTask<T> WithTryInterval(TimeSpan tryInterval)
        {
            var retryTask = Clone();
            retryTask.TryInterval = tryInterval;
            return retryTask;
        }

        /// <summary>
        ///   Configures the max try count limit.
        /// </summary>
        /// <param name = "maxTryCount">The max try count.</param>
        /// <returns></returns>
        public AsyncRetryTask<T> WithMaxTryCount(int maxTryCount)
        {
            var retryTask = Clone();
            retryTask.MaxTryCount = maxTryCount;
            return retryTask;
        }

        /// <summary>
        /// Configures the action to take when the try action timed out before success.
        /// </summary>
        /// <param name="timeoutAction">The action to take on timeout.</param>
        /// <returns></returns>
        public AsyncRetryTask<T> OnTimeout(Action timeoutAction)
        {
            return OnTimeout((result, tryCount) => timeoutAction());
        }

        /// <summary>
        /// Configures the action to take when the try action timed out before success. 
        /// The result of the last failed attempt is passed as parameter to the action.
        /// For <see cref="UntilNoException"/>, the parameter passed to the action 
        /// is always <c>default(T)</c>
        /// </summary>
        /// <param name="timeoutAction">The action to take on timeout.</param>
        /// <returns></returns>
        public AsyncRetryTask<T> OnTimeout(Action<T> timeoutAction)
        {
            return OnTimeout((result, tryCount) => timeoutAction(result));

        }

        /// <summary>
        /// Configures the action to take when the try action timed out before success. 
        /// The result of the last failed attempt and the total count of attempts 
        /// are passed as parameters to the action.
        /// For <see cref="UntilNoException"/>, the parameter passed to the action 
        /// is always <c>default(T)</c>
        /// </summary>
        /// <param name="timeoutAction">The action to take on timeout.</param>
        /// <returns></returns>
        public AsyncRetryTask<T> OnTimeout(Action<T, int> timeoutAction)
        {
            return OnTimeout((result, tryCount) => Task.Run(() => timeoutAction(result, tryCount)));
        }

        /// <summary>
        /// Configures the asynchronous action to take when the try action timed out before success.
        /// </summary>
        /// <param name="timeoutAction">The action to take on timeout.</param>
        /// <returns></returns>
        public AsyncRetryTask<T> OnTimeout(Func<Task> timeoutAction)
        {
            return OnTimeout((result, tryCount) => timeoutAction());
        }

        /// <summary>
        /// Configures the asynchronous action to take when the try action timed out before success. 
        /// The result of the last failed attempt is passed as parameter to the action.
        /// For <see cref="UntilNoException"/>, the parameter passed to the action 
        /// is always <c>default(T)</c>
        /// </summary>
        /// <param name="timeoutAction">The action to take on timeout.</param>
        /// <returns></returns>
        public AsyncRetryTask<T> OnTimeout(Func<T, Task> timeoutAction)
        {
            return OnTimeout((result, tryCount) => timeoutAction(result));
        }

        /// <summary>
        /// Configures the asynchronous action to take when the try action timed out before success. 
        /// The result of the last failed attempt and the total count of attempts 
        /// are passed as parameters to the action.
        /// For <see cref="UntilNoException"/>, the parameter passed to the action 
        /// is always <c>default(T)</c>
        /// </summary>
        /// <param name="timeoutAction">The action to take on timeout.</param>
        /// <returns></returns>
        public AsyncRetryTask<T> OnTimeout(Func<T, int, Task> timeoutAction)
        {
            var retryTask = Clone();
            retryTask.OnTimeoutAction += timeoutAction;
            return retryTask;
        }

        /// <summary>
        /// Configures the action to take after each time the try action fails and before the next try. 
        /// </summary>
        /// <param name="failureAction">The action to take on failure.</param>
        /// <returns></returns>
        public AsyncRetryTask<T> OnFailure(Action failureAction)
        {
            return OnFailure((result, tryCount) => failureAction());
        }

        /// <summary>
        /// Configures the action to take after each time the try action fails and before the next try. 
        /// The result of the failed try action will be passed as parameter to the action.
        /// </summary>
        /// <param name="failureAction">The action to take on failure.</param>
        /// <returns></returns>
        public AsyncRetryTask<T> OnFailure(Action<T> failureAction)
        {
            return OnFailure((result, tryCount) => failureAction(result));
        }

        /// <summary>
        /// Configures the action to take after each time the try action fails and before the next try. 
        /// The result of the failed try action and the total count of attempts that 
        /// have been performed are passed as parameters to the action.
        /// </summary>
        /// <param name="failureAction">The action to take on failure.</param>
        /// <returns></returns>
        public AsyncRetryTask<T> OnFailure(Action<T, int> failureAction)
        {
            return OnFailure((result, tryCount) => Task.Run(() => failureAction(result, tryCount)));
        }

        /// <summary>
        /// Configures the asynchronous action to take after each time the try action fails and before the next try. 
        /// </summary>
        /// <param name="failureAction">The action to take on failure.</param>
        /// <returns></returns>
        public AsyncRetryTask<T> OnFailure(Func<Task> failureAction)
        {
            return OnFailure((result, tryCount) => failureAction());
        }

        /// <summary>
        /// Configures the asynchronous action to take after each time the try action fails and before the next try. 
        /// The result of the failed try action and the total count of attempts that 
        /// have been performed are passed as parameters to the action.
        /// </summary>
        /// <param name="failureAction">The action to take on failure.</param>
        /// <returns></returns>
        public AsyncRetryTask<T> OnFailure(Func<T, Task> failureAction)
        {
            return OnFailure((result, tryCount) => failureAction(result));
        }

        /// <summary>
        /// Configures the asynchronous action to take after each time the try action fails and before the next try. 
        /// The result of the failed try action and the total count of attempts that 
        /// have been performed are passed as parameters to the action.
        /// </summary>
        /// <param name="failureAction">The action to take on failure.</param>
        /// <returns></returns>
        public AsyncRetryTask<T> OnFailure(Func<T, int, Task> failureAction)
        {
            var retryTask = Clone();
            retryTask.OnFailureAction += failureAction;
            return retryTask;
        }

        /// <summary>
        /// Configures the action to take when the try action succeeds.
        /// </summary>
        /// <param name="successAction">The action to take on success.</param>
        /// <returns></returns>
        public AsyncRetryTask<T> OnSuccess(Action successAction)
        {
            return OnSuccess((result, tryCount) => successAction());
        }

        /// <summary>
        /// Configures the action to take when the try action succeeds.
        /// The result of the successful attempt is passed as parameter to the action.
        /// </summary>
        /// <param name="successAction">The action to take on success.</param>
        /// <returns></returns>
        public AsyncRetryTask<T> OnSuccess(Action<T> successAction)
        {
            return OnSuccess((result, tryCount) => successAction(result));
        }

        /// <summary>
        /// Configures the action to take when the try action succeeds.
        /// The result of the successful attempt and the total count of attempts 
        /// are passed as parameters to the action. This count includes the 
        /// final successful one.
        /// </summary>
        /// <param name="successAction">The action to take on success.</param>
        /// <returns></returns>
        public AsyncRetryTask<T> OnSuccess(Action<T, int> successAction)
        {
            return OnSuccess((result, tryCount) => Task.Run(() => successAction(result, tryCount)));
        }

        /// <summary>
        /// Configures the asynchronous action to take when the try action succeeds.
        /// </summary>
        /// <param name="successAction">The action to take on success.</param>
        /// <returns></returns>
        public AsyncRetryTask<T> OnSuccess(Func<Task> successAction)
        {
            return OnSuccess((result, tryCount) => successAction());
        }

        /// <summary>
        /// Configures the asynchronous action to take when the try action succeeds.
        /// The result of the successful attempt is passed as parameter to the action.
        /// </summary>
        /// <param name="successAction">The action to take on success.</param>
        /// <returns></returns>
        public AsyncRetryTask<T> OnSuccess(Func<T, Task> successAction)
        {
            return OnSuccess((result, tryCount) => successAction(result));
        }

        /// <summary>
        /// Configures the asynchronous action to take when the try action succeeds.
        /// The result of the successful attempt and the total count of attempts 
        /// are passed as parameters to the action. This count includes the 
        /// final successful one.
        /// </summary>
        /// <param name="successAction">The action to take on success.</param>
        /// <returns></returns>
        public AsyncRetryTask<T> OnSuccess(Func<T, int, Task> successAction)
        {
            var retryTask = Clone();
            retryTask.OnSuccessAction += successAction;
            return retryTask;
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        protected virtual AsyncRetryTask<T> Clone()
        {
            return new AsyncRetryTask<T>(TaskToTry, TraceSource, MaxTryTime, MaxTryCount, TryInterval)
            {
                OnTimeoutAction = OnTimeoutAction,
                OnSuccessAction = OnSuccessAction,
                OnFailureAction = OnFailureAction,
            };
        }

        #region Private methods

        private async Task<T> TryImplAsync()
        {
            TraceSource.TraceVerbose("Starting trying with max try time {0} and max try count {1}.",
                MaxTryTime, MaxTryCount);
            TriedCount = 0;
            Stopwatch = Stopwatch.StartNew();

            // Start the try loop.
            T result;
            do
            {
                TraceSource.TraceVerbose("Trying time {0}, elapsed time {1}.", TriedCount, Stopwatch.Elapsed);
                result = default(T);

                try
                {
                    // Perform the try action.
                    result = await TaskToTry();
                }
                catch (Exception ex)
                {
                    if (ShouldThrow(ex))
                    {
                        throw;
                    }
                    // Otherwise, store the exception and continue.
                    LastException = ex;
                    continue;
                }

                if (await EndCondition(result))
                {
                    TraceSource.TraceVerbose("Trying succeeded after time {0} and total try count {1}.",
                        Stopwatch.Elapsed, TriedCount + 1);
                    await InvokeCallback(OnSuccessAction, result, TriedCount + 1);
                    return result;
                }
            } while (await ShouldContinue(result));

            // Should not continue. 
            await InvokeCallback(OnTimeoutAction, result, TriedCount);
            throw new TimeoutException(TimeoutErrorMsg, LastException);
        }

        /// <summary>
        /// Multicast delegate needs to be called and awaited one by one. Otherwise only the task
        /// from the last delegate is returned and awaited.
        /// </summary>
        private async Task InvokeCallback(Delegate callback, T result, int triedCount)
        {
            foreach (Func<T, int, Task> singleCallback in callback.GetInvocationList())
            {
                await singleCallback(result, triedCount);
            }
        }

        private bool ShouldThrow(Exception exception)
        {
            // If exception is not recoverable,
            if (exception is OutOfMemoryException || exception is AccessViolationException ||
                // or exception is not expected or not of expected type.
                !RetryOnException || !ExpectedExceptionType.IsInstanceOfType(exception))
            {
                TraceSource.TraceError("{0} detected when trying; throwing...", exception.GetType().Name);
                return true;
            }

            TraceSource.TraceVerbose("{0} detected when trying; continue trying...; details: {1}", exception.GetType().Name, exception);
            return false;
        }

        private async Task<bool> ShouldContinue(T result)
        {
            if (Stopwatch.Elapsed >= MaxTryTime)
            {
                TimeoutErrorMsg = string.Format(CultureInfo.InvariantCulture,
                    "The maximum try time {0} for the operation has been exceeded.", MaxTryTime);
                return false;
            }
            if (++TriedCount >= MaxTryCount)
            {
                TimeoutErrorMsg = string.Format(CultureInfo.InvariantCulture,
                    "The maximum try count {0} for the operation has been exceeded.", MaxTryCount);
                return false;
            }

            // If should continue, perform the OnFailure action and wait some time before next try.
            await InvokeCallback(OnFailureAction, result, TriedCount);
            // Using Task.Delay instead of Thread.Sleep actually makes the interval less precise
            // with ~15ms off. It could be ok for most retry scenarios where a lot of I/O operations
            // take place, but further investigation might be needed.
            await Task.Delay(TryInterval);
            return true;
        }

        #endregion
    }
}
