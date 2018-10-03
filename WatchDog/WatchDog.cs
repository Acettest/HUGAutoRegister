using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Microsoft.ApplicationBlocks.Data;
#if MYSQL
using MySql.Data.MySqlClient;
#else
using System.Data.SqlClient;
#endif


namespace WatchDog
{
    public class WatchDog : IWatchDog
    {
        #region 构造函数
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbconn_str"></param>
        /// <param name="tablename"></param>
        /// <param name="host"></param>
        /// <param name="component"></param>
        /// <param name="refresh_interval">毫秒为单位</param>
        /// <param name="timeout"></param>
        public WatchDog(string dbconn_str, string tablename, string host, string component, int refresh_interval, int timeout)
        {
            m_DBString = dbconn_str;
            m_TableName = tablename;
            m_Host = host;
            m_Component = component;
            m_RefreshInterval = refresh_interval;
            m_TimeoutPeriod = timeout;
            m_Status = DogStatus.GOOD;
        }
        #endregion

        #region 私有成员
        private string m_Host, m_Component, m_DBString, m_TableName;
        private int m_RefreshInterval; // 秒
        private int m_TimeoutPeriod;
        DogStatus m_Status;
        #endregion

        #region IWatchDog 成员

        public void Feed()
        {
#if MYSQL
            MySqlConnection conn = new MySqlConnection(m_DBString);
#else
            SqlConnection conn = new SqlConnection(m_DBString);
#endif

            try
            {
                DataSet ds = new DataSet();

                string q = "select * from " + m_TableName;
                q += " where host='" + m_Host.Trim() + "'";
                q += " and component='" + m_Component.Trim() + "'";

                SqlHelper.FillDataset(conn, CommandType.Text, q, ds, new string[] { "watchdog" });
                if (ds.Tables["watchdog"].Rows.Count == 0)
                {
                    q = "insert into " + m_TableName;
                    q += " (host, component, update_time, check_time, refresh_interval, pid)";
                    q += String.Format(" values ('{0}','{1}','{2}','{3}',{4},{5})",
                        m_Host.Trim(), m_Component.Trim(), DateTime.Now.ToString(), DateTime.Now.ToString(), m_RefreshInterval, System.Diagnostics.Process.GetCurrentProcess().Id);

                    SqlHelper.ExecuteNonQuery(conn, CommandType.Text, q);
                }
                else
                {
                    q = "update " + m_TableName + " set update_time='" + DateTime.Now.ToString() + "',";
                    q += " check_time='" + DateTime.Now.ToString() + "',";
                    q += " pid=" + System.Diagnostics.Process.GetCurrentProcess().Id.ToString();
                    q += " where host='" + m_Host.Trim() + "' and component='" + m_Component.Trim() + "'";

                    SqlHelper.ExecuteNonQuery(conn, CommandType.Text, q);
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                conn.Close();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>如果此次狗正常，则返回GOOD；否则，{如果上次已非GOOD状态的狗，返回状态为STLL_BAD；否则返回BAD}</returns>
        public DogStatus Check()
        {
#if MYSQL
            MySqlConnection conn = new MySqlConnection(m_DBString);
#else
            SqlConnection conn = new SqlConnection(m_DBString);
#endif

            try
            {
                DataSet ds = new DataSet();

                string q = "select update_time from " + m_TableName;
                q += " where host='" + m_Host.Trim() + "'";
                q += " and component='" + m_Component.Trim() + "'";

                SqlHelper.FillDataset(conn, CommandType.Text, q, ds, new string[] { "watchdog" });
                if (ds.Tables["watchdog"].Rows.Count > 0)
                {
                    DateTime checktime = DateTime.Now;
                    DateTime updatetime = Convert.ToDateTime(ds.Tables["watchdog"].Rows[0]["update_time"]);
                    TimeSpan span = checktime - updatetime;

                    //int interval = Convert.ToInt16(ds.Tables["watchdog"].Rows[0]["refresh_interval"]);
                    //int timeout = Convert.ToInt16(ds.Tables["watchdog"].Rows[0]["timeout"]);

                    if (span.TotalMilliseconds > m_RefreshInterval * m_TimeoutPeriod)
                    {
                        m_Status = (m_Status != DogStatus.GOOD) ? DogStatus.STILL_BAD : DogStatus.BAD;
                    }
                    else
                    {
                        m_Status = DogStatus.GOOD;
                    }

                    q = "update " + m_TableName + " set check_time='" + checktime + "'";
                    q += " where host='" + m_Host.Trim() + "' and component='" + m_Component.Trim() + "'";

                    try
                    {
                        SqlHelper.ExecuteNonQuery(conn, CommandType.Text, q);
                    }
                    catch
                    { }
                }
                else
                {
                    m_Status = (m_Status != DogStatus.GOOD) ? DogStatus.STILL_BAD : DogStatus.BAD;
                }

                return m_Status;
            }
            catch
            {
                throw;
            }
            finally
            {
                conn.Close();
            }
        }

        public DogStatus Status
        {
            get { return m_Status; }
        }

        public string Host
        {
            get { return m_Host; }
        }

        public string Component
        {
            get { return m_Component; }
        }
        #endregion

        #region IComparable<IWatchDog> 成员

        public int CompareTo(IWatchDog other)
        {
            int i = m_Host.CompareTo(other.Host);
            if (i < 0)
                return -1;
            else if (i > 0)
                return 1;

            i = m_Component.CompareTo(other.Component);
            if (i < 0)
                return -1;
            else if (i > 0)
                return 1;
            else
                return 0;
        }

        #endregion
    }
}
