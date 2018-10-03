using System;
using System.Collections.Generic;
using System.Text;

namespace TK_AlarmManagement
{
    public class CommException : System.Exception
    {
        public CommException()
            : base()
        {
        }

        public CommException(string info)
            : base(info)
        {
        }
    }
}
