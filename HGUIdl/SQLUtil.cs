#region 描述
//-----------------------------------------------------------------------------
// 文 件 名: SQLUtil
// 作    者：d.w
// 创建时间：2014/6/12 16:12:52
// 描    述：
// 版    本：1.0.0.0
//-----------------------------------------------------------------------------
// 历史更新纪录
//-----------------------------------------------------------------------------
// 版    本：           修改时间：           修改人：           
// 修改内容：
//-----------------------------------------------------------------------------
// Copyright (C) 2009-2014 www.chaselwang.com . All Rights Reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.ApplicationBlocks.Data;
using System.Data;
using System.Data.SqlClient;

namespace HGU.Idl
{
    public class SQLUtil
    {
        public static string s_conn;

        public static void UpdateSvrStatus(string conn, string connectStatus, string statusDesc, string svrID)
        {
            string sql = "update GPONServer set connectStatus='" + connectStatus + "',statusDesc='" + statusDesc + "'";
            if (connectStatus == "0")
                sql += ",lastOffTime='" + DateTime.Now.ToString() + "'";
            else
                sql += ",connectTime='" + DateTime.Now.ToString() + "'";

            sql += " where SvrID=" + svrID;

            SqlHelper.ExecuteNonQuery(conn, CommandType.Text, sql);
        }

        public static void ExecProc(string conn, string proName, params SqlParameter[] commandParameters)
        {
            SqlHelper.ExecuteNonQuery(conn, CommandType.StoredProcedure, proName, commandParameters);
        }

        public static void ExecText(string conn, string text)
        {
            SqlHelper.ExecuteNonQuery(conn, CommandType.Text, text);
        }

        /// <summary>
        /// 从服务器配置表获取配置信息
        /// </summary>
        public static OMCAddInfo GetOMCInfo(string conn, int omcID)
        {
            DataSet ds = new DataSet();

            string sql = "select city,manufacturer,omcName,omcIP,omcPort,userName,pwd from GPONServer where SvrID=" + omcID.ToString();

            SqlHelper.FillDataset(conn, CommandType.Text, sql, ds, new string[] { "svrinfo" });
            OMCAddInfo omc = new OMCAddInfo(
                new OMC(
                    ds.Tables["svrinfo"].Rows[0]["city"].ToString(),
                    ds.Tables["svrinfo"].Rows[0]["manufacturer"].ToString(),
                    ds.Tables["svrinfo"].Rows[0]["omcName"].ToString()),
                ds.Tables["svrinfo"].Rows[0]["omcIP"].ToString(),
                Int32.Parse(ds.Tables["svrinfo"].Rows[0]["omcPort"].ToString()),
                ds.Tables["svrinfo"].Rows[0]["userName"].ToString(),
                ds.Tables["svrinfo"].Rows[0]["pwd"].ToString());
            return omc;
        }


