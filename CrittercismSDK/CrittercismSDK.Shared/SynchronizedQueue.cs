using System;
using System.Collections.Generic;
using System.Linq;

namespace CrittercismSDK {
    public class SynchronizedQueue<T> {
        private Queue<T> _q;
        public bool IsSynchronized {
            get {
                return true;
            }
        }
        public object SyncRoot {
            get {
                return this._q;
            }
        }
        public int Count {
            get {
                lock (this._q)
                    return this._q.Count;
            }
        }
        internal SynchronizedQueue(Queue<T> q) {
            this._q=q;
        }
        public void Clear() {
            lock (this._q)
                this._q.Clear();
        }
        public bool Contains(T obj) {
            lock (this._q)
                return this._q.Contains(obj);
        }
        public void CopyTo(T[] array,int arrayIndex) {
            lock (this._q)
                this._q.CopyTo(array,arrayIndex);
        }
        public void Enqueue(T value) {
            ////////////////////////////////////////////////////////////////
            // NOTE: MSDN doc excerpt re Queue<T>.Enqueue Method 
            // "Adds an object to the end of the Queue<T>."
            // https://msdn.microsoft.com/en-us/library/t249c2y7(v=vs.110).aspx
            ////////////////////////////////////////////////////////////////
            lock (this._q)
                this._q.Enqueue(value);
        }
        public T Dequeue() {
            ////////////////////////////////////////////////////////////////
            // NOTE: MSDN doc excerpt re Queue<T>.Dequeue Method
            // "Removes and returns the object at the beginning of the Queue<T>."
            // InvalidOperationException if The Queue<T> is empty.
            // https://msdn.microsoft.com/en-us/library/1c8bzx97(v=vs.110).aspx
            ////////////////////////////////////////////////////////////////
            lock (this._q)
                return this._q.Dequeue();
        }
        public IEnumerator<T> GetEnumerator() {
            lock (this._q)
                return this._q.GetEnumerator();
        }
        public T Peek() {
            ////////////////////////////////////////////////////////////////
            // NOTE: MSDN doc excerpt re Queue<T>.Peek Method 
            // "Returns the object at the beginning of the Queue<T> without
            // removing it."
            // https://msdn.microsoft.com/en-us/library/1cz28y5c(v=vs.110).aspx
            ////////////////////////////////////////////////////////////////
            lock (this._q)
                return this._q.Peek();
        }
        public T[] ToArray() {
            ////////////////////////////////////////////////////////////////
            // NOTE: MSDN doc excerpt re Queue<T>.ToArray Method 
            // "The Queue<T> is not modified. The order of the elements in the new
            // array is the same as the order of the elements from the beginning
            // of the Queue<T> to its end. This method is an O(n) operation, where
            // n is Count."
            // https://msdn.microsoft.com/en-us/library/0d49dexz(v=vs.110).aspx
            ////////////////////////////////////////////////////////////////
            lock (this._q)
                return this._q.ToArray();
        }
        public List<T> ToList() {
            ////////////////////////////////////////////////////////////////
            // NOTE: MSDN doc excerpt re Queue<T>.ToList Method 
            // "Creates a List<T> from an IEnumerable<T>.(Defined by Enumerable.)"
            // https://msdn.microsoft.com/en-us/library/0d49dexz(v=vs.110).aspx
            ////////////////////////////////////////////////////////////////
            lock (this._q)
                return this._q.ToList();
        }
        public T Last() {
            lock (this._q)
                return this._q.Last();
        }
    }
}

