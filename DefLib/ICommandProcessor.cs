using System;
namespace TK_AlarmManagement
{
    public interface ICommandProcessor
    {
        void DispatchCommand(ICommunicationMessage cm);
        void DispatchCommands(System.Collections.Generic.List<ICommunicationMessage> msgs);
        void registerReportHandler(Constants.TK_CommandType type, ICommandHandler handler, System.Collections.Generic.Dictionary<Constants.TK_CommandType, byte> supercommands);
        void Start(bool isServer);
        void Stop();
        void unregisterReportHandler(Constants.TK_CommandType type, ICommandHandler handler);
    }
}
