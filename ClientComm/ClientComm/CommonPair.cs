using System;
using System.Collections.Generic;
using System.Text;

namespace TK_AlarmManagement
{
    public class CommonPair<F, S>
    {
        F _first = default(F);
        S _second = default(S);

        public CommonPair(F first, S second)
        {
            First = first;
            Second = second;
        }

        public F First
        {
            get { return _first; }
            set { _first = value; }
        }

        public S Second
        {
            get { return _second; }
            set { _second = value; }
        }
    }
}
