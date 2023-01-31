﻿using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;

namespace Retry
{
    public static class Extensions
    {
        #region TraceSource

        /// <summary>
        /// Writes an error message to the trace listeners in the System.Diagnostics.TraceSource.Listeners
        /// collection using the specified object array and formatting information.
        /// </summary>
        /// <param name="traceSource">The trace source instance.</param>
        /// <param name="message">A composite format string that contains text intermixed with zero or more
        /// format items, which correspond to objects in the args array.</param>
        public static void TraceError(this TraceSource traceSource, string message)
        {
            traceSource.TraceData(TraceEventType.Error, 0, message);
        }

        /// <summary>
        /// Writes a verbose message to the trace listeners in the System.Diagnostics.TraceSource.Listeners
        /// collection using the specified object array and formatting information.
        /// </summary>
        /// <param name="traceSource">The trace source instance.</param>
        /// <param name="message">A composite format string that contains text intermixed with zero or more
        /// format items, which correspond to objects in the args array.</param>
        public static void TraceVerbose(this TraceSource traceSource, string message)
        {
            traceSource.TraceData(TraceEventType.Verbose, 0, message);
        }

        /// <summary>
        /// Writes an error message to the trace listeners in the System.Diagnostics.TraceSource.Listeners
        /// collection using the specified object array and formatting information.
        /// </summary>
        /// <param name="traceSource">The trace source instance.</param>
        /// <param name="format">A composite format string that contains text intermixed with zero or more
        /// format items, which correspond to objects in the args array.</param>
        /// <param name="args">An array containing zero or more objects to format.</param>
        public static void TraceError(this TraceSource traceSource, string format, params object[] args)
        {
            traceSource.TraceError(string.Format(CultureInfo.InvariantCulture, format, args));
        }

        /// <summary>
        /// Writes a verbose message to the trace listeners in the System.Diagnostics.TraceSource.Listeners
        /// collection using the specified object array and formatting information.
        /// </summary>
        /// <param name="traceSource">The trace source instance.</param>
        /// <param name="format">A composite format string that contains text intermixed with zero or more
        /// format items, which correspond to objects in the args array.</param>
        /// <param name="args">An array containing zero or more objects to format.</param>
        public static void TraceVerbose(this TraceSource traceSource, string format, params object[] args)
        {
            traceSource.TraceVerbose(string.Format(CultureInfo.InvariantCulture, format, args));
        }

        #endregion

        #region Delegates

        /// <summary>
        /// Makes an <see cref="Func{TResult}"/> instance from an <see cref="Action"/> instance. 
        /// The <see cref="Func{TResult}"/> instance would execute the <see cref="Action"/> delegate
        /// and return the specified value when being called.
        /// </summary>
        /// <typeparam name="TResult">The return type of <see cref="Func{TResult}"/>.</typeparam>
        /// <param name="action">The <see cref="Action"/> instance.</param>
        /// <param name="value">The return value of the <see cref="Func{TResult}"/> instance.</param>
        /// <returns></returns>
        [Obsolete("This method will become internal in a future version.")]
        public static Func<TResult> MakeFunc<TResult>(this Action action, TResult value = default(TResult))
        {
            if (action == null)
                throw new ArgumentNullException("action");

            return () =>
            {
                action();
                return value;
            };
        }

        /// <summary>
        /// Makes an Func&lt;Task&lt;TResult&gt;&gt; instance from an <see cref="Func{TResult}"/> instance. 
        /// Executing the returned delegate would execute the <paramref name="action"/> delegate, waiting
        /// on the task returned by the returned delegate would also wait for the task returned by the
        /// <paramref name="action"/> delegate. When the returned task is completed, it returns the value 
        /// specified by <paramref name="value"/>.
        /// </summary>
        /// <typeparam name="TResult">The return type of <see cref="Task{TResult}"/>.</typeparam>
        /// <param name="action">The <see cref="Func{TResult}"/> instance.</param>
        /// <param name="value">The return value of the <see cref="Task{TResult}"/> instance.</param>
        /// <returns></returns>
        internal static Func<Task<TResult>> MakeFunc<TResult>(this Func<Task> action, TResult value = default(TResult))
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            return async () =>
            {
                await action();
                return value;
            };
        }

        #endregion
    }
}