        /// <summary>
        /// 补齐字段，加空值判断
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="task"></param>
        public static void InsertTask(string conn, Task task)
        {
            #region 之前的代码
            ////StringBuilder sb = new StringBuilder();
            ////sb.Append("insert into Task ([taskID],[taskType],[city],[manufacturer],[omcName]," +
            ////    "[oltID],[ponID],[onuID],[onuPort],[onuType],[svlan],[cvlan]," +
            ////    "[phone],[receiveTime]) values('");
            ////sb.Append(task.TaskID.ToString());
            ////sb.Append("','");
            ////sb.Append(IdlUtil.ParseTaskTypeToString(task.Type));
            ////sb.Append("','");
            ////sb.Append(task.City);
            ////sb.Append("','");
            ////sb.Append(task.Manufacturer);
            ////sb.Append("','");
            ////sb.Append(task.OmcName);
            ////sb.Append("',");
            ////sb.Append(task.OltID == null ? "null" : ("'" + task.OltID + "'"));
            ////sb.Append(",");
            ////sb.Append(task.PonID == null ? "null" : ("'" + task.PonID + "'"));
            ////sb.Append(",");
            ////sb.Append(task.OnuID == null ? "null" : ("'" + task.OnuID + "'"));
            ////sb.Append(",");
            ////sb.Append(task.OnuType == null ? "null" : ("'" + task.OnuType + "'"));
            ////sb.Append(",");
            ////sb.Append(task.Svlan == -1 ? "null" : task.Svlan.ToString());
            ////sb.Append(",");
            ////sb.Append(task.Cvlan == -1 ? "null" : task.Cvlan.ToString());
            ////sb.Append(",'");
            ////sb.Append(task.Phone);
            ////sb.Append("',");
            ////sb.Append(",'");
            //////sb.Append(task.ResponseBoss);
            //////sb.Append("','");
            ////sb.Append(task.ReceiveTime.ToString());
            ////sb.Append("')");
            ////SqlHelper.ExecuteNonQuery(conn, CommandType.Text, sb.ToString());
            #endregion

            System.Data.SqlClient.SqlParameter sp_taskID = new System.Data.SqlClient.SqlParameter("@taskID", task.TaskID);
            System.Data.SqlClient.SqlParameter sp_taskType = new System.Data.SqlClient.SqlParameter("@taskType", IdlUtil.ParseTaskTypeToString(task.Type));
            System.Data.SqlClient.SqlParameter sp_city = new System.Data.SqlClient.SqlParameter("@city", task.City);
            System.Data.SqlClient.SqlParameter sp_manufacturer = new System.Data.SqlClient.SqlParameter("@manufacturer",task.Manufacturer);
            System.Data.SqlClient.SqlParameter sp_omcName = new System.Data.SqlClient.SqlParameter("@omcName",task.OmcName);
            System.Data.SqlClient.SqlParameter sp_oltID = new System.Data.SqlClient.SqlParameter("@oltID",task.OltID);
            System.Data.SqlClient.SqlParameter sp_ponID = new System.Data.SqlClient.SqlParameter("@ponID",task.PonID);
            System.Data.SqlClient.SqlParameter sp_onuID = new System.Data.SqlClient.SqlParameter("@onuID",task.OnuID);
            System.Data.SqlClient.SqlParameter sp_svlan = new System.Data.SqlClient.SqlParameter("@svlan",task.Svlan);
            System.Data.SqlClient.SqlParameter sp_cvlan = new System.Data.SqlClient.SqlParameter("@cvlan",task.Cvlan);
            System.Data.SqlClient.SqlParameter sp_phone = new System.Data.SqlClient.SqlParameter("@phone",task.Phone);
            System.Data.SqlClient.SqlParameter sp_receiveTime = new System.Data.SqlClient.SqlParameter("@receiveTime", task.ReceiveTime);
            System.Data.SqlClient.SqlParameter sp_mvlan = new System.Data.SqlClient.SqlParameter("@mvlan",task.Mvlan);
            System.Data.SqlClient.SqlParameter sp_feNumber = new System.Data.SqlClient.SqlParameter("@feNumber",task.FENumber);
            System.Data.SqlClient.SqlParameter sp_potsNumber = new System.Data.SqlClient.SqlParameter("@potsNumber",task.POTSNumber);
            SQLUtil.ExecProc(conn, "spInsertTask", sp_taskID, sp_taskType, sp_city,sp_manufacturer,sp_omcName,sp_oltID,sp_ponID,sp_onuID,sp_svlan,
                              sp_cvlan,sp_phone,sp_receiveTime, sp_mvlan, sp_feNumber, sp_potsNumber);
        }





