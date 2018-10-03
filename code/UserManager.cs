using System;
using System.Collections.Generic;
using System.Collections;
using System.Threading;

//using MySql.Data.MySqlClient;
using Microsoft.ApplicationBlocks.Data;
using System.Data;
using DefLib.Util;

namespace TK_AlarmManagement
{
	/// <summary>
	/// UserManager 的摘要说明。
	/// </summary>
	public class UserManager: ICommandHandler
	{
		private long bRun = 0;

		/// <summary>
		/// 用户信息数据库连接
		/// </summary>
		private string m_ConnStr;

        #region Singleton Implementation
        private static object m_Singleton = new int();
        private static UserManager _usermanager = null;
		public static UserManager instance()
		{
            lock (m_Singleton)
            {
                if (_usermanager == null)
                    _usermanager = new UserManager();

                return _usermanager;
            }
		}

		protected UserManager()
		{
			ReadDBConfig();
        }
        #endregion

        public string ConnString
        {
            get { return m_ConnStr; }
        }

		#region ICommandHandler 成员

		public void handleCommand(ICommunicationMessage message)
        {
            CommandMsgV2 responseMsg = null;
            switch (message.TK_CommandType)
            {
                case Constants.TK_CommandType.LOGIN:
                    responseMsg = UserLogin(message);
                    break;
                case Constants.TK_CommandType.LOGOUT:
                    responseMsg = UserLogout(message);
                    break;
                default:
                    break;
            }

            CommandProcessor.instance().DispatchCommand(responseMsg);
		}

		#endregion

        #region 启停接口
        private object m_LockRun = new int();
		public void Start()
		{
            lock (m_LockRun)
            {
                if (Interlocked.Read(ref bRun) == 1)
                    return;
                else
                    Interlocked.Exchange(ref bRun, 1);

                Dictionary<Constants.TK_CommandType, byte> super_commands = new Dictionary<Constants.TK_CommandType, byte>();
                CommandProcessor.instance().registerReportHandler(Constants.TK_CommandType.LOGIN, this, super_commands);
                CommandProcessor.instance().registerReportHandler(Constants.TK_CommandType.LOGOUT, this, super_commands);
            }
		}

		public void Stop()
		{
            lock (m_LockRun)
            {
                if (Interlocked.Read(ref bRun) == 1)
                    Interlocked.Exchange(ref bRun, 0);
                else
                    return;

                CommandProcessor.instance().unregisterReportHandler(Constants.TK_CommandType.LOGIN, this);
                CommandProcessor.instance().unregisterReportHandler(Constants.TK_CommandType.LOGOUT, this);
            }
        }
        #endregion

		private CommandMsgV2 UserLogin(ICommunicationMessage message)
		{
			CommandMsgV2 responseMsg = new CommandMsgV2();

			try
			{
				if(message.Contains("ClientID"))
				{
					if(message.Contains("用户名") && message.Contains("密码"))
					{
						string sUserName = message.GetValue("用户名").ToString().Trim();
						string sPassword = message.GetValue("密码").ToString().Trim();

						string sQuery = "select manage,company from Operator where valid=1 and login_name = '" + sUserName + "' and password = '" + sPassword + "'";
						DataSet ds;
                        ds = SqlHelper.ExecuteDataset(m_ConnStr, CommandType.Text, sQuery);

						if (ds.Tables[0].Rows.Count == 1 )
						{
							//用户名、密码正确
							object[] objs = ds.Tables[0].Rows[0].ItemArray;
							string sRight = objs[0].ToString();
                            string sCompany = ds.Tables[0].Rows[0]["company"].ToString();

							//查询用户可管理的业务类型
							sQuery = "select businesstype from Operator_BusinessType where login_name = '" + sUserName + "'";
							ds.Tables.Clear();

                            ds = SqlHelper.ExecuteDataset(m_ConnStr, CommandType.Text, sQuery);

							string sFilter = "";
							foreach(DataRow dr in ds.Tables[0].Rows)
							{
								object[] temps = dr.ItemArray;
								sFilter += temps[0].ToString().Trim() + ",";
							}

							#region 先发命令给CM，通知客户端登陆成功
							CommandMsgV2 MsgLogOK = new CommandMsgV2();
							MsgLogOK.TK_CommandType = Constants.TK_CommandType.RESPONSE;
                            MsgLogOK.SeqID = CommandProcessor.AllocateID();
							MsgLogOK.SetValue("ClientID", message.GetValue("ClientID"));
							MsgLogOK.SetValue("RESPONSE_TO", message.SeqID);
							MsgLogOK.SetValue("RESULT", "OK");
							MsgLogOK.SetValue("BUSINESSTYPE", sFilter.Trim());
                            MsgLogOK.SetValue("RIGHT", sRight.Trim());
                            MsgLogOK.SetValue("COMPANY", sCompany.Trim());

							CommandProcessor.instance().DispatchCommand(MsgLogOK);

							#endregion


							//发命令给AM，注册客户端
							responseMsg.SeqID = message.SeqID;
							responseMsg.TK_CommandType = Constants.TK_CommandType.REGISTERCLIENT;
							responseMsg.SetValue("ClientID", message.GetValue("ClientID"));
                            responseMsg.SetValue(Constants.MSG_PARANAME_AUTHORIZED, true);
                            responseMsg.SetValue("SERVERNAME", Constants.ALARM_SERVERNAME);
							responseMsg.SetValue("Filter", sFilter.Trim());
							responseMsg.SetValue("RIGHT", sRight.Trim());
                            responseMsg.SetValue("COMPANY", sCompany.Trim());

							sQuery = "update Operator set lastlogintime = '" + DateTime.Now.ToString() + "' where login_name = '" + sUserName + "'";

                            SqlHelper.ExecuteNonQuery(m_ConnStr, CommandType.Text, sQuery);

                            Logger.Instance().SendLog("UM", "用户:" + sUserName + " 已登录到系统.");
						}
						else
						{
							//登录失败
							responseMsg.TK_CommandType = Constants.TK_CommandType.RESPONSE;
							responseMsg.SetValue("ClientID", message.GetValue("ClientID"));
							responseMsg.SetValue("RESPONSE_TO", message.SeqID);
							responseMsg.SetValue("RESULT", "NOK");

                            Logger.Instance().SendLog("UM", "用户:" + sUserName + " 登录失败."); 
						}
					}
					else
					{
						//登录失败
						responseMsg.TK_CommandType = Constants.TK_CommandType.RESPONSE;
						responseMsg.SetValue("ClientID", message.GetValue("ClientID"));
						responseMsg.SetValue("RESPONSE_TO", message.SeqID);
						responseMsg.SetValue("RESULT", "NOK");

                        Logger.Instance().SendLog("UM", "无效登录包");
					}
				}
				else
				{
					return null;
				}
			}
			catch(Exception ex)
			{
                try
                {
                    //登录失败
                    responseMsg.TK_CommandType = Constants.TK_CommandType.RESPONSE;
                    responseMsg.SetValue("ClientID", message.GetValue("ClientID"));
                    responseMsg.SetValue("RESPONSE_TO", message.SeqID);
                    responseMsg.SetValue("RESULT", "NOK");

                    Logger.Instance().SendLog("UM", "登录时出现异常:" + ex.ToString());
                }
                catch { }
			}
			finally
			{
			}
			return responseMsg;
			
		}

