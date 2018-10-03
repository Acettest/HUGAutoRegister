using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace TK_AlarmManagement
{
    /// <summary>
    /// 数据入库前临时存储的辅助类
    /// </summary>
    public class TempStorageHelper : ITempStorageHelper
    {
        #region Singleton Implementation
        private static ITempStorageHelper m_instance = null;
        private static object m_LockSingleton = new int();
        public static ITempStorageHelper Instance()
        {
            lock (m_LockSingleton)
            {
                if (m_instance == null)
                    m_instance = new TempStorageHelper();
            }

            return m_instance;
        }
        #endregion

        string m_WorkPath = "";

        #region ITempStorageHelper 成员

        public void Init()
        {
            m_WorkPath = AppDomain.CurrentDomain.BaseDirectory + "tempstorage" + Path.DirectorySeparatorChar;

            if (!Directory.Exists(m_WorkPath))
                Directory.CreateDirectory(m_WorkPath);
        }

        public void Store<T>(long key, T o)
        {
            lock (this)
            {
                string fn = m_WorkPath + o.GetType().FullName + "_" + key;
                using (FileStream fw = new FileStream(fn, FileMode.Create))
                {
                    BinaryFormatter f = new BinaryFormatter();
                    f.Serialize(fw, o);
                    fw.Close();
                }
            }
        }

        public List<T> RestoreAllAndClear<T>()
        {
            lock (this)
            {
                List<T> result = new List<T>();

                string[] files = Directory.GetFiles(m_WorkPath);
                string prefix = typeof(T).FullName + "_";
                foreach (string fn in files)
                {
                    FileInfo info = new FileInfo(fn);
                    if (info.Name.StartsWith(prefix))
                    {
                        using (FileStream fr = new FileStream(fn, FileMode.Open))
                        {
                            BinaryFormatter f = new BinaryFormatter();
                            T o = (T)f.Deserialize(fr);
                            result.Add(o);
                        }

                        File.Delete(fn);
                    }
                }

                return result;
            }
        }

        public void Clear<T>(long key)
        {
            lock (this)
            {
                string fn = m_WorkPath + typeof(T).FullName + "_" + key;
                File.Delete(fn);
            }
        }
        #endregion
    }
}