        public static void InsertRelocateTask(string conn, string taskID,string taskType, string city, string manufacturer,string omcName,
             string oltID, string ponID, string onuID, string phone, int svlan, int cvlan, int mvlan, int feNumber, int potsNumber,
             string oldCity, string oldManufacturer, string oldOmcName,string oldOltID, string oldPonID, string oldOnuID,
             string oldPhone,int oldSvlan, int oldCvlan, int oldmvlan,int oldFeNumber, int oldPotsNumber,string IsContainIMS,
             int imsSvlan, int imsCvlan, string IsContainIPTV,int iptvSvlan,int iptvCvlan)
        {
            System.Data.SqlClient.SqlParameter sp_taskID = new System.Data.SqlClient.SqlParameter("@taskID", taskID);
            System.Data.SqlClient.SqlParameter sp_taskType = new System.Data.SqlClient.SqlParameter("@taskType", taskType);
            System.Data.SqlClient.SqlParameter sp_city = new System.Data.SqlClient.SqlParameter("@city", city);
            System.Data.SqlClient.SqlParameter sp_manufacturer = new System.Data.SqlClient.SqlParameter("@manufacturer", manufacturer);
            System.Data.SqlClient.SqlParameter sp_omcName = new System.Data.SqlClient.SqlParameter("@omcName", omcName);
            System.Data.SqlClient.SqlParameter sp_oltID = new System.Data.SqlClient.SqlParameter("@oltID", oltID);
            System.Data.SqlClient.SqlParameter sp_ponID = new System.Data.SqlClient.SqlParameter("@ponID", ponID);
            System.Data.SqlClient.SqlParameter sp_onuID = new System.Data.SqlClient.SqlParameter("@onuID", onuID);
            System.Data.SqlClient.SqlParameter sp_phone = new System.Data.SqlClient.SqlParameter("@phone", phone);
            System.Data.SqlClient.SqlParameter sp_svlan = new System.Data.SqlClient.SqlParameter("@svlan", svlan);
            System.Data.SqlClient.SqlParameter sp_cvlan = new System.Data.SqlClient.SqlParameter("@cvlan", cvlan);
            System.Data.SqlClient.SqlParameter sp_mvlan = new System.Data.SqlClient.SqlParameter("@mvlan", mvlan);
            System.Data.SqlClient.SqlParameter sp_feNumber = new System.Data.SqlClient.SqlParameter("@feNumber", feNumber);
            System.Data.SqlClient.SqlParameter sp_potsNumber = new System.Data.SqlClient.SqlParameter("@potsNumber", potsNumber);
            System.Data.SqlClient.SqlParameter sp_receiveTime = new System.Data.SqlClient.SqlParameter("@receiveTime", DateTime.Now.ToString());

            System.Data.SqlClient.SqlParameter sp_oldCity = new System.Data.SqlClient.SqlParameter("@oldCity", oldCity);
            System.Data.SqlClient.SqlParameter sp_oldManufacturer = new System.Data.SqlClient.SqlParameter("@oldManufacturer", oldManufacturer);
            System.Data.SqlClient.SqlParameter sp_oldOmcName = new System.Data.SqlClient.SqlParameter("@oldOmcName", oldOmcName);
            System.Data.SqlClient.SqlParameter sp_oldOltID = new System.Data.SqlClient.SqlParameter("@oldOltID", oldOltID);
            System.Data.SqlClient.SqlParameter sp_oldPonID = new System.Data.SqlClient.SqlParameter("@oldPonID", oldPonID);
            System.Data.SqlClient.SqlParameter sp_oldOnuID = new System.Data.SqlClient.SqlParameter("@oldOnuID", oldOnuID);
            System.Data.SqlClient.SqlParameter sp_oldphone = new System.Data.SqlClient.SqlParameter("@oldphone", phone);
            System.Data.SqlClient.SqlParameter sp_oldSvlan = new System.Data.SqlClient.SqlParameter("@oldSvlan", oldSvlan);
            System.Data.SqlClient.SqlParameter sp_oldCvlan = new System.Data.SqlClient.SqlParameter("@oldCvlan", oldCvlan);
            System.Data.SqlClient.SqlParameter sp_oldmvlan = new System.Data.SqlClient.SqlParameter("@oldmvlan", oldmvlan);
            System.Data.SqlClient.SqlParameter sp_oldFeNumber = new System.Data.SqlClient.SqlParameter("@oldFeNumber", oldFeNumber);
            System.Data.SqlClient.SqlParameter sp_oldPotsNumber = new System.Data.SqlClient.SqlParameter("@oldPotsNumber", oldPotsNumber);

            System.Data.SqlClient.SqlParameter sp_IsContainIMS = new System.Data.SqlClient.SqlParameter("@IsContainIMS", IsContainIMS);
            System.Data.SqlClient.SqlParameter sp_imsSvlan = new System.Data.SqlClient.SqlParameter("@imsSvlan", imsSvlan);
            System.Data.SqlClient.SqlParameter sp_imsCvlan = new System.Data.SqlClient.SqlParameter("@imsCvlan", imsCvlan);

            System.Data.SqlClient.SqlParameter sp_IsContainIPTV = new System.Data.SqlClient.SqlParameter("@IsContainIPTV", IsContainIPTV);
            System.Data.SqlClient.SqlParameter sp_iptvSvlan = new System.Data.SqlClient.SqlParameter("@iptvSvlan", iptvSvlan);
            System.Data.SqlClient.SqlParameter sp_iptvCvlan = new System.Data.SqlClient.SqlParameter("@iptvCvlan", iptvCvlan);

            SQLUtil.ExecProc(conn, "spInsertRelocateTask", sp_taskID, sp_taskType, sp_city, sp_manufacturer, sp_omcName, sp_oltID, sp_ponID,
                             sp_onuID, sp_phone, sp_svlan, sp_cvlan, sp_mvlan, sp_feNumber, sp_potsNumber, sp_receiveTime, sp_oldCity, sp_oldManufacturer,
                             sp_oldOmcName, sp_oldOltID, sp_oldPonID, sp_oldOnuID, sp_oldphone, sp_oldSvlan, sp_oldCvlan, sp_oldmvlan, sp_oldFeNumber,
                             sp_oldPotsNumber, sp_IsContainIMS, sp_imsSvlan, sp_imsCvlan, sp_IsContainIPTV, sp_iptvSvlan, sp_iptvCvlan);
        }

