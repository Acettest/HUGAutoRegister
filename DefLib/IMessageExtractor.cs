using System;
using System.Collections.Generic;

namespace TK_AlarmManagement
{
    public interface IMessageExtractor
    {
        List<ICommunicationMessage> extractMessages(byte[] package, ref byte[] rest);

        bool Compressed
        {
            get;
            set;
        }
    }
}
