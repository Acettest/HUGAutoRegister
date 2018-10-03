using System;
using System.Collections.Generic;
using System.Text;
using DefLib.Util;

namespace TK_AlarmManagement
{
    public class AdapterControlHelper : ICommandHandler
    {
        ICommServer m_AdapterServer = null;
        public AdapterControlHelper(ICommServer server)
        {
            m_AdapterServer = server;
        }

        public void RegisterCommand()
        {
            CommandProcessor.instance().registerReportHandler(Constants.TK_CommandType.ADAPTER_START, this, null);
            CommandProcessor.instance().registerReportHandler(Constants.TK_CommandType.ADAPTER_STOP, this, null);
            CommandProcessor.instance().registerReportHandler(Constants.TK_CommandType.MON_VERIFYTERMINALSSTATUS, this, null);
        }

        public void UnregisterCommand()
        {
            CommandProcessor.instance().unregisterReportHandler(Constants.TK_CommandType.ADAPTER_START, this);
            CommandProcessor.instance().unregisterReportHandler(Constants.TK_CommandType.ADAPTER_STOP, this);
            CommandProcessor.instance().unregisterReportHandler(Constants.TK_CommandType.MON_VERIFYTERMINALSSTATUS, this);
        }

        #region ICommandHandler 成员

        public void handleCommand(ICommunicationMessage message)
        {
            switch (message.TK_CommandType)
            {
                case Constants.TK_CommandType.ADAPTER_START:
                    CommandProcessor.instance().DispatchCommand(_StartAdapter(message));
                    break;
                case Constants.TK_CommandType.ADAPTER_STOP:
                    CommandProcessor.instance().DispatchCommand(_StopAdapter(message));
                    break;
                case Constants.TK_CommandType.MON_VERIFYTERMINALSSTATUS:
                    _VerifyTerminalsStatus(message);
                    break;
                default:
                    break;
            }
        }

        #endregion

        void _VerifyTerminalsStatus(ICommunicationMessage message)
        {
            C5.HashDictionary<long, AdapterInfo> ads = new C5.HashDictionary<long, AdapterInfo>();
            AlarmManager.instance().GetAdaptersInfo(ads);

            foreach (C5.KeyValuePair<long, AdapterInfo> info in ads)
            {
                try
                {
                    System.Net.IPEndPoint end = new System.Net.IPEndPoint(System.Net.IPAddress.Parse(info.Value.Address), info.Value.ControllerPort);
                    if (end.Port == 0)
                    {
                        continue;
                    }
                    else
                    {
                        ICommClient comm = CommManager.instance().CreateCommClient<CommandMsgV2, TKMessageV2Extractor, TKMessageV2Encoder>("控制器",
                            end.Address.ToString(), end.Port, 30, false, false);

                        if (!comm.Start())
                        {
                            // remove adapter
                            AlarmManager.instance().RemoveAdapterInfo(info.Key);
                        }
                        else
                            comm.Close();
                    }
                }
                catch (Exception ex)
                {
                    DefLib.Util.Logger.Instance().SendLog(ex.ToString());
                }
            }

            CommandMsgV2 msg = new CommandMsgV2();
            msg.TK_CommandType = Constants.TK_CommandType.MON_GETTERMINALSINFO;
            msg.SeqID = message.SeqID;
            msg.SetValue("ClientID", message.GetValue("ClientID"));

            CommandProcessor.instance().DispatchCommand(msg);
        }

