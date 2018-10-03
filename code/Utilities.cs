using System;
using System.Text;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using TK_AlarmManagement;
using System.Collections.Generic;
using DefLib.Util;

namespace TK_AlarmManagement
{
	/// <summary>
	/// Form_Query 的摘要说明。
	/// </summary>
	/// 

	public class Utilities
	{
		public static string ExecutePath;
		public static Hashtable DBConnections = new Hashtable();
		public static Hashtable SysPara = new Hashtable();
        /// <summary>
        /// 集中告警表
        /// </summary>
        public static DataTable m_AlarmDataTable = new DataTable();

        /// <summary>
        /// 遍历告警表结构
        /// </summary>
        public static void ReadAlarmConf()
        {
            m_AlarmDataTable.Columns.Clear();
            System.Reflection.FieldInfo[] fieldinfo = typeof(TKAlarm).GetFields();
            foreach (System.Reflection.FieldInfo info in fieldinfo)
            {
                if (info.Name != "SyncRoot")
                    m_AlarmDataTable.Columns.Add(info.Name);
            }
            m_AlarmDataTable.Columns.Add("APHotArea");
        }

		public static string convert2DBStr(string src)
		{
			return src.Replace("'", "''");
		}
	
		public static string restrictLen(string str, int byteLen)
		{
			System.Text.Encoding  encoding = System.Text.Encoding.Default;
			byte[] bytes = encoding.GetBytes(str);

			if (bytes.Length > byteLen)
			{
				return encoding.GetString(bytes, 0, byteLen);
			}

			return str;
		}

		public static string ConvertStrArr2String(ArrayList ar)
		{
			if (ar == null)
				return "";

			string result = "";
			foreach (object o in ar)
			{
				string s = o.ToString();
				result += ("'" + s + "',");
			}
			if (result != "")
			{
				result = result.Remove(result.Length - 1, 1);
			}

			return result;
		}

		public static ArrayList ConvertString2StrArr(string s)
		{
			ArrayList ar = new ArrayList();
			if (s.Trim() == "")
				return ar;
			
			string[] subs = s.Split(',');
			foreach (string sub in subs)
			{
				int i1 = sub.IndexOf('\'');
				int i2 = sub.LastIndexOf('\'');
				if (i2 < i1 + 1)
					throw new Exception("字符串格式转换失败:" + s.ToString());

				ar.Add(sub.Substring(i1 + 1, i2 - i1 - 1));
			}

			return ar;
        }

        #region 读取配置文件
        public static Dictionary<string, NorthServer> m_NorthServers = new Dictionary<string, NorthServer>();

        /// <summary>
        /// 读取北向服务器配置
        /// </summary>
        public static void ReadNorthServerConf()
        {
            //获取当前路径
            string sPath = System.AppDomain.CurrentDomain.BaseDirectory;
            sPath = sPath.Substring(0, sPath.LastIndexOf(Path.DirectorySeparatorChar) + 1);

            //读取保存在dbconnection.xml中的登陆连接信息
            DataSet xmlds = new DataSet();
            xmlds.ReadXml(sPath + Path.DirectorySeparatorChar + "NorthServerConf.xml");

            if (xmlds.Tables["NorthServerInfo"].Rows.Count == 0)
            {
                Logger.Instance().SendLog("BL", "未定义北向服务器连接信息");
                return;
            }

            m_NorthServers.Clear();

            //读取NorthServer配置
            foreach (DataRow dr in xmlds.Tables["NorthServerInfo"].Rows)
            {
                if (!m_NorthServers.ContainsKey(dr["name"].ToString()))
                {
                    NorthServer ns = new NorthServer();
                    ns.servername = dr["name"].ToString();
                    ns.serverip = dr["ip"].ToString();
                    ns.port = dr["port"].ToString();
                    m_NorthServers.Add(ns.servername, ns);
                }
            }
        }
        #endregion

        
	}
}
