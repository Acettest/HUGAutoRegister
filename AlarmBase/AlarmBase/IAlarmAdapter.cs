using System;
using System.Collections.Generic;
using TK_AlarmManagement;

namespace TK_AlarmManagement
{

	public delegate void AlarmHandler(List<TKAlarm> lstAlarm);
    public delegate void StateChangeHandler(string name, string state);

	/// <summary>
	/// 厂商适配器接口
	/// </summary>
	public interface IAlarmAdapter
	{
		/// <summary>
		/// 收到告警事件
		/// </summary>
		//event AlarmHandler AlarmReceived;
		event LogHandler LogReceived;
        event StateChangeHandler StateChanged;

        //ftth
        int SvrID
        {
            get;
            set;
        }

        string EncodingStr
        {
            get;
            set;
        }

        string Name
        {
            get;
            set;
        }

        int Interval
        {
            get;
            set;
        }

        int ControllerPort
        {
            get;
            set;
        }

		/// <summary>
		/// 初始化适配器
		/// </summary>
		void Init(ICommClient comm);

		/// <summary>
		/// 开始采集告警
		/// </summary>
		void Start();

		/// <summary>
		/// 停止采集
		/// </summary>
		void Stop();

        Dictionary<string, string> OMCInfo
        {
            get;
        }

        /// <summary>
        /// PendingFlag使系统趋于运行
        /// </summary>
        long PendingRunFlag
        {
            get;
        }

        /// <summary>
        /// CompleteFlag表示系统已经处于的稳定状态
        /// </summary>
        long CompleteRunFlag
        {
            get;
        }
	}
}
