using System;
namespace TK_AlarmManagement
{
    public delegate void ConnectionBrokenHandler(ICommunicator ptr, string broken_info);

    public interface ICommunicator : ILogHandler
    {
        System.Net.IPEndPoint RemoteEP { get; }
        long ClientID { get; }
        void endWork();
        void enqueueDelayedMessages(ICommunicationMessage msg);
        void enqueueMessage(ICommunicationMessage msg);
        event ConnectionBrokenHandler onConnectionBroken;
        void startWork();
    }
}
