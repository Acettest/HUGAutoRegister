using System;
using System.Collections.Generic;
using System.Text;

namespace DefLib.Util
{
    public interface IPoolableObject<T>
        where T : IPoolableObject<T>, new()
    {
        ObjectPool<T>.PoolState StateInPool { get; set; }
        void InitializeByPool();
        void CleanUpByPool();
    }

    public class ObjectPool<T> : IDisposable 
        where T : IPoolableObject<T>, new ()
    {
        public enum PoolState
        {
            Idle,
            InUse
        }

        private static ObjectPool<T> m_Instance = null;
        private static object m_SingletonLock = new object();
        public static ObjectPool<T> Instance
        {
            get
            {
                lock (m_SingletonLock)
                    if (m_Instance == null)
                        m_Instance = new ObjectPool<T>();
                return m_Instance;
            }
        }

        protected ObjectPool()
        {
        }

        private int m_ObjectsCount = 0;
        public int ObjectsCount
        {
            get { return m_ObjectsCount; }
        }

        private int m_HitCount = 0;
        public int HitCount
        {
            get { return m_HitCount; }
        }

        private DefLib.Util.ArrayBuilder<T> m_Pool = new ArrayBuilder<T>();
        public T Get()
        {
            T o;
            lock (m_Pool)
            {
                if (m_Pool.Length == 0)
                {
                    o = new T();
                }
                else
                {
                    o = m_Pool[m_Pool.Length - 1];//.Last();//.First();
                    m_Pool.RemoveInterval(m_Pool.Length - 1, 1);//.RemoveAt(m_Pool.Count - 1);

                    ++m_HitCount;
                }

                ++m_ObjectsCount;
            }

            if (o.StateInPool != PoolState.Idle)
                throw new Exception("unidle objects in pool.");

            o.InitializeByPool();
            o.StateInPool = PoolState.InUse;
            return o;
        }

        public void PutBack(T o)
        {
            lock (m_Pool)
            {
                o.CleanUpByPool();
                m_Pool.Append(o);//.Add(o);
                o.StateInPool = PoolState.Idle;

                --m_ObjectsCount;
            }
        }

        public void PutBackRange(IEnumerable<T> objects)
        {
            lock (m_Pool)
            {
                foreach (T o in objects)
                {
                    o.CleanUpByPool();
                    m_Pool.Append(o);//.Add(o);
                    o.StateInPool = PoolState.Idle;

                    --m_ObjectsCount;
                }
            }
        }

        public void PutBackRange(IEnumerable<T> objects, int count)
        {
            int c = 0;
            lock (m_Pool)
            {
                foreach (T o in objects)
                {
                    o.CleanUpByPool();
                    m_Pool.Append(o);//.Add(o);
                    o.StateInPool = PoolState.Idle;

                    --m_ObjectsCount;
                    if (++c == count) break;
                }
            }
        }

        #region IDisposable 成员

        public void Dispose()
        {
            lock (m_Pool)
            {
                for (int i = 0; i < m_Pool.Length; ++i)
                    m_Pool[i].CleanUpByPool();
                //m_Pool.ForEach(delegate(T o) { o.CleanUpByPool(); });
                m_Pool.Clear();
                m_ObjectsCount = 0;
            }
        }

        #endregion
    }
}