		private CommandMsgV2 UserLogout(ICommunicationMessage message)
		{
			CommandMsgV2 responseMsg = new CommandMsgV2();

			try
			{
				if(message.Contains("ClientID"))
				{
					if(message.Contains("用户名"))
					{
						string sUserName = message.GetValue("用户名").ToString().Trim();
						string sql = "update Operator set lastlogouttime = '" + DateTime.Now.ToString().Trim() + "' where login_name = '" + sUserName + "'";
                        SqlHelper.ExecuteNonQuery(m_ConnStr, CommandType.Text, sql);

                        // 注销内部客户端的数据
                        CommandMsgV2 logout = new CommandMsgV2();
                        logout.TK_CommandType = Constants.TK_CommandType.UNREGISTERCLIENT;
                        logout.SeqID = CommandProcessor.AllocateID();
                        logout.SetValue("ClientID", message.GetValue("ClientID"));
                        CommandProcessor.instance().DispatchCommand(logout);

                        responseMsg.SeqID = CommandProcessor.AllocateID();
                        responseMsg.TK_CommandType = Constants.TK_CommandType.RESPONSE;
                        responseMsg.SetValue("ClientID", message.GetValue("ClientID"));
                        responseMsg.SetValue("RESPONSE_TO", message.SeqID);
                        responseMsg.SetValue("RESULT", "OK");
                    }
					else
					{
						responseMsg.TK_CommandType = Constants.TK_CommandType.RESPONSE;
                        responseMsg.SeqID = CommandProcessor.AllocateID();
						responseMsg.SetValue("ClientID", message.GetValue("ClientID"));
						responseMsg.SetValue("RESPONSE_TO", message.SeqID);
						responseMsg.SetValue("RESULT", "NOK");
					}
				}
				else
				{
					return null;
				}
			}
			catch
			{
                try
                {
                    responseMsg.TK_CommandType = Constants.TK_CommandType.RESPONSE;
                    responseMsg.SeqID = CommandProcessor.AllocateID();
                    responseMsg.SetValue("ClientID", message.GetValue("ClientID"));
                    responseMsg.SetValue("RESPONSE_TO", message.SeqID);
                    responseMsg.SetValue("RESULT", "NOK");
                }
                catch { }
			}
			finally
			{	
			}
			return responseMsg;
			
		}

		/// <summary>
		/// 读取用户信息数据库配置文件
		/// </summary>
		public void ReadDBConfig()
		{
			//获取当前路径
            string sPath = AppDomain.CurrentDomain.BaseDirectory;

			//读取保存在dbconnection.xml中的登陆连接信息
			DataSet xmlds = new DataSet();

            xmlds.ReadXml(sPath + "dbconnection.xml");

			if (xmlds.Tables["UserDB"].Rows.Count == 0)
			{
				Logger.Instance().SendLog("UM", "未定义登陆数据库连接");
				return;
			}

			//将xml文件中的连接信息转换成连接字符串
			m_ConnStr =  "Persist Security Info=False;";
			m_ConnStr = m_ConnStr + "User ID=" + xmlds.Tables["UserDB"].Rows[0]["username"].ToString() + ";";
			m_ConnStr = m_ConnStr + "pwd =" + xmlds.Tables["UserDB"].Rows[0]["userpass"].ToString() + ";";
			m_ConnStr = m_ConnStr + "Initial Catalog=" + xmlds.Tables["UserDB"].Rows[0]["database"].ToString() + ";";
			m_ConnStr = m_ConnStr + "Data Source=" + xmlds.Tables["UserDB"].Rows[0]["hostname"].ToString();
#if MYSQL
            m_ConnStr += "; character set=utf8";
#endif
		}

	}
}
