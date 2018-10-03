using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace TK_AlarmManagement
{
	/// <summary>
	/// 通讯包类，负责信息的打包和解包
	/// </summary>
    /// 
	public interface ICommunicationMessage
	{
        string EncodeMode
        {
            get;
            set;
        }

		Constants.TK_CommandType TK_CommandType
		{
			get;
			set;
		}

		long SeqID
		{
			get;
			set;
		}

        //System.Threading.AutoResetEvent Mutex
        //{
        //    get;
        //}

        void SetValue(string key, object value);
        object GetValue(string key);
        bool RemoveKey(string key);
        bool Contains(string key);
        C5.ICollectionValue<string> GetKeys();

		string encode();
		bool decode(string msg);

        byte[] ToNetBuf();
        bool FromNetBuf(byte[] buf);

        ICommunicationMessage clone(); // 浅层拷贝
	}
}
