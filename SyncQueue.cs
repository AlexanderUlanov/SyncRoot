using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncQueue
{
    /// <summary>
    /// Represents queue which accepts pushes from multiple threads. Pop method returns a task of <typeparamref name="T"/> and will wait untill queue will be populated.
    /// </summary>
    /// <typeparam name="T">Type of the elements</typeparam>
    public class SyncQueue<T> : IEnumerable<T>, ICollection
    {
        private readonly object _syncRoot = new object();
        private readonly Queue<T> _innerQueue = new Queue<T>();
        private readonly object _waitingQueueLock = new object();
        private readonly Queue<TaskCompletionSource<T>> _waitingQueue = new Queue<TaskCompletionSource<T>>();

        public void Push(T item)
        {
            lock (_syncRoot)
                _innerQueue.Enqueue(item);

            ProcessWaitingQueue();
        }

        public async Task<T> Pop()
        {
            lock (_syncRoot)
            {
                if (_innerQueue.Any())
                    return _innerQueue.Dequeue();
            }

            var tcs = new TaskCompletionSource<T>();
            lock (_waitingQueueLock)
                _waitingQueue.Enqueue(tcs);

            ProcessWaitingQueue();
            return tcs.Task.IsCompleted ? tcs.Task.Result : await tcs.Task.ConfigureAwait(false);
        }

        private void ProcessWaitingQueue()
        {
            TaskCompletionSource<T> tcs = null;
            var item = default(T);
            lock (_waitingQueueLock)
                if (_waitingQueue.Any())
                    lock (_syncRoot)
                        if (_innerQueue.Any())
                        {
                            tcs = _waitingQueue.Dequeue();
                            item = _innerQueue.Dequeue();
                        }

            tcs?.SetResult(item);
        }

        #region Implementation of ICollection

        public int Count
        {
            get
            {
                lock (_syncRoot)
                    return _innerQueue.Count;
            }
        }

        public object SyncRoot => _syncRoot;

        public bool IsSynchronized => true;

        #endregion

        #region Implementation of IEnumerable

        public IEnumerator<T> GetEnumerator()
        {
            // just taking snapshot
            lock (_syncRoot)
                return _innerQueue.ToList().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
