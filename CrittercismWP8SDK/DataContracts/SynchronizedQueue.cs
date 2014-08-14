using System;
using System.Collections.Generic;
using System.Linq;

namespace CrittercismSDK.DataContracts
{
    public class SynchronizedQueue<T>
    {
        private Queue<T> _q;
        public bool IsSynchronized
        {
            get
            {
                return true;
            }
        }
        public object SyncRoot
        {
            get
            {
                return this._q;
            }
        }
        // Unfortunately, we can't "override" the Queue<T>'s own "Count".
        // Just remember to use "SafeCount" instead of "Count".
        public int Count
        {
            get
            {
                lock (this._q)
                    return this._q.Count;
            }
        }
        internal SynchronizedQueue(Queue<T> q)
        {
            this._q = q;
        }
        public void Clear()
        {
            lock (this._q)
                this._q.Clear();
        }
        public bool Contains(T obj)
        {
            lock (this._q)
                return this._q.Contains(obj);
        }
        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (this._q)
                this._q.CopyTo(array, arrayIndex);
        }
        public void Enqueue(T value)
        {
            lock (this._q)
                this._q.Enqueue(value);
        }
        public T Dequeue()
        {
            lock (this._q)
                return this._q.Dequeue();
        }
        public IEnumerator<T> GetEnumerator()
        {
            lock (this._q)
                return this._q.GetEnumerator();
        }
        public T Peek()
        {
            lock (this._q)
                return this._q.Peek();
        }
        public T[] ToArray()
        {
            lock (this._q)
                return this._q.ToArray();
        }
        public T Last()
        {
            lock (this._q)
                return this._q.Last();
        }
    }
}