        public static void UpdateTask(string conn, Task task, string tableName, bool isRolllback)
        {
            StringBuilder sb = new StringBuilder();
            DateTime empty = new DateTime();
            sb.Append("update " + tableName + " set ");
            //移机标记 1 拆机 2 装机
            if (!isRolllback)
                sb.Append("[exeCount]=exeCount+1,");
            
            sb.Append("[netInterrupt]='" + task.NetInterrupt + "'");
            sb.Append(",[taskStatus]='" + IdlUtil.ParseTaskStatusToString(task.Status) + "'");
            sb.Append(",[receiveTime]=" + (task.ReceiveTime == empty ? "null" : ("'" + task.ReceiveTime.ToString() + "'")));
            sb.Append(",[responseMsg]=" + (task.ResponseMsg == null ? "null" : ("'" + task.ResponseMsg.Replace("'", "''") + "'")));
            sb.Append(",[executeTime]=" + (task.ExecuteTime == empty ? "null" : ("'" + task.ExecuteTime.ToString() + "'")));
            sb.Append(",[completeTime]=" + (task.CompleteTime == empty ? "null" : ("'" + task.CompleteTime.ToString() + "'")));
            sb.Append(",[responseTime]=" + (task.ResponseTime == empty ? "null" : ("'" + task.ResponseTime.ToString() + "'")));
            if (task.Status == TaskStatus.Fail && !isRolllback)
                sb.Append(",[isRollback]='" + task.Rollback.ToString() + "'");
            if (task.Status == TaskStatus.Fail)
                sb.Append(",[errorDesc]='" + Enum.GetName(typeof(ErrorDesc), task.Error) + "'");
            sb.Append(" where taskid='" + task.TaskID.ToString() + "'");
            SqlHelper.ExecuteNonQuery(conn, CommandType.Text, sb.ToString());
        }

        public static void UpdateNetDelayTask(string conn, Task task, string tableName)
        {
            string sql = "update " + tableName +
                " set netDelay=1,responseMsg='激活系统与OMC连接中断，数据未下发，请联系网管人员' where taskid='" +
                task.TaskID.ToString() + "'";
            SqlHelper.ExecuteNonQuery(conn, CommandType.Text, sql);
        }

        public static void UpdateOltOfflineTask(string conn, Task task, string tableName)
        {
            string sql = "update " + tableName +
                " set oltOffline='" + task.OltOffline + "',netDelay=1,responseMsg='OMC与OLT连接中断，数据未下发，请联系网管人员' where taskid='" +
                task.TaskID.ToString() + "'";
            SqlHelper.ExecuteNonQuery(conn, CommandType.Text, sql);
        }

        public static void UpdateTaskReplyInfo(string conn, string taskID, string bossReply,
            DateTime ResponseTime, int bossReplyStatus, string resXML, string tableName)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("update " + tableName + " set ");
            sb.Append("[responseBoss]=1");
            sb.Append(",[bossReply]='" + bossReply + "'");
            sb.Append(",[responseTime]='" + ResponseTime.ToString() + "'");
            sb.Append(",[bossReplyStatus]='" + bossReplyStatus.ToString() + "'");
            sb.Append(",[responseXML]='" + resXML.Replace("'", "''") + "'");
            sb.Append(" where taskid='" + taskID.ToString() + "'");
            SqlHelper.ExecuteNonQuery(conn, CommandType.Text, sb.ToString());
        }

