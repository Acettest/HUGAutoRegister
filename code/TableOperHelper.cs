using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

#if MYSQL
using MySql.Data.MySqlClient;
#else
using System.Data.SqlClient;
#endif

using Microsoft.ApplicationBlocks.Data;
using DefLib.Util;
using System.IO;

namespace TK_AlarmManagement
{
    public class TableOperHelper : ICommandHandler
    {

        #region 命令注册、去注册
        public void RegisterCommand()
        {
            CommandProcessor.instance().registerReportHandler(Constants.TK_CommandType.MON_GET_TABLUARINFO, this, null);
            CommandProcessor.instance().registerReportHandler(Constants.TK_CommandType.MON_COMMIT_TABLUARINFO, this, null);
            CommandProcessor.instance().registerReportHandler(Constants.TK_CommandType.MON_GET_TABLUARINFO_RAWQUERY, this, null);
            CommandProcessor.instance().registerReportHandler(Constants.TK_CommandType.MON_EXECUTE_NONQUERY, this, null);
        }


        public void UnregisterCommand()
        {
            CommandProcessor.instance().unregisterReportHandler(Constants.TK_CommandType.MON_GET_TABLUARINFO, this);
            CommandProcessor.instance().unregisterReportHandler(Constants.TK_CommandType.MON_COMMIT_TABLUARINFO, this);
            CommandProcessor.instance().unregisterReportHandler(Constants.TK_CommandType.MON_GET_TABLUARINFO_RAWQUERY, this);
            CommandProcessor.instance().unregisterReportHandler(Constants.TK_CommandType.MON_EXECUTE_NONQUERY, this);
        }
        #endregion

        #region ICommandHandler 成员

        public void handleCommand(ICommunicationMessage message)
        {
            switch (message.TK_CommandType)
            {
                case Constants.TK_CommandType.MON_GET_TABLUARINFO:
                    CommandProcessor.instance().DispatchCommand(_GetTabluarInfo(message));
                    break;
                case Constants.TK_CommandType.MON_COMMIT_TABLUARINFO:
                    CommandProcessor.instance().DispatchCommand(_CommitTabluarInfo(message));
                    break;
                case Constants.TK_CommandType.MON_GET_TABLUARINFO_RAWQUERY:
                    CommandProcessor.instance().DispatchCommand(_GetTabluarVariantInfo(message));
                    break;
                case Constants.TK_CommandType.MON_EXECUTE_NONQUERY:
                    CommandProcessor.instance().DispatchCommand(_ExecuteNonQuery(message));
                    break;
            }
        }


        private ICommunicationMessage _ExecuteNonQuery(ICommunicationMessage message)
        {
            CommandMsgV2 resp = new CommandMsgV2();
            resp.SeqID = CommandProcessor.AllocateID();
            resp.TK_CommandType = Constants.TK_CommandType.RESPONSE;
            resp.SetValue(Constants.MSG_PARANAME_RESPONSE_TO, message.SeqID);
            resp.SetValue("ClientID", message.GetValue("ClientID"));
            resp.SetValue(Constants.MSG_PARANAME_RESULT, "OK");

            try
            {
                string cmd = message.GetValue(Constants.MSG_PARANAME_RAWSQL).ToString();
                string catalog = message.GetValue(Constants.MSG_PARANAME_TABLECATALOG).ToString().ToUpper();
                string connstr = "";
                if (catalog == "SYS_DB")
                    connstr = AlarmManager.instance().ConnString;
                else if (catalog == "USER_DB")
                    connstr = UserManager.instance().ConnString;

                SqlHelper.ExecuteNonQuery(connstr, CommandType.Text, cmd);
            }
            catch (Exception ex)
            {
                resp.SetValue(Constants.MSG_PARANAME_RESULT, "NOK");
                resp.SetValue(Constants.MSG_PARANAME_REASON, ex.Message);
            }

            return resp;
        }

        private ICommunicationMessage _GetTabluarVariantInfo(ICommunicationMessage message)
        {
            CommandMsgV2 resp = new CommandMsgV2();
            resp.SeqID = CommandProcessor.AllocateID();
            resp.TK_CommandType = Constants.TK_CommandType.RESPONSE;
            resp.SetValue(Constants.MSG_PARANAME_RESPONSE_TO, message.SeqID);
            resp.SetValue("ClientID", message.GetValue("ClientID"));
            resp.SetValue(Constants.MSG_PARANAME_RESULT, "OK");
            resp.SetValue(Constants.MSG_PARANAME_CHARTRESULT, null);

            try
            {
                string cmd = message.GetValue(Constants.MSG_PARANAME_RAWSQL).ToString();
                string catalog = message.GetValue(Constants.MSG_PARANAME_TABLECATALOG).ToString().ToUpper();
                string connstr = "";
                if (catalog == "SYS_DB")
                    connstr = AlarmManager.instance().ConnString;
                else if (catalog == "USER_DB")
                    connstr = UserManager.instance().ConnString;

                DataSet ds = new DataSet();
                SqlHelper.FillDataset(connstr, CommandType.Text, cmd, ds, new string[] { "result" });
                ds.AcceptChanges();
                resp.SetValue(Constants.MSG_PARANAME_CHARTRESULT, ds.Tables["result"]);
            }
            catch (Exception ex)
            {
                resp.SetValue(Constants.MSG_PARANAME_RESULT, "NOK");
                resp.SetValue(Constants.MSG_PARANAME_REASON, ex.Message);
            }

            return resp;
        }

