using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Threading;

namespace TK_AlarmManagement
{
    public class PluginServer : System.MarshalByRefObject, IPluginServer, ICommandHandler
    {
        Dictionary<string, CommonPair<IPlugin, List<Constants.TK_CommandType>>> m_Plugins = new Dictionary<string, CommonPair<IPlugin, List<Constants.TK_CommandType>>>();
        Dictionary<Constants.TK_CommandType, List<IPlugin>> m_Handlers = new Dictionary<Constants.TK_CommandType, List<IPlugin>>();

        System.Timers.Timer m_TestAliveTimer = new System.Timers.Timer(1000);

        public PluginServer()
        {
            m_TestAliveTimer.Elapsed += new System.Timers.ElapsedEventHandler(m_TestAliveTimer_Elapsed);

            m_TestAliveTimer.Start();
        }

        public void Stop()
        {
            m_TestAliveTimer.Stop();
            m_TestAliveSignal.WaitOne();
        }

        ManualResetEvent m_TestAliveSignal = new ManualResetEvent(true);

        bool m_bInAliveTimer = false;
        object m_LockTimer = new object();
        void m_TestAliveTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            lock (m_LockTimer)
            {
                if (m_bInAliveTimer)
                    return;

                m_bInAliveTimer = true;
            }

            try
            {
                m_TestAliveSignal.Reset();

                if (!m_TestAliveTimer.Enabled)
                    return;

                List<CommonPair<IPlugin, string>> plugins = new List<CommonPair<IPlugin, string>>();
                lock (m_Plugins)
                {
                    foreach (KeyValuePair<string, CommonPair<IPlugin, List<Constants.TK_CommandType>>> p in m_Plugins)
                        plugins.Add(new CommonPair<IPlugin, string>(p.Value.First, p.Key));
                }

                foreach (CommonPair<IPlugin, string> p in plugins)
                {
                    try
                    {
                        if (!p.First.IsActive)
                            UnregisterPlugin(p.Second);
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            UnregisterPlugin(p.Second);
                        }
                        catch { }
                    }
                }
            }
            finally
            {
                m_TestAliveSignal.Set();
                lock (m_LockTimer)
                    m_bInAliveTimer = false;
            }
        }

        #region IPluginServer 成员
        public Dictionary<long, AdapterInfo> GetTerminalInfos()
        {
            Dictionary<long, AdapterInfo> infos = new Dictionary<long, AdapterInfo>();
            AlarmManager.instance().GetAdaptersInfo(infos);
            return infos;
        }

        /// <summary>
        /// 派发消息并分配流水号
        /// </summary>
        /// <param name="msg"></param>
        public void DispatchCommand(ICommunicationMessage msg)
        {
            if (msg.SeqID == 0)
                msg.SeqID = CommandProcessor.AllocateID();
            CommandProcessor.instance().DispatchCommand(msg);
        }
        #endregion

        public override Object InitializeLifetimeService()
        {
            return null;
        }


        #region IPluginServer 成员

        public void RegisterPlugin(string plugin_name, string url, List<Constants.TK_CommandType> handlingcmds)
        {
            try
            {
                lock (m_Plugins)
                {
                    if (m_Plugins.ContainsKey(plugin_name))
                        UnregisterPlugin(plugin_name);
                }

                    IPlugin plugin = (IPlugin)Activator.GetObject(typeof(IPlugin), url);

                lock (m_Handlers)
                {
                    foreach (Constants.TK_CommandType cmd in handlingcmds)
                    {
                        CommandProcessor.instance().registerReportHandler(cmd, this, null);

                        List<IPlugin> plugins;
                        if (m_Handlers.ContainsKey(cmd))
                        {
                            plugins = m_Handlers[cmd];
                        }
                        else
                            plugins = (m_Handlers[cmd] = new List<IPlugin>());

                        plugins.Add(plugin);
                    }
                }

                lock (m_Plugins)
                {
                    m_Plugins.Add(plugin_name, new CommonPair<IPlugin, List<Constants.TK_CommandType>>(plugin, handlingcmds));
                }

                Logger.Instance().SendLog("已安装: " + plugin_name);
            }
            catch (Exception ex)
            {
                Logger.Instance().SendLog(ex.ToString());
            }
        }

        public void UnregisterPlugin(string plugin_name)
        {
            try
            {
                CommonPair<IPlugin, List<Constants.TK_CommandType>> cmds = null;
                lock (m_Plugins)
                {
                    if (!m_Plugins.ContainsKey(plugin_name))
                        return;

                    cmds = m_Plugins[plugin_name];
                    m_Plugins.Remove(plugin_name);

                    try
                    {
                        cmds.First.Kill();
                    }
                    catch { }
                }

                foreach (Constants.TK_CommandType cmd in cmds.Second)
                {
                    CommandProcessor.instance().unregisterReportHandler(cmd, this);
                }

                Logger.Instance().SendLog("已卸载: " + plugin_name);
            }
            catch (Exception ex)
            {
                Logger.Instance().SendLog(ex.ToString());
            }
        }

        #endregion

        #region ICommandHandler 成员

        public void handleCommand(ICommunicationMessage message)
        {
            lock (m_Handlers)
            {
                if (m_Handlers.ContainsKey(message.TK_CommandType))
                {
                    foreach (IPlugin plugin in m_Handlers[message.TK_CommandType])
                    {
                        try
                        {
                            plugin.handleCommand(message);
                        }
                        catch { }
                    }

                }
            }
        }

        #endregion
    }
}
