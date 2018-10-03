using System;
using System.Collections.Generic;

namespace TK_AlarmManagement
{
    interface ICommManager
    {
        CommClient<MSGTYPE> CreateCommClient<MSGTYPE, EXTRACTOR, ENCODER>(string name, string ip, int port, int synccmd_timeout, bool keepalive, bool compress)
            where MSGTYPE : ICommunicationMessage, new()
            where EXTRACTOR : IMessageExtractor, new()
            where ENCODER : IMessageEncoder, new();

        CommServer<T, MSGTYPE, EXTRACTOR, ENCODER> CreateCommServer<T, MSGTYPE, EXTRACTOR, ENCODER>(
            string name, List<Constants.TK_CommandType> acceptedcommands, List<Constants.TK_CommandType> supercmds,
            int port, int maxclients, int synccmd_timeout, bool keepalive, bool compress)
            where T : ICommandInterpreter, new()
            where MSGTYPE : ICommunicationMessage, new()
            where EXTRACTOR : IMessageExtractor, new()
            where ENCODER : IMessageEncoder, new();

        void DisposeClient(ICommClient client);
        void DisposeServer(string name);
        ICommServer GetServer(string name);
        event LogHandler LogReceived;
        bool Start();
        bool Stop();
    }
}
