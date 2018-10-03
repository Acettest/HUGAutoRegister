using System;
using System.Collections.Generic;
using System.Text;

namespace DefLib.Util
{
    [Serializable]
    public class ArrayBuilder<T> : IEnumerable<T>
        where T : new()
    {
        static int m_InitialCapacity = 1000;
        static int m_AllocStep = 1000;
        T[] m_FinalData = null;
        T[] m_Temp1 = null;
        int m_Length = 0;
        int m_CurrentCapacity = 0;
        public ArrayBuilder()
        {
            m_FinalData = new T[m_InitialCapacity];
            m_CurrentCapacity = m_InitialCapacity;

        }

        public ArrayBuilder(int initialcapacity)
        {
            if (initialcapacity < 0)
                throw new Exception("非法的初始容量（应大于等于零）");

            m_FinalData = new T[initialcapacity];
            m_CurrentCapacity = initialcapacity;
        }

        public int Length
        {
            get
            {
                return m_Length;
            }
        }

        private void _AdjustCapacity(int length)
        {
            if (length > m_CurrentCapacity)
            {
                while (length > m_CurrentCapacity)
                {
                    m_CurrentCapacity += m_AllocStep;
                }

                m_Temp1 = new T[m_CurrentCapacity];
                Array.Copy(m_FinalData, m_Temp1, m_Length);
                m_FinalData = m_Temp1;
            }
        }

        public void Clear()
        {
            m_Length = 0;
        }

        public T this[int i]
        {
            get { return m_FinalData[i]; }
            set { m_FinalData[i] = value; }
        }

        public void Append(T c)
        {
            _AdjustCapacity(Length + 1);
            m_FinalData[Length] = c;
            ++m_Length;
        }

        public void Append(T[] m, int count)
        {
            Append(m, 0, count);
        }

        public void Append(T[] m, int start_idx, int count)
        {
            if (count> m.Length)
                throw new Exception("Count超过了数组的长度");

            _AdjustCapacity(Length + count);
            Array.Copy(m, start_idx, m_FinalData, Length, count);
            m_Length += count;
        }

        public void Insert(int idx_before, T c)
        {
            _AdjustCapacity(Length + 1);
            m_Temp1 = new T[m_CurrentCapacity];

            if (idx_before <= 0)
            {
                m_Temp1[0] = c;
                Array.Copy(m_FinalData, 0, m_Temp1, 1, Length);
            }
            else
            {
                Array.Copy(m_FinalData, 0, m_Temp1, 0, idx_before);
                m_Temp1[idx_before] = c;
                Array.Copy(m_FinalData, idx_before, m_Temp1, idx_before + 1, m_Length - idx_before);
            }

            m_FinalData = m_Temp1;
            ++m_Length;
        }

        public void Insert(int idx_before, T[] m, int count)
        {
            if (count > m.Length)
                throw new Exception("Count超过了数组的长度");

            _AdjustCapacity(Length +count);
            m_Temp1 = new T[m_CurrentCapacity];

            if (idx_before <= 0)
            {
                Array.Copy(m, m_Temp1, count);
                Array.Copy(m_FinalData, 0, m_Temp1, count, Length);
            }
            else
            {
                Array.Copy(m_FinalData, 0, m_Temp1, 0, idx_before);
                Array.Copy(m, 0, m_Temp1, idx_before, count);
                Array.Copy(m_FinalData, idx_before, m_Temp1, idx_before + count, m_Length - idx_before);
            }

            m_FinalData = m_Temp1;
            m_Length += count;
        }

        public void RemoveInterval(int start_idx, int count)
        {
            if (count == 0) return;

            if (start_idx + count > Length)
                throw new Exception("移除元素超出了数组范围");

            if (start_idx + count < Length)
                Array.Copy(m_FinalData, start_idx + count, m_FinalData, start_idx, Length - count - start_idx);

            // 释放对删除元素的引用
            for (int i = m_Length - count; i < m_Length; ++i)
                m_FinalData[i] = default(T);

            m_Length -= count;
        }

        public T[] ToArray()
        {
            T[] r = new T[m_Length];
            Array.Copy(m_FinalData, r, m_Length);
            return r;
        }

        /// <summary>
        /// 返回原始数组（没有数组拷贝），访问时注意使用Length限制长度边界
        /// </summary>
        /// <returns></returns>
        public T[] ToNativeArray()
        {
            return m_FinalData;
        }

        #region IEnumerable<T> 成员

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Length; ++i)
                yield return m_FinalData[i];
        }

        #endregion

        #region IEnumerable 成员

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
