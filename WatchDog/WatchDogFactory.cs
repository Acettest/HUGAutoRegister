using System;
using System.Data;
using System.Collections.Generic;
using System.Text;

namespace WatchDog
{
    public class WatchDogFactory : IWatchDogFactory
    {
        #region Singleton Implementation
        private static WatchDogFactory m_Instance = null;
        private static object m_LockSingleton = new int();

        public static WatchDogFactory Instance()
        {
            lock (m_LockSingleton)
            {
                if (m_Instance == null)
                    m_Instance = new WatchDogFactory();

                return m_Instance;
            }
        }

        protected WatchDogFactory()
        {
        }
        #endregion

        Dictionary<string, Dictionary<string, WatchDog>> m_Dogs = new Dictionary<string, Dictionary<string, WatchDog>>();
        string m_TableName;
        string m_DBStr;

        #region IWatchDogFactory 成员
        public void Init(string dbconn, string tablename)
        {
            lock (m_Dogs)
                m_Dogs.Clear();

            m_DBStr = dbconn;
            m_TableName = tablename;
        }

        public IWatchDog CreateWatchDog(string host, string component, int refresh_interval, int timeout)
        {
            WatchDog dog = new WatchDog(m_DBStr, m_TableName, host, component, refresh_interval, timeout);

            lock (m_Dogs)
            {
                if (!m_Dogs.ContainsKey(host))
                    m_Dogs.Add(host, new Dictionary<string, WatchDog>());
                
                if (m_Dogs[host].ContainsKey(component))
                    throw new Exception(System.String.Format("WatchDog: {0}:{1} already exists.", host, component));

                m_Dogs[host][component] = dog;
            }

            return dog;
        }

        public int CreateWatchDogsFromFile(string filename)
        {
            lock (m_Dogs)
            {
                m_Dogs.Clear();

                //获取应用程序运行路径
                string path = System.AppDomain.CurrentDomain.BaseDirectory;
                path = path.Substring(0, path.LastIndexOf("\\") + 1);
                DataSet DBds = new DataSet();

                //读入数据库连接参数
                DBds = MD5Encrypt.DES.instance().DecryptXML2DS(path + filename, 1);
                if (DBds.Tables["SysDB"].Rows.Count == 0)
                {
                    throw new Exception("未定义数据库连接，请检查配置文件");
                }

                Dictionary<string, string> sysdb = new Dictionary<string, string>();
                Dictionary<string, string> paras = new Dictionary<string, string>();

                foreach (DataRow r in DBds.Tables["SysDB"].Rows)
                {
                    string conn = "Persist Security Info=False;";
                    conn += "server=" + r["hostname"].ToString() + ";";
                    conn += "user id=" + r["username"] + ";";
                    conn += "pwd =" + r["userpass"].ToString() + ";";
                    conn += "database=" + r["database"].ToString();
                    sysdb[r["dbname"].ToString()] = conn;
                }

                if (!sysdb.ContainsKey("WatchDogDB"))
                {
                    throw new Exception("未定义系统数据库, 请检查配置文件");
                }

                foreach (DataRow r in DBds.Tables["Parameters"].Rows)
                {
                    paras.Add(r["paraname"].ToString(), r["value"].ToString());
                }

                m_DBStr = sysdb["WatchDogDB"].ToString();
                m_TableName = paras["WatchDog Table"];

                foreach (DataRow r in DBds.Tables["HostDB"].Rows)
                {
                    IWatchDog dog = CreateWatchDog(
                        r["host"].ToString(), r["component"].ToString(), Convert.ToInt32(r["refresh_interval"]),
                        Convert.ToInt32(r["timeout"]));

                    m_Dogs[dog.Host][dog.Component] = dog as WatchDog;
                }

                return m_Dogs.Count;
            }
        }

        /// <summary>
        /// 检查HOST下所有component的dog状态，遇到第一个非GOOD即返回
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public DogStatus Check(string host)
        {
            lock (m_Dogs)
            {
                if (!m_Dogs.ContainsKey(host))
                    return DogStatus.GOOD;
                    //throw new Exception("WatchDog Host: " + host + " not configured.");

                Dictionary<string, WatchDog> component_dogs = m_Dogs[host];
                foreach (KeyValuePair<string, WatchDog> pair in component_dogs)
                {
                    DogStatus status = pair.Value.Check();
                    if (status != DogStatus.GOOD)
                        return status;
                }

                return DogStatus.GOOD;
            }
        }

        /// <summary>
        /// 当Check狗发生异常，喂狗进程被外部重启后，为避免狗数据异常导致新起进程未来得及喂狗即被再次重启，
        /// 可以调用Update方法来更新喂狗时间。
        /// </summary>
        /// <param name="host"></param>
        public void Update(string host)
        {
            lock (m_Dogs)
            {
                if (!m_Dogs.ContainsKey(host))
                    return;

                Dictionary<string, WatchDog> component_dogs = m_Dogs[host];
                foreach (KeyValuePair<string, WatchDog> pair in component_dogs)
                {
                    pair.Value.Feed();
                }
            }
        }

        public IWatchDog GetWatchDog(string host, string component)
        {
            lock (m_Dogs)
            {
                if (!m_Dogs.ContainsKey(host))
                    return null;

                if (!m_Dogs[host].ContainsKey(component))
                    return null;
                else
                    return m_Dogs[host][component];
            }
        }

        #endregion
    }
}
