using System;
using System.Collections.Generic;


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
		event AlarmHandler AlarmReceived;
		event LogHandler LogReceived;
        //event StateChangeHandler StateChanged;

		/// <summary>
		/// 初始化适配器
		/// </summary>
		void Init();

		/// <summary>
		/// 开始采集告警
		/// </summary>
		void Start();

		/// <summary>
		/// 停止采集
		/// </summary>
		void Stop();

        //Dictionary<string, string> OMCInfo
        //{
        //    get;
        //}
	}
}