        public static DataTable GetTask(string conn, string viewName, string whereStr)
        {
            DataSet ds = new DataSet();
            string sql = "select * from " + viewName + " " + whereStr;
            SqlHelper.FillDataset(conn, CommandType.Text, sql, ds, new string[] { "Task" });
            return ds.Tables["Task"];
        }

        public static Task GetRelocateRollbackTask(string taskID)
        {
            DataSet ds = new DataSet();
            string sql = "select taskID, taskType, oldcity,oldmanufacturer, oldomcName, oldoltID, oldponID, oldonuID, oldonuType,oldsvlan,oldcvlan,oldPhone," +
                          "receiveTime,oldmuvlan,oldywvlan,oldmvlan,oldfeNumber,oldpotsNumber,IsContainIMS,imsSvlan,imsCvlan,imsUV from task where taskid='" + taskID + "'";
            SqlHelper.FillDataset(s_conn, CommandType.Text, sql, ds, new string[] { "Task" });
            if (ds.Tables["Task"].Rows.Count == 0)
                return null;
            else
            {
                DataRow dr = ds.Tables["Task"].Rows[0];
                return new Task(taskID,
                    IdlUtil.ParseStringToTaskType(dr["taskType"].ToString()),
                    dr["oldcity"].ToString(),
                    dr["oldmanufacturer"].ToString(),
                    dr["oldomcName"].ToString(),
                    dr["oldoltID"].ToString(),
                    (dr["oldponID"] == DBNull.Value ? null : dr["oldponID"].ToString()),
                    (dr["oldonuID"] == DBNull.Value ? null : dr["oldonuID"].ToString()),
                    (dr["oldonuType"] == DBNull.Value ? null : dr["oldonuType"].ToString()),
                    (dr["oldsvlan"] == DBNull.Value ? -1 : int.Parse(dr["oldsvlan"].ToString())),
                    (dr["oldcvlan"] == DBNull.Value ? -1 : int.Parse(dr["oldcvlan"].ToString())),
                    dr["oldPhone"].ToString(), DateTime.Parse(dr["receiveTime"].ToString()),
                    (dr["oldmuvlan"] == DBNull.Value ? -1 : int.Parse(dr["oldmuvlan"].ToString())),
                    (dr["oldywvlan"] == DBNull.Value ? -1 : int.Parse(dr["oldywvlan"].ToString())),
                    (dr["oldmvlan"] == DBNull.Value ? -1 : int.Parse(dr["oldmvlan"].ToString())),
                    (dr["oldfeNumber"] == DBNull.Value ? -1 : int.Parse(dr["oldfeNumber"].ToString())),
                    (dr["oldpotsNumber"] == DBNull.Value ? -1 : int.Parse(dr["oldpotsNumber"].ToString())),
                    (dr["IsContainIMS"] == DBNull.Value ? null : dr["IsContainIMS"].ToString()),
                    (dr["imsSvlan"] == DBNull.Value ? -1 : int.Parse(dr["imsSvlan"].ToString())),
                    (dr["imsCvlan"] == DBNull.Value ? -1 : int.Parse(dr["imsCvlan"].ToString())),
                    (dr["imsUV"] == DBNull.Value ? -1 : int.Parse(dr["imsUV"].ToString()))
                );
            }
        }

        /// <summary>
        /// 回复boss信息
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="viewName"></param>
        /// <returns></returns>
        public static DataTable GetReplyBossMsg(string conn, string viewName)
        {
            DataSet ds = new DataSet();
            string sql = "select * from " + viewName;
            SqlHelper.FillDataset(conn, CommandType.Text, sql, ds, new string[] { "Reply" });
            return ds.Tables["Reply"];
        }