        CommandMsgV2 _StartAdapter(ICommunicationMessage message)
        {
            try
            {
                CommandMsgV2 resp = new CommandMsgV2();
                resp.TK_CommandType = Constants.TK_CommandType.RESPONSE;
                resp.SeqID = CommandProcessor.AllocateID();
                resp.SetValue("ClientID", message.GetValue("ClientID"));
                resp.SetValue(Constants.MSG_PARANAME_RESPONSE_TO, message.SeqID);


                long adapterid = Convert.ToInt64(message.GetValue(Constants.MSG_PARANAME_ADAPTER_ID));

                C5.HashDictionary<long, AdapterInfo> ads = new C5.HashDictionary<long, AdapterInfo>();
                AlarmManager.instance().GetAdaptersInfo(ads);
                if (!ads.Contains(adapterid))
                {
                    resp.SetValue(Constants.MSG_PARANAME_RESULT, "NOK");
                    resp.SetValue(Constants.MSG_PARANAME_REASON, "采集器不存在.");
                    return resp;
                }

                try
                {
                    CommandMsgV2 cmd = new CommandMsgV2();
                    cmd.SeqID = CommandProcessor.AllocateID();
                    cmd.TK_CommandType = Constants.TK_CommandType.ADAPTER_START;
                    cmd.SetValue("ClientID", adapterid);
                    cmd.SetValue(Constants.MSG_PARANAME_ADAPTER_NAME, ads[adapterid].Name);

                    System.Net.IPEndPoint end = new System.Net.IPEndPoint(System.Net.IPAddress.Parse(ads[adapterid].Address), ads[adapterid].ControllerPort);
                    if (end.Port == 0)
                    {
                        resp.SetValue(Constants.MSG_PARANAME_RESULT, "NOK");
                        resp.SetValue(Constants.MSG_PARANAME_REASON, "不可远程控制的采集器");
                    }
                    else
                    {
                        ICommClient comm = CommManager.instance().CreateCommClient<CommandMsgV2, TKMessageV2Extractor, TKMessageV2Encoder>("控制器",
                            end.Address.ToString(), end.Port, 30, false, false);

                        comm.Start();
                        ICommunicationMessage r2 = comm.SendCommand(cmd);
                        resp.SetValue(Constants.MSG_PARANAME_RESULT, r2.GetValue(Constants.MSG_PARANAME_RESULT));
                        comm.Close();
                    }
                }
                catch (Exception ex)
                {
                    resp.SetValue(Constants.MSG_PARANAME_RESULT, "NOK");
                    resp.SetValue(Constants.MSG_PARANAME_REASON, ex.Message);
                }

                return resp;
            }
            catch (Exception ex)
            {
                Logger.Instance().SendLog(ex.ToString());
                return null;
            }
        }

        CommandMsgV2 _StopAdapter(ICommunicationMessage message)
        {
            try
            {
                CommandMsgV2 resp = new CommandMsgV2();
                resp.TK_CommandType = Constants.TK_CommandType.RESPONSE;
                resp.SeqID = CommandProcessor.AllocateID();
                resp.SetValue("ClientID", message.GetValue("ClientID"));
                resp.SetValue(Constants.MSG_PARANAME_RESPONSE_TO, message.SeqID);


                long adapterid = Convert.ToInt64(message.GetValue(Constants.MSG_PARANAME_ADAPTER_ID));

                C5.HashDictionary<long, AdapterInfo> ads = new C5.HashDictionary<long, AdapterInfo>();
                AlarmManager.instance().GetAdaptersInfo(ads);
                if (!ads.Contains(adapterid))
                {
                    resp.SetValue(Constants.MSG_PARANAME_RESULT, "NOK");
                    resp.SetValue(Constants.MSG_PARANAME_REASON, "采集器不存在.");
                    return resp;
                }

                try
                {
                    CommandMsgV2 cmd = new CommandMsgV2();
                    cmd.SeqID = CommandProcessor.AllocateID();
                    cmd.TK_CommandType = Constants.TK_CommandType.ADAPTER_STOP;
                    cmd.SetValue("ClientID", adapterid);
                    cmd.SetValue(Constants.MSG_PARANAME_ADAPTER_NAME, ads[adapterid].Name);

                    System.Net.IPEndPoint end = new System.Net.IPEndPoint(System.Net.IPAddress.Parse(ads[adapterid].Address), ads[adapterid].ControllerPort);
                    if (end.Port == 0)
                    {
                        resp.SetValue(Constants.MSG_PARANAME_RESULT, "NOK");
                        resp.SetValue(Constants.MSG_PARANAME_REASON, "不可远程控制的采集器");
                    }
                    else
                    {
                        ICommClient comm = CommManager.instance().CreateCommClient<CommandMsgV2, TKMessageV2Extractor, TKMessageV2Encoder>("控制器",
                            end.Address.ToString(), end.Port, 30, false, false);
                        
                        comm.Start();

                        ICommunicationMessage r2 = comm.SendCommand(cmd);
                        resp.SetValue(Constants.MSG_PARANAME_RESULT, r2.GetValue(Constants.MSG_PARANAME_RESULT));

                        comm.Close();
                    }
                }
                catch (Exception ex)
                {
                    resp.SetValue(Constants.MSG_PARANAME_RESULT, "NOK");
                    resp.SetValue(Constants.MSG_PARANAME_REASON, ex.Message);
                }

                return resp;
            }
            catch (Exception ex)
            {
                Logger.Instance().SendLog(ex.ToString());
                return null;
            }
        }
    }
}
