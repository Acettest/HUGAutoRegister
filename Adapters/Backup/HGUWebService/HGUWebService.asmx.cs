using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Services;
using System.Xml;
using System.IO;
using System.Threading;
using HGU.Idl;
using System.Data;


namespace HGU.WebService
{
	/// <summary>
	/// HGUWebService 的摘要说明
	/// </summary>
	[WebService(Namespace = "http://tempuri.org/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	[System.ComponentModel.ToolboxItem(false)]
	// 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消对下行的注释。
	// [System.Web.Script.Services.ScriptService]
	public class HGUWebService : System.Web.Services.WebService
	{
		private static string m_conn = System.Configuration.ConfigurationManager.ConnectionStrings["HGU"].ConnectionString;

		/// <summary>
		/// 装机工单、拆机工单下发
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		[WebMethod]
		public string SendTask(string xml)
		{
			string result = string.Empty;
			try
			{
				XmlDocument xmldoc = new XmlDocument();
				xmldoc.LoadXml(xml);
				string taskID = xmldoc.DocumentElement.SelectSingleNode("//TaskID").InnerText;
				string taskType = xmldoc.DocumentElement.SelectSingleNode("//TaskType").InnerText;
				string city = xmldoc.DocumentElement.SelectSingleNode("//City").InnerText;
				string manufacturer = xmldoc.DocumentElement.SelectSingleNode("//Manufacturer").InnerText;
				string omcName = xmldoc.DocumentElement.SelectSingleNode("//OMCName").InnerText;
				string oltID = xmldoc.DocumentElement.SelectSingleNode("//OLTID").InnerText;
				string ponID = xmldoc.DocumentElement.SelectSingleNode("//PONID").InnerText;
				string onuID = xmldoc.DocumentElement.SelectSingleNode("//ONUID").InnerText;
				string phone = xmldoc.DocumentElement.SelectSingleNode("//PhoneNumber").InnerText;
				string onuType = null;
				int svlan = 0, cvlan = 0, muvlan = 0, ywvlan = 0, msvlan = 0, mcvlan = 0, feNumber = -1, potsNumber = -1;

				SQLUtil.CheckOMC(m_conn, city, manufacturer, omcName);

				DateTime dt1 = DateTime.Now;
				//入库处理
				result = IdlUtil.ConvertToReslutXML(1, "接收成功");
				TaskType type = IdlUtil.ParseCodeToTaskType(taskType);
				string tName = Enum.GetName(typeof(TaskType), type);
				WriteLog.WriteLine(WriteLog.m_FilePreName, string.Format("{0} {1}", tName, taskID));
				switch (type)
				{
					case TaskType.NewBroadband:
					case TaskType.AddBroadband:
						try
						{
							svlan = int.Parse(xmldoc.DocumentElement.SelectSingleNode("//SVLAN").InnerText);
						}
						catch { }

						try
						{
							cvlan = int.Parse(xmldoc.DocumentElement.SelectSingleNode("//CVLAN").InnerText);
						}
						catch { }

						try
						{
							msvlan = int.Parse(xmldoc.DocumentElement.SelectSingleNode("//MSVLAN").InnerText);
						}
						catch { }

						try
						{
							mcvlan = int.Parse(xmldoc.DocumentElement.SelectSingleNode("//MCVLAN").InnerText);
						}
						catch { }

						try
						{
							feNumber = int.Parse(xmldoc.DocumentElement.SelectSingleNode("//FENUMBER").InnerText);
						}
						catch { }

						try
						{
							potsNumber = int.Parse(xmldoc.DocumentElement.SelectSingleNode("//POTSNUMBER").InnerText);
						}
						catch { }

						Task broadbandTask = new Task(taskID, type, city, manufacturer, omcName, oltID, ponID, onuID, onuType, svlan,
													  cvlan, phone, DateTime.Now, muvlan, ywvlan, msvlan, mcvlan, feNumber, potsNumber);
						SQLUtil.InsertTask(m_conn, broadbandTask);
						break;
					case TaskType.NewIMS:
					case TaskType.AddIMS:
						try
						{
							svlan = int.Parse(xmldoc.DocumentElement.SelectSingleNode("//SVLAN").InnerText);
						}
						catch { }

						try
						{
							cvlan = int.Parse(xmldoc.DocumentElement.SelectSingleNode("//CVLAN").InnerText);
						}
						catch { }

						try
						{
							msvlan = int.Parse(xmldoc.DocumentElement.SelectSingleNode("//MSVLAN").InnerText);
						}
						catch { }

						try
						{
							mcvlan = int.Parse(xmldoc.DocumentElement.SelectSingleNode("//MCVLAN").InnerText);
						}
						catch { }

						try
						{
							feNumber = int.Parse(xmldoc.DocumentElement.SelectSingleNode("//FENUMBER").InnerText);
						}
						catch { }

						try
						{
							potsNumber = int.Parse(xmldoc.DocumentElement.SelectSingleNode("//POTSNUMBER").InnerText);
						}
						catch { }

						Task imsTask = new Task(taskID, type, city, manufacturer, omcName, oltID, null, ponID, onuID, onuType, svlan, cvlan,
												phone, DateTime.Now, muvlan, ywvlan, msvlan, mcvlan, feNumber, potsNumber);
						SQLUtil.InsertTask(m_conn, imsTask);
						break;
					case TaskType.DelONU:
						try
						{
							msvlan = int.Parse(xmldoc.DocumentElement.SelectSingleNode("//MSVLAN").InnerText);
						}
						catch { }

						try
						{
							mcvlan = int.Parse(xmldoc.DocumentElement.SelectSingleNode("//MCVLAN").InnerText);
						}
						catch { }

						try
						{
							feNumber = int.Parse(xmldoc.DocumentElement.SelectSingleNode("//FENUMBER").InnerText);
						}
						catch { }

						try
						{
							potsNumber = int.Parse(xmldoc.DocumentElement.SelectSingleNode("//POTSNUMBER").InnerText);
						}
						catch { }

						Task delOnuTask = new Task(taskID, type, city, manufacturer, omcName, oltID, ponID, onuID, phone, DateTime.Now,
												   muvlan, ywvlan, msvlan, mcvlan, feNumber, potsNumber);
						SQLUtil.InsertTask(m_conn, delOnuTask);
						break;
					case TaskType.DelBroadband:
					case TaskType.DelIMS:
						try
						{
							svlan = int.Parse(xmldoc.DocumentElement.SelectSingleNode("//SVLAN").InnerText);
						}
						catch { }

						try
						{
							cvlan = int.Parse(xmldoc.DocumentElement.SelectSingleNode("//CVLAN").InnerText);
						}
						catch { }

						try
						{
							msvlan = int.Parse(xmldoc.DocumentElement.SelectSingleNode("//MSVLAN").InnerText);
						}
						catch { }

						try
						{
							mcvlan = int.Parse(xmldoc.DocumentElement.SelectSingleNode("//MCVLAN").InnerText);
						}
						catch { }

						try
						{
							feNumber = int.Parse(xmldoc.DocumentElement.SelectSingleNode("//FENUMBER").InnerText);
						}
						catch { }

						try
						{
							potsNumber = int.Parse(xmldoc.DocumentElement.SelectSingleNode("//POTSNUMBER").InnerText);
						}
						catch { }

						Task delTask = new Task(taskID, type, city, manufacturer, omcName, oltID, ponID, onuID, svlan, cvlan,
												 phone, DateTime.Now, muvlan, ywvlan, msvlan, mcvlan, feNumber, potsNumber);
						SQLUtil.InsertTask(m_conn, delTask);
						break;
					default:
						WriteLog.WriteLine(WriteLog.m_FilePreName, string.Format("{0} {1} {2}", tName, taskID, "工单类型：" + taskType + " 未定义"));
						result = IdlUtil.ConvertToReslutXML(0, "工单类型：" + taskType + " 未定义");
						break;
				}

				WriteLog.WriteLine(WriteLog.m_FilePreName, string.Format("{0} {1} spend {2} s", tName, taskID, (DateTime.Now - dt1).TotalSeconds.ToString()));
			}
			catch (NullReferenceException ex)
			{
				result = IdlUtil.ConvertToReslutXML(0, "报文信息不完整或格式有误,请核查");
				WriteLog.WriteLine(xml + Environment.NewLine + ex.ToString());
			}
			catch (Exception ex)
			{
				if (ex.Message.IndexOf("重复键") >= 0)
				{
					result = IdlUtil.ConvertToReslutXML(0, "此工单已下发");
				}
				else
				{
					result = IdlUtil.ConvertToReslutXML(0, ex.Message);
				}
				WriteLog.WriteLine(xml + Environment.NewLine + ex.ToString());
			}

			return result;
		}

		/// <summary>
		/// 回滚
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		[WebMethod]
		public string RollbackTask(string xml)
		{
			string result = string.Empty;
			try
			{
				XmlDocument xmldoc = new XmlDocument();
				xmldoc.LoadXml(xml);
				string taskID = xmldoc.DocumentElement.SelectSingleNode("//TaskID").InnerText;

				DateTime dt1 = DateTime.Now;
				WriteLog.WriteLine(WriteLog.m_FilePreName, string.Format("RollbackTask {0}", taskID));

				System.Data.SqlClient.SqlParameter sp_taskID = new System.Data.SqlClient.SqlParameter("@taskID", taskID);
				System.Data.SqlClient.SqlParameter sp_flag = new System.Data.SqlClient.SqlParameter("@flag", System.Data.SqlDbType.Int);
				sp_flag.Direction = System.Data.ParameterDirection.Output;
				System.Data.SqlClient.SqlParameter sp_msg = new System.Data.SqlClient.SqlParameter("@msg", System.Data.SqlDbType.VarChar);
				sp_msg.Direction = System.Data.ParameterDirection.Output;
				sp_msg.Size = 1024;
				SQLUtil.ExecProc(m_conn, "spRollbackHistoryTask", sp_taskID, sp_flag, sp_msg);
				result = sp_msg.Value.ToString();

				WriteLog.WriteLine(WriteLog.m_FilePreName, string.Format("RollbackTask {0} spend {1} s", taskID, (DateTime.Now - dt1).TotalSeconds.ToString()));
				switch (int.Parse(sp_flag.Value.ToString()))
				{
					case 1:
						result = IdlUtil.ConvertToReslutXML(1, "接收成功");
						break;
					default:
						result = IdlUtil.ConvertToReslutXML(0, result);
						break;
				}
			}
			catch (NullReferenceException ex)
			{
				result = IdlUtil.ConvertToReslutXML(0, "报文信息不完整或格式有误,请核查");
				WriteLog.WriteLine(xml + Environment.NewLine + ex.ToString());
			}
			catch (Exception ex)
			{
				if (ex.Message.IndexOf("重复键") >= 0)
				{
					result = IdlUtil.ConvertToReslutXML(0, "此工单已下发");
				}
				else
				{
					result = IdlUtil.ConvertToReslutXML(0, ex.Message);
				}
				WriteLog.WriteLine(xml + Environment.NewLine + ex.ToString());
			}

			return result;
		}
	}

	public class WriteLog
	{
		static public string m_Path = System.AppDomain.CurrentDomain.BaseDirectory + @"log\";
		static public string m_FilePreName = "sys";
		static public string m_ExcFilePreName = "exc";
		static Mutex m_WriteMutex = new Mutex();
		public WriteLog()
		{
			//
			//TODO: 在此处添加构造函数逻辑
			//
		}
		/// <summary>
		/// 记录日志
		/// </summary>
		/// <param name="dataText">日志文本</param>
		/// <returns></returns>
		static public bool WriteLine(string dataText)
		{
			return WriteLine(m_ExcFilePreName, dataText, 3);
		}

		/// <summary>
		/// 记录日志
		/// </summary>
		/// <param name="dataText">日志文本</param>
		/// <returns></returns>
		static public bool WriteLine(string filePreName, string dataText)
		{
			return WriteLine(filePreName, dataText, 3);
		}

		static public bool WriteLine(string filePreName, string dataText, int theLevel)
		{
			FileStream fs = null;
			StreamWriter sw = null;
			bool ret = true;
			m_WriteMutex.WaitOne();
			try
			{
				string FileName = m_Path;
				//CHECK文件目录存在不   
				if (!Directory.Exists(FileName))
				{
					Directory.CreateDirectory(FileName);
				}
				FileName += @"\" + filePreName + DateTime.Now.ToString(".yyyMMdd") + ".log";

				//CHECK文件存在不   
				if (!File.Exists(FileName))
				{
					FileStream tempfs = File.Create(FileName);
					tempfs.Close();
				}
				fs = new FileStream(
				   FileName,
					FileMode.Append,
				   FileAccess.Write,
				   FileShare.None);
				fs.Seek(0, System.IO.SeekOrigin.End);
				sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
				string LineText = DateTime.Now.ToString("yyy-MM-dd ") + DateTime.Now.ToString("T");
				sw.WriteLine(LineText);
				sw.WriteLine("-------------------------------------------------------------------------");
				sw.WriteLine(dataText);
				sw.WriteLine("");
				if (sw != null)
				{
					sw.Close();
					sw = null;
				}
				if (fs != null)
				{
					fs.Close();
					fs = null;
				}
			}
			catch (Exception)
			{
				ret = false;
			}
			finally
			{
				try
				{
					if (sw != null)
					{
						sw.Close();
						sw = null;
					}
					if (fs != null)
					{
						fs.Close();
						fs = null;
					}
				}
				catch
				{
				}
				m_WriteMutex.ReleaseMutex();
			}
			return ret;
		}

		static public void WriteFile(string filename, byte[] dataText)
		{
			try
			{
				FileInfo finfo = new FileInfo(filename);
				FileStream fs = finfo.OpenWrite();
				fs.Write(dataText, 0, dataText.Length);
				fs.Close();
				return;
			}
			catch (Exception)
			{
				return;
			}
		}
	}
}