        public static void InsertCommandAndRes(string conn, string taskID, CommandAndResponsePair pair, bool rollback)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("insert into Command ([taskID],[commandStep],[commandText],[generateTime],[executeTime],[omcTime],[localTime]," +
                "[completionCode],[additionalInfo],[replyContent],[isRollback]) values('");
            sb.Append(taskID);
            sb.Append("',");
            sb.Append(pair.Step);
            sb.Append(",'");
            sb.Append(pair.Command.CommandText);
            sb.Append("','");
            sb.Append(pair.Command.GenerateTime.ToString());
            if (pair.Command.Execute)
            {
                sb.Append("','");
                sb.Append(pair.Command.ExecuteTime.ToString());
                sb.Append("',");
            }
            else
            {
                sb.Append("',null,");
            }
            if (pair.Msg != null)
            {
                sb.Append("'");
                sb.Append(pair.Msg.LocalTime.ToString());
                sb.Append("','");
                sb.Append(pair.Msg.LocalTime.ToString());
                sb.Append("','");
                sb.Append(pair.Msg.CompletionCode);
                sb.Append("','");
                sb.Append(CutShortMsg(pair.Msg.GetAddInfo().Replace("'", ""), 1000));
                sb.Append("','");
                //sb.Append(CutShortMsg(pair.Msg.ReplyContent.Replace("'", ""), 1000));
                sb.Append(pair.Msg.ReplyContent != null ? CutShortMsg(pair.Msg.ReplyContent.Replace("'", ""), 1000) : "");
                sb.Append("','");
            }
            else
                sb.Append("null,null,null,null,null,'");
            sb.Append((rollback == true ? 1 : 0) + "')");
            SqlHelper.ExecuteNonQuery(conn, CommandType.Text, sb.ToString());
        }

        private static string CutShortMsg(string msg, int length)
        {
                if (msg.Length < length)
                    return msg;
            return msg.Substring(0, length) + " :out of size.";
        }

        public static string GetOlt(string conn, string oltID)
        {
            DataSet ds = new DataSet();
            string sql = "select * from OltInfo where devip='" + oltID + "'";
            SqlHelper.FillDataset(conn, CommandType.Text, sql, ds, new string[] { "OltInfo" });
            if (ds.Tables["OltInfo"].Rows.Count < 1)
                return null;
            else
                return ds.Tables["OltInfo"].Rows[0]["DevName"].ToString();
        }

        public static void RaiseOltAlarm(string conn, OMC omc, Task task)
        {
            string oltName = SQLUtil.GetOlt(conn, task.OltID);
            if (oltName == null || oltName == "")
                oltName = task.OltID;
            StringBuilder sb = new StringBuilder();
            sb.Append("insert into ftth_activealarm (CompanyNo,CITYNAME,SVRNAME,description,raisetime,Severity,ftthTaskID,type,neid) values(");
            sb.Append("'" + omc.Manufacturer + "',");
            sb.Append("'" + omc.City + "',");
            sb.Append("'" + omc.OmcName + "',");
            sb.Append("'" + omc.OmcName + " " + oltName + " 与OMC连接中断',");
            sb.Append("'" + DateTime.Now.ToString() + "',");
            sb.Append("1,");
            sb.Append("'" + task.TaskID + "',");
            sb.Append("'olt',");
            sb.Append("'" + oltName + "')");
            SqlHelper.ExecuteNonQuery(conn, CommandType.Text, sb.ToString());
        }

        public static void RaiseOMCAlarm(string conn, OMC omc)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("insert into ftth_activealarm (CompanyNo,CITYNAME,SVRNAME,description,raisetime,Severity,type,neid) values(");
            sb.Append("'" + omc.Manufacturer + "',");
            sb.Append("'" + omc.City + "',");
            sb.Append("'" + omc.OmcName + "',");
            sb.Append("'OMC网管 " + omc.OmcName + " 与激活系统连接中断',");
            sb.Append("'" + DateTime.Now.ToString() + "',");
            sb.Append("1,");
            sb.Append("'omc',");
            sb.Append("'" + omc.OmcName + "')");
            SqlHelper.ExecuteNonQuery(conn, CommandType.Text, sb.ToString());
        }

        public static void ClearOMCAlarm(string conn, OMC omc)
        {
            SqlParameter spOMCName = new SqlParameter("@omcName", omc.OmcName);
            SQLUtil.ExecProc(conn, "spClearOMCAlarm", spOMCName);
        }

        public static void CheckOMC(string conn, string city, string manufacturer, string omcName)
        {

            DataSet ds = new DataSet();
            string sql = "select * from GPONServer where city='" + city + "' and manufacturer='" + manufacturer + "' and omcName='" + omcName + "'";
            SqlHelper.FillDataset(conn, CommandType.Text, sql, ds, new string[] { "serverInfo" });
            if (ds.Tables["serverInfo"].Rows.Count < 1)
                throw new Exception(string.Format("地市={0};厂家={1};网管名称={2};错误描述=网管信息不存在，请核查数据;", city, manufacturer, omcName));
        }
    }
}
