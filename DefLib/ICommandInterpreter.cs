using System;
using System.Collections.Generic;
using System.Text;

namespace TK_AlarmManagement
{
    public interface ICommandInterpreter
    {
        ICommunicator CommunicatorObj
        {
            get;
            set;
        }

        bool Start();
        bool Stop();
        ICommunicationMessage ImmediatelyInterpret(ICommunicationMessage cm);
        bool DelayedInterpret(ICommunicationMessage cm);
    }
}