        CommandMsgV2 _GetTabluarInfo(ICommunicationMessage message)
        {
            CommandMsgV2 resp = new CommandMsgV2();
            resp.SeqID = CommandProcessor.AllocateID();
            resp.TK_CommandType = Constants.TK_CommandType.RESPONSE;
            resp.SetValue(Constants.MSG_PARANAME_RESPONSE_TO, message.SeqID);
            resp.SetValue("ClientID", message.GetValue("ClientID"));
            resp.SetValue(Constants.MSG_PARANAME_RESULT, "OK");
            resp.SetValue(Constants.MSG_PARANAME_CHARTRESULT, null);

            try
            {
                string filter = message.GetValue(Constants.MSG_PARANAME_TABLEFILTER).ToString();
                //string pkcolumn = message.GetValue(Constants.MSG_PARANAME_TABLEPKCOL).ToString();
                string tn = message.GetValue(Constants.MSG_PARANAME_TABLENAME).ToString();

                string catalog = message.GetValue(Constants.MSG_PARANAME_TABLECATALOG).ToString().ToUpper();
                string connstr = "";
                if (catalog == "SYS_DB")
                    connstr = AlarmManager.instance().ConnString;
                else if (catalog == "USER_DB")
                    connstr = UserManager.instance().ConnString;

                string cmd = "select * from " + tn;
                if (filter.Trim() != "")
                    cmd += " where " + filter;

                DataSet ds = new DataSet();
                SqlHelper.FillDataset(connstr, CommandType.Text, cmd, ds, new string[] { tn });
                ds.AcceptChanges();
                resp.SetValue(Constants.MSG_PARANAME_CHARTRESULT, ds.Tables[tn]);
            }
            catch (Exception ex)
            {
                resp.SetValue(Constants.MSG_PARANAME_RESULT, "NOK");
                resp.SetValue(Constants.MSG_PARANAME_REASON, ex.Message);
            }

            return resp;
        }

        CommandMsgV2 _CommitTabluarInfo(ICommunicationMessage message)
        {
            CommandMsgV2 resp = new CommandMsgV2();
            resp.SeqID = CommandProcessor.AllocateID();
            resp.TK_CommandType = Constants.TK_CommandType.RESPONSE;
            resp.SetValue(Constants.MSG_PARANAME_RESPONSE_TO, message.SeqID);
            resp.SetValue("ClientID", message.GetValue("ClientID"));
            resp.SetValue(Constants.MSG_PARANAME_RESULT, "OK");

            try
            {
                string pkcolumn = message.GetValue(Constants.MSG_PARANAME_TABLEPKCOL).ToString();
                string tn = message.GetValue(Constants.MSG_PARANAME_TABLENAME).ToString();

                string catalog = message.GetValue(Constants.MSG_PARANAME_TABLECATALOG).ToString().ToUpper();

                DataTable dt = message.GetValue(Constants.MSG_PARANAME_CHARTRESULT) as DataTable;
                _CommitTable(catalog, dt, tn, pkcolumn);
            }
            catch (Exception ex)
            {
                resp.SetValue(Constants.MSG_PARANAME_RESULT, "NOK");
                resp.SetValue(Constants.MSG_PARANAME_REASON, ex.Message);
            }

            return resp;
        }

        private void _CommitTable(string catalog, DataTable dataTable, string tn, string pkcolumn)
        {
            DataTable newtable = dataTable.GetChanges();
            if (newtable == null)
                return;

            string connstr = "";
            if (catalog == "SYS_DB")
                connstr = AlarmManager.instance().ConnString;
            else if (catalog == "USER_DB")
                connstr = UserManager.instance().ConnString;

#if MYSQL
            MySqlConnection conn = new MySqlConnection(connstr);
            MySqlTransaction tr = null;
#else
            SqlConnection conn = new SqlConnection(connstr);
            SqlTransaction tr = null;
#endif

            try
            {
                conn.Open();
                tr = conn.BeginTransaction();

                string[] pk = pkcolumn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                DataColumn[] pkcol = new DataColumn[pk.Length];
                for (int i = 0; i < pk.Length; ++i)
                {
                    pkcol[i] = dataTable.Columns[pk[i].Trim()];
                }

                dataTable.PrimaryKey = pkcol;
#if MYSQL
                SqlHelper.UpdateDataTable(tr, string.Format("select * from {0} limit 0,1", tn), dataTable);
#else
                SqlHelper.UpdateDataTable(tr, string.Format("select top 1 * from {0}", tn), dataTable);
#endif


                tr.Commit();
            }
            catch
            {
                try
                {
                    if (tr != null)
                        tr.Rollback();
                }
                catch { }

                throw;
            }
            finally
            {
                try
                {
                    conn.Close();
                }
                catch { }
            }
        }
        #endregion
    }

    public class NorthServer
    {
        public string servername = "";
        public string serverip = "";
        public string port = "";
    }
}
