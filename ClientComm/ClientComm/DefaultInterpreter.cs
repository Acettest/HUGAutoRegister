using System;
using System.Collections.Generic;
using System.Text;

namespace TK_AlarmManagement
{
    public class DefaultInterpreter : ICommandInterpreter
    {
        private ICommunicator m_Communicator = null;
        #region ICommandInterpreter 成员

        public ICommunicator CommunicatorObj
        {
            get
            {
                return m_Communicator;
            }
            set
            {
                m_Communicator = value;
            }
        }

        public bool Start()
        {
            return true;
        }

        public bool Stop()
        {
            return true;
        }

        public ICommunicationMessage ImmediatelyInterpret(ICommunicationMessage cm)
        {
            return null;
        }

        public bool DelayedInterpret(ICommunicationMessage cm)
        {
            return false;
        }

        #endregion
    }
}
