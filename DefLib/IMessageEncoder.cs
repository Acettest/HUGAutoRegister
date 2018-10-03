using System;
using System.Collections.Generic;
using System.Text;

namespace TK_AlarmManagement
{
    public interface IMessageEncoder
    {
        byte[] encodeMessage(string message, Encoding encode, bool compress);
        byte[] encodeMessage(ICommunicationMessage msg, bool compress);
        byte[] encodeMessages(List<ICommunicationMessage> msgs, bool compress);
    }
}
