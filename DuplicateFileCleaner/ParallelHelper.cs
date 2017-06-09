using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DuplicateFileCleaner
{
    public class ParallelHelper
    {
        public delegate void ParallelAction<in T>(T obj);

        public delegate void ParallelActionWithCancel<in T>(T obj, CancellationTokenSource cancellation);

        public delegate void ParallelActionWithState<in T>(T obj, CancellationTokenSource cancellation, ParallelLoopState pls);

        private static ParallelOptions CreateParallelOptions(int threadPerRequest, CancellationTokenSource cts = null)
        {
            var options = new ParallelOptions();
            if (cts != null)
                options.CancellationToken = cts.Token;
            options.MaxDegreeOfParallelism = threadPerRequest;
            return options;
        }

        public static void For<T>(IList<T> list, int threadPerRequest, ParallelAction<T> action)
        {
            For(0, list.Count, threadPerRequest, delegate(int i, CancellationTokenSource cts) { action(list[i]); });
        }

        public static void For<T>(T[] array, int threadPerRequest, ParallelAction<T> action)
        {
            For(0, array.Length, threadPerRequest, delegate(int i, CancellationTokenSource cts) { action(array[i]); });
        }

        public static void For<T>(IList<T> list, int threadPerRequest, ParallelActionWithCancel<T> action)
        {
            For(0, list.Count, threadPerRequest, delegate(int i, CancellationTokenSource cts) { action(list[i], cts); });
        }

        public static void For<T>(T[] array, int threadPerRequest, ParallelActionWithCancel<T> action)
        {
            For(0, array.Length, threadPerRequest, delegate(int i, CancellationTokenSource cts) { action(array[i], cts); });
        }

        public static void For<T>(IList<T> list, int threadPerRequest, ParallelActionWithState<T> action)
        {
            For(0, list.Count, threadPerRequest,
                delegate(int i, CancellationTokenSource cts, ParallelLoopState pls) { action(list[i], cts, pls); });
        }

        public static void For<T>(T[] array, int threadPerRequest, ParallelActionWithState<T> action)
        {
            For(0, array.Length, threadPerRequest,
                delegate(int i, CancellationTokenSource cts, ParallelLoopState pls) { action(array[i], cts, pls); });
        }

        public static void For(int fromInclusive, int toExclusive, int threadPerRequest,
            ParallelActionWithCancel<int> action)
        {
            var cts = new CancellationTokenSource();
            var options = CreateParallelOptions(threadPerRequest, cts);
            RunWithCatch(() =>
            {
                Parallel.For(fromInclusive, toExclusive, options, i => { action(i, cts); });
            });
        }

        public static void For(int fromInclusive, int toExclusive, int threadPerRequest,
            ParallelActionWithState<int> action)
        {
            var cts = new CancellationTokenSource();
            var options = CreateParallelOptions(threadPerRequest, cts);
            RunWithCatch(() =>
            {
                Parallel.For(fromInclusive, toExclusive, options, (i, pls) => { action(i, cts, pls); });
            });
        }

        public static void ForEach<T>(IEnumerable<T> list, int threadPerRequest, ParallelAction<T> action)
        {
            ForEach<T>(list, threadPerRequest, delegate(T obj, CancellationTokenSource cts) { action(obj); });
        }

        public static void ForEach<T>(IEnumerable<T> list, Action<T> action)
        {
            Parallel.ForEach(list, action);
        }

        public static void ForEach<T>(IEnumerable<T> list, int threadPerRequest,
            ParallelActionWithCancel<T> action)
        {
            var cts = new CancellationTokenSource();
            var options = CreateParallelOptions(threadPerRequest, cts);
            RunWithCatch(() =>
            {
                Parallel.ForEach(list, options, obj => { action(obj, cts); });
            });
        }

        public static void ForEach<T>(IEnumerable<T> list, int threadPerRequest,
            ParallelActionWithState<T> action)
        {
            var cts = new CancellationTokenSource();
            var options = CreateParallelOptions(threadPerRequest, cts);
            RunWithCatch(() =>
            {
                Parallel.ForEach(list, options, (obj, pls) => { action(obj, cts, pls); });
            });
        }

        private static void RunWithCatch(Action action)
        {
            try
            {
                action();
            }
            catch (AggregateException aggException)
            {
                var exStr = new StringBuilder(200);
                exStr.Append(string.Format("ParallelHelperException[{0}]<br/>", aggException.Message));
                foreach (var ex in aggException.InnerExceptions)
                {
                    exStr.Append("-------------------------------------------------------<br/>");
                    exStr.Append(string.Format("Message:{0}<br/>", ex.Message));
                    exStr.Append(string.Format("StackTrace:{0}<br/>", ex.StackTrace));
                    var inner = ex.InnerException;
                    var i = 1;
                    while (inner != null)
                    {
                        exStr.Append(string.Format("第{0}层InnerException<br/>", i));
                        exStr.Append(string.Format("Message:{0}<br/>", inner.Message));
                        exStr.Append(string.Format("StackTrace:{0}<br/>", inner.StackTrace));
                        inner = inner.InnerException;
                        i++;
                    }
                    exStr.Append("-------------------------------------------------------<br/>");
                }
                throw new Exception(exStr.ToString());
            }
        }
    }
}
