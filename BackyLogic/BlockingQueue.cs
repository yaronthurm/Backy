using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BackyLogic
{
    public class BlockingQueue<T>
    {
        private Queue<T> _queue = new Queue<T>();
        private object _locker = new object();

        public T Dequeue()
        {
            lock (_locker)
            {
                while (_queue.Count == 0)
                    Monitor.Wait(_locker);
                var ret = _queue.Dequeue();
                return ret;
            }
        }

        public GetOrTimeoutResult<T> Dequeue(int milliseconds)
        {
            lock (_locker)
            {
                var sw = Stopwatch.StartNew();
                int timeTillTimeout = (int)(milliseconds - sw.ElapsedMilliseconds);
                while (_queue.Count == 0 && timeTillTimeout > 0)
                {
                    Monitor.Wait(_locker, timeTillTimeout);
                    timeTillTimeout = (int)(milliseconds - sw.ElapsedMilliseconds);
                }
                sw.Stop();

                if (_queue.Count == 0) // Woke up due to timeout
                {
                    return new GetOrTimeoutResult<T> { Timedout = true, Value = default(T) };
                }
                else // Woke up due to signal
                {
                    return new GetOrTimeoutResult<T> { Timedout = false, Value = _queue.Dequeue() };
                }                
            }
        }

        public Task<GetOrTimeoutResult<T>> DequeueAsync(int milliseconds)
        {
            var ret = Task.Run(() => this.Dequeue(milliseconds));
            return ret;
        }

        public void Add(T item)
        {
            lock (_locker)
            {
                _queue.Enqueue(item);
                Monitor.Pulse(_locker); // Signal anyone who waits for an item
            }
        }

        public void Clear()
        {
            lock (_locker)
            {
                _queue.Clear();
            }
        }
    }

    public class GetOrTimeoutResult<T>
    {
        public bool Timedout;
        public T Value;
    }
}
