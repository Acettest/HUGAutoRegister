using System;
using System.Collections.Generic;
using System.Text;

namespace TK_AlarmManagement
{
    public interface ITempStorageHelper
    {
        void Init();
        void Store<T>(long key, T o);
        void Clear<T>(long key);
        List<T> RestoreAllAndClear<T>();
    }
}
