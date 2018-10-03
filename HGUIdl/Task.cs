#region 描述
//-----------------------------------------------------------------------------
// 文 件 名: Task
// 作    者：d.w
// 创建时间：2014/6/12 16:12:12
// 描    述：
// 版    本：1.0.0.0
//-----------------------------------------------------------------------------
// 历史更新纪录
//-----------------------------------------------------------------------------
// 版    本：           修改时间：           修改人：           
// 修改内容：
//-----------------------------------------------------------------------------
// Copyright (C) 2009-2014 www.chaselwang.com . All Rights Reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using System.Collections.Generic;
using System.Text;

namespace HGU.Idl
{

    public enum TaskType
    {
        NewBroadband = 0,           //宽带新装
        NewIMS = 1,                 //IMS新装
        RelocateBroadband = 2,      //宽带移机 
        AddBroadband = 3,           //宽带加装
        AddIMS = 4,                 //IMS加装
        DelONU = 5,                 //ONU拆机
        DelBroadband = 6,           //宽带拆机
        DelIMS = 7,                 //IMS拆机
        RelocateIMS = 8,            //IMS移机 
        SameInstall = 9,             //同装
        NewIPTV = 10,              //IPTV新装
        AddIPTV = 11,              //IPTV加装
        DelIPTV = 12,              //IPTV拆机
        RelocateIPTV = 13,          //IPTV移机 
        None = 14
    };

    //public enum BusinessType
    //{
    //    NewBroadband = 0,           //宽带新装
    //    NewIMS = 1,                 //IMS新装
    //    AddBroadband = 3,           //宽带加装
    //    AddIMS = 4,                 //IMS加装
    //    DelONU = 5,                 //ONU拆机
    //    DelBroadband = 6,           //宽带拆机
    //    DelIMS = 7                 //IMS拆机
    //}

    public enum TaskStatus
    {
        NotStart = 0, Executing = 1, Succeed = 2, Fail = 3
    };

    /// <summary>
    /// nms中有一个当前工单currentTask和回滚工单rollbackTask
    /// IsRelocateTask标记是否由移机工单拆分而来，
    /// 影响到返回结果的处理
    /// 缺各种时间
    /// </summary>
    public class Task
    {
        #region IFields
        private long m_id;
        private string m_taskID;                                                  //工单号 上级网管
        private TaskType m_type;                                       //工单类型
        //private BusinessType m_businessType;                            //业务类型
        private TaskStatus m_status;                                   //工单状态
        private OMC m_omc;
        private string m_oltID;                                        //OLT信息
        private string m_oltName;
        private string m_ponID;                                        //PON口信息
        private string m_onuID;                                        //ONU信息 LOID模式
        private string m_onuType;                                      //ONUTYPE
        private int m_svlan;                                          //SVLAN
        private int m_cvlan;                                           //CVLAN
        private int m_uvlan;                                           //业务UVLAN
        private int m_muvlan;                                           //管理业务MUVLAN
        private int m_mvlan;                                           //管理vlan
        private int m_feNumber;                                          //ONU FE口数
        private int m_potsNumber;                                        //ONU POTS口数
        private string m_phone;                                      //上网账号、电话号码
        private bool m_responseBoss = false;                           //是否已回复boss
        private string m_responseMsg;                                  //回复boss信息
        private string m_responseXML;                                  //回复boss报文内容
        private string m_bossReply;                                    //boss返回信息
        //各种时间
        private DateTime m_receiveTime;                                //接收时间
        private DateTime m_executeTime;                                //执行时间
        private DateTime m_completeTime;                               //完成时间
        private DateTime m_responseTime;                               //回复时间
        //
        private bool m_rollback = false;                               //是否已回滚
        private bool m_netInterrupt;                                   //网络异常导致的失败处理和一般失败不同
        private bool m_netDelay;                                       //网络中断导致的超时

        private bool m_isRelocateTask;                                 //是否由移机工单拆分
        private bool m_isRollbackTask = false;                         //是否是回滚的工单
        private bool m_isRollbackHisTask;                              //是否是回滚的历史工单
        private ErrorDesc m_error = ErrorDesc.None;
        private bool m_oltOffline = false;                             //olt offline
        private bool m_hwIMSDelay = false;

        private TaskType m_oldTaskType;                                 //华为回滚工单使用 原工单为新装 则不做数据核查直接删除ONU

        private string m_isContainIMS;//是否含有IMS业务
        private int m_imssvlan;   //IMSsvlan
        private int m_imscvlan;  //IMScvlan
        private int m_imsUV;    //IMS业务vlan

        private string m_isContainIPTV;//是否含有IPTV业务
        private int m_iptvsvlan;   //IPTVsvlan
        private int m_iptvcvlan;  //IPTVcvlan
        private int m_iptvUV;    //IPTV业务vlan
        #endregion

        #region IProperties
        public string TaskID
        {
            get { return m_taskID; }
        }

        /// <summary>
        /// 工单类型
        /// </summary>
        public TaskType Type
        {
            get { return m_type; }
        }

        public string City
        {
            get { return m_omc.City; }
        }

        public string Manufacturer
        {
            get { return m_omc.Manufacturer; }
        }
        public string OmcName
        {
            get { return m_omc.OmcName; }
        }
        public string OltID
        {
            get { return m_oltID; }
        }
        public string OltName
        {
            get { return m_oltName; }
        }

        public string PonID
        {
            get { return m_ponID; }
        }

        public string OnuID
        {
            get { return m_onuID; }
        }

        public string OnuType
        {
            get { return m_onuType; }
        }

        public int Svlan
        {
            get { return m_svlan; }
        }

        public int Cvlan
        {
            get { return m_cvlan; }
        }

        public int Uvlan
        {
            get { return m_uvlan; }
        }

        public int Muvlan
        {
            get { return m_muvlan; }
        }

        public int Mvlan
        {
            get { return m_mvlan; }
        }
        public int FENumber
        {
            get { return m_feNumber; }
        }
        public int POTSNumber
        {
            get { return m_potsNumber;}
        }

        public DateTime ReceiveTime
        {
            get { return m_receiveTime; }
        }

        //get set锁，其他偷懒
        public DateTime ExecuteTime
        {
            get
            {
                DateTime dt;
                lock (this)
                {
                    dt = m_executeTime;
                }
                return dt;
            }
            set
            {
                lock (this)
                {
                    m_executeTime = value;
                }
            }
        }

        public bool Rollback
        {
            get
            {
                bool b;
                lock (this)
                {
                    b = m_rollback;
                }
                return b;
            }
            set
            {
                lock (this)
                {
                    m_rollback = value;
                }
            }
        }

        public bool IsRollbackTask
        {
            get
            {
                bool b;
                lock (this)
                {
                    b = m_isRollbackTask;
                }
                return b;
            }
            set
            {
                lock (this)
                {
                    m_isRollbackTask = value;
                }
            }
        }

        public bool ResponseBoss
        {
            get
            {
                bool b;
                lock (this)
                { b = m_responseBoss; }
                return b;
            }
            set
            {
                lock (this)
                {
                    m_responseBoss = value;
                }
            }
        }

        public string ResponseMsg
        {
            get
            {
                string s = string.Empty;
                lock (this)
                {
                    s = m_responseMsg;
                }
                return s;
            }
            set
            {
                lock (this)
                {
                    m_responseMsg = value;
                }
            }
        }

        public string BossReply
        {
            get
            {
                string s;
                lock (this)
                {
                    s = m_bossReply;
                }
                return s;
            }
            set
            {
                lock (this)
                {
                    m_bossReply = value;
                }
            }
        }

        public DateTime CompleteTime
        {
            get
            {
                DateTime dt;
                lock (this)
                {
                    dt = m_completeTime;
                }
                return dt;
            }
            set
            {
                lock (this)
                {
                    m_completeTime = value;
                }
            }
        }

        public DateTime ResponseTime
        {
            get
            {
                DateTime dt;
                lock (this)
                {
                    dt = m_responseTime;
                }
                return dt;
            }
            set
            {
                lock (this)
                {
                    m_responseTime = value;
                }
            }
        }

        public TaskStatus Status
        {
            get
            {
                TaskStatus ts;
                lock (this)
                {
                    ts = m_status;
                }
                return ts;
            }
            set
            {
                lock (this)
                {
                    m_status = value;
                }
            }
        }



        public bool NetInterrupt
        {
            get
            {
                bool b;
                lock (this)
                {
                    b = m_netInterrupt;
                }
                return b;
            }
            set
            {
                lock (this)
                {
                    m_netInterrupt = value;
                }
            }
        }

        public long Id
        {
            get
            {
                long l;
                lock (this)
                {
                    l = m_id;
                }
                return l;
            }
            set
            {
                lock (this)
                {
                    m_id = value;
                }
            }
        }

        public bool IsRollbackHisTask
        {
            get
            {
                bool b = false;
                lock (this)
                {
                    b = m_isRollbackHisTask;
                }
                return b;
            }
            set
            {
                lock (this)
                {
                    m_isRollbackHisTask = value;
                }
            }
        }

        public bool NetDelay
        {
            get
            {
                bool b = false;
                lock (this)
                {
                    b = m_netDelay;
                }
                return b;
            }
            set
            {
                lock (this)
                {
                    m_netDelay = value;
                }
            }
        }

        public ErrorDesc Error
        {
            get { return m_error; }
            set { m_error = value; }
        }

        public bool OltOffline
        {
            get { return m_oltOffline; }
            set { m_oltOffline = value; }
        }

        public string Phone
        {
            get { return m_phone; }
            set { m_phone = value; }
        }

        public bool IsRelocateTask
        {
            get { return m_isRelocateTask; }
            set { m_isRelocateTask = value; }
        }

        //public BusinessType BusinessType
        //{
        //    get { return m_businessType; }
        //    set { m_businessType = value; }
        //}

        public string ResponseXML
        {
            get { return m_responseXML; }
            set { m_responseXML = value; }
        }

        public bool HwIMSDelay
        {
            get { return m_hwIMSDelay; }
            set { m_hwIMSDelay = value; }
        }

        public TaskType OldTaskType
        {
            get { return m_oldTaskType; }
            set { m_oldTaskType = value; }
        }

        public string IsContainIMS
        {
            get { return m_isContainIMS; }
            set { m_isContainIMS = value; }
        }
        public int IMSSvlan
        {
            get { return m_imssvlan; }
        }
        public int IMSCvlan
        {
            get { return m_imscvlan; }
        }
        public int IMSUV
        {
            get { return m_imsUV; }
        }
        public string IsContainIPTV
        {
            get { return m_isContainIPTV; }
            set { m_isContainIPTV = value; }
        }
        public int IPTVSvlan
        {
            get { return m_iptvsvlan; }
        }
        public int IPTVCvlan
        {
            get { return m_iptvcvlan; }
        }
        public int IPTVUV
        {
            get { return m_iptvUV; }
        }

        #endregion

        #region IConstructors
        /// <summary>
        /// 需完整性约束
        /// 还没有格式约束,如ip和port格式
        /// </summary>
        /// <param name="taskID"></param>
        /// <param name="type"></param>
        /// <param name="city"></param>
        /// <param name="manufacturer"></param>
        /// <param name="omcName"></param>
        /// <param name="oltID"></param>
        /// <param name="ponID"></param>
        /// <param name="onuID"></param>
        /// <param name="onuPort"></param>
        /// <param name="onuType"></param>
        /// <param name="svlan"></param>
        /// <param name="cvlan"></param>
        /// <param name="isRelocateTask"></param>
		public Task(string taskID, TaskType type, string city, string manufacturer, string omcName, string oltID,string oltName, string ponID, 
            string onuID, string onuType, int svlan, int cvlan, string phone, DateTime receiveTime, int muvlan, int uvlan, int mvlan, int feNumber, 
            int potsNumber,string IsContainIMS, int imsSvlan, int imsCvlan, int imsUV, string IsContainIPTV, int iptvSvlan, int iptvCvlan, int iptvUV)
		{
			if (city == null || city == "" ||
				manufacturer == null || manufacturer == "" ||
				omcName == null || omcName == "")
				throw new Exception("工单完整性约束失败:" +
					ArgsToString(taskID, type, city, manufacturer, omcName, oltID, ponID, onuID, onuType,
								 svlan, cvlan, phone, muvlan, uvlan, mvlan, feNumber, potsNumber));
			switch (type)
			{
				case TaskType.NewBroadband:
				case TaskType.AddBroadband:
					if (oltID == null || oltID == "" ||
						ponID == null || ponID == "" ||
						onuID == null || onuID == "" ||
						svlan == 0 || cvlan == 0 || mvlan==0 ||
                        feNumber == -1 || potsNumber == -1)
						throw new Exception("工单完整性约束失败:" +
							ArgsToString(taskID, type, city, manufacturer, omcName, oltID, ponID, onuID, onuType,
										 svlan, cvlan, phone, muvlan, uvlan, mvlan, feNumber, potsNumber));
					break;
				case TaskType.NewIMS:
				case TaskType.AddIMS:
					if (oltID == null || oltID == "" ||
						ponID == null || ponID == "" ||
						onuID == null || onuID == "" ||
						svlan == 0 || cvlan == 0 ||
						phone == null || phone == "")
						throw new Exception("工单完整性约束失败:" +
							ArgsToString(taskID, type, city, manufacturer, omcName, oltID, ponID, onuID, onuType,
										 svlan, cvlan, phone, muvlan, uvlan, mvlan, feNumber, potsNumber));
					break;
				case TaskType.DelONU:
					if (oltID == null || oltID == "" ||
						ponID == null || ponID == "" ||
						onuID == null || onuID == "")
						throw new Exception("工单完整性约束失败:" +
							ArgsToString(taskID, type, city, manufacturer, omcName, oltID, ponID, onuID, onuType,
										 svlan, cvlan, phone, muvlan, uvlan, mvlan, feNumber, potsNumber));
					break;
				case TaskType.DelBroadband:
				case TaskType.DelIMS:
					if (oltID == null || oltID == "" ||
						ponID == null || ponID == "" ||
						onuID == null || onuID == "" ||
						svlan == 0 || cvlan == 0)
						throw new Exception("工单完整性约束失败:" +
							ArgsToString(taskID, type, city, manufacturer, omcName, oltID, ponID, onuID, onuType,
										 svlan, cvlan, phone, muvlan, uvlan, mvlan, feNumber, potsNumber));
					break;
				default:
					break;
			}
			this.m_taskID = taskID;
			this.m_type = type;
			//this.m_city = city;
			//this.m_manufacturer = manufacturer;
			//this.m_omcName = omcName;
			m_omc = new OMC(city, manufacturer, omcName);
			this.m_oltID = oltID;
			this.m_oltName = oltName;
			this.m_ponID = ponID;
			this.m_onuID = onuID;
			this.m_onuType = onuType;
			this.m_svlan = svlan;
			this.m_cvlan = cvlan;
			this.m_status = TaskStatus.NotStart;
			this.m_phone = phone;
			this.m_muvlan = muvlan;
			this.m_uvlan = uvlan;
            this.m_mvlan = mvlan;
			this.m_feNumber = feNumber;
			this.m_potsNumber = potsNumber;
			this.m_receiveTime = receiveTime;
			this.m_hwIMSDelay = false;
            this.m_isContainIMS = IsContainIMS;
            this.m_imssvlan = imsSvlan;
            this.m_imscvlan = imsCvlan;
            this.m_imsUV = imsUV;

            this.m_isContainIPTV = IsContainIPTV;
            this.m_iptvsvlan = iptvSvlan;
            this.m_iptvcvlan = iptvCvlan;
            this.m_iptvUV = iptvUV;
		}

        /// <summary>
        /// 宽带新装
        /// </summary>
        /// <param name="taskID"></param>
        /// <param name="type"></param>
        /// <param name="city"></param>
        /// <param name="manufacturer"></param>
        /// <param name="omcName"></param>
        /// <param name="oltID"></param>
        /// <param name="ponID"></param>
        /// <param name="onuID"></param>
        /// <param name="onuPort"></param>
        /// <param name="onuType"></param>
        /// <param name="svlan"></param>
        /// <param name="cvlan"></param>
        /// <param name="phone"></param>
        /// <param name="receiveTime"></param>
        public Task(string taskID, TaskType type, string city, string manufacturer, string omcName, string oltID,
                     string ponID, string onuID, string onuType, int svlan, int cvlan, string phone, DateTime receiveTime,
                     int muvlan, int uvlan, int mvlan, int feNumber, int potsNumber, string IsContainIMS, int imsSvlan, int imsCvlan, int imsUV) :
            this(taskID, type, city, manufacturer, omcName, oltID, null, ponID, onuID, onuType, svlan, cvlan, phone, receiveTime, muvlan,
                       uvlan, mvlan, feNumber, potsNumber, IsContainIMS, imsSvlan, imsCvlan, imsUV, "NULL", 0, 0, 0)
        { }

        /// <summary>
        /// 加装
        /// </summary>
        /// <param name="taskID"></param>
        /// <param name="type"></param>
        /// <param name="city"></param>
        /// <param name="manufacturer"></param>
        /// <param name="omcName"></param>
        /// <param name="oltID"></param>
        /// <param name="ponID"></param>
        /// <param name="onuID"></param>
        /// <param name="onuType"></param>
        /// <param name="svlan"></param>
        /// <param name="cvlan"></param>
        /// <param name="phone"></param>
        /// <param name="receiveTime"></param>
        /// <param name="muvlan"></param>
        /// <param name="uvlan"></param>
        /// <param name="mvlan"></param>
        /// <param name="feNumber"></param>
        /// <param name="potsNumber"></param>
        public Task(string taskID, TaskType type, string city, string manufacturer, string omcName, string oltID,
                    string ponID, string onuID, string onuType, int svlan, int cvlan, string phone,DateTime receiveTime,
                    int muvlan, int uvlan, int feNumber, int potsNumber, string IsContainIMS, int imsSvlan, int imsCvlan, int imsUV) :
                    this(taskID, type, city, manufacturer, omcName, oltID, null, ponID, onuID, onuType,svlan, cvlan, phone, receiveTime,
                    muvlan, uvlan, -1, feNumber, potsNumber, IsContainIMS, imsSvlan, imsCvlan, imsUV, "NULL", 0, 0, 0)
        { }

        /// <summary>
        /// ONU拆机
        /// </summary>
        /// <param name="taskID"></param>
        /// <param name="type"></param>
        /// <param name="city"></param>
        /// <param name="manufacturer"></param>
        /// <param name="omcName"></param>
        /// <param name="oltID"></param>
        /// <param name="ponID"></param>
        /// <param name="onuID"></param>
        /// <param name="onuPort"></param>
        /// <param name="receiveTime"></param>
        public Task(string taskID, TaskType type, string city, string manufacturer, string omcName, string oltID, string ponID,
                    string onuID, string phone, DateTime receiveTime, string IsContainIMS, int imsSvlan, int imsCvlan, int imsUV) :
               this(taskID, type, city, manufacturer, omcName, oltID, ponID, onuID,null,-1,-1,phone, receiveTime,-1,-1,-1,-1,-1,"NULL",-1,-1,0)

        { }


        /// <summary>
        /// 单拆业务
        /// </summary>
        /// <param name="taskID"></param>
        /// <param name="type"></param>
        /// <param name="city"></param>
        /// <param name="manufacturer"></param>
        /// <param name="omcName"></param>
        /// <param name="oltID"></param>
        /// <param name="ponID"></param>
        /// <param name="onuID"></param>
        /// <param name="onuPort"></param>
        /// <param name="svlan"></param>
        /// <param name="cvlan"></param>
        /// <param name="phone"></param>
        /// <param name="receiveTime"></param>
        public Task(string taskID, TaskType type, string city, string manufacturer,string omcName,
                    string oltID, string ponID, string onuID, int svlan, int cvlan,string phone,DateTime receiveTime) :
               this(taskID, type, city, manufacturer,omcName, oltID, ponID, onuID,null, svlan, cvlan, phone, receiveTime,
                     -1,-1,-1,-1,-1,"NULL",-1,-1,0)
        { }

        //用type为null检测
        static public string ArgsToString(string taskID, TaskType type, string city, string manufacturer,
             string omcName, string oltID, string ponID, string onuID, string onuType,int svlan, int cvlan, 
            string phone, int muvlan, int uvlan, int mvlan,int feNumber,int potsNumber)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("taskID=" + taskID.ToString());
            sb.Append(";type=" + Enum.GetName(typeof(TaskType), type));
            sb.Append(";city=" + (city == null ? "null" : city));
            sb.Append(";manufacturer=" + (manufacturer == null ? "null" : manufacturer));
            sb.Append(";omcName=" + (omcName == null ? "null" : omcName));
            sb.Append(";oltID=" + (oltID == null ? "null" : oltID));
            sb.Append(";ponID=" + (ponID == null ? "null" : ponID));
            sb.Append(";onuID=" + (onuID == null ? "null" : onuID));
            sb.Append(";onuType=" + (onuType == null ? "null" : onuType));
            sb.Append(";svlan=" + svlan.ToString());
            sb.Append(";cvlan=" + cvlan.ToString());
            sb.Append(";phone=" + (phone == null ? "null" : phone));
            sb.Append(";muvlan=" + muvlan.ToString());
            sb.Append(";ywvlan=" + uvlan.ToString());
            sb.Append(";mvlan=" + mvlan.ToString());
            sb.Append(";feNumber=" + feNumber.ToString());
            sb.Append(";potsNumber=" + potsNumber.ToString());
            return sb.ToString();
        }
        #endregion

        #region IMethods
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("taskID=" + m_taskID);
            sb.Append(";type=" + Enum.GetName(typeof(TaskType), m_type));
            sb.Append(";city=" + (m_omc.City == null ? "null" : m_omc.City));
            sb.Append(";manufacturer=" + (m_omc.Manufacturer == null ? "null" : m_omc.Manufacturer));
            sb.Append(";omcName=" + (m_omc.OmcName == null ? "null" : m_omc.OmcName));
            sb.Append(";oltID=" + (m_oltID == null ? "null" : m_oltID));
            sb.Append(";ponID=" + (m_ponID == null ? "null" : m_ponID));
            sb.Append(";onuID=" + (m_onuID == null ? "null" : m_onuID));
            sb.Append(";onuType=" + (m_onuType == null ? "null" : m_onuType));
            sb.Append(";svlan=" + m_svlan.ToString());
            sb.Append(";cvlan=" + m_cvlan.ToString());
            sb.Append(";muvlan=" + m_muvlan.ToString());
            sb.Append(";ywvlan=" + m_uvlan.ToString());
            sb.Append(";mvlan=" + m_mvlan.ToString());
            sb.Append(";feNumber=" + m_feNumber.ToString());
            sb.Append(";potsNumber=" + m_potsNumber.ToString());
            return sb.ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (object.ReferenceEquals(this, obj))
                return true;
            if (this.GetType() != obj.GetType())
                return false;
            return CompareMembers(this, obj as Task);
        }

        public static bool CompareMembers(Task left, Task right)
        {
            if (left == null || right == null)
                return false;
            if (left.m_taskID != right.m_taskID)
                return false;
            return true;
        }

        /// <summary>
        /// 现在只有开机业务有回滚
        /// </summary>
        /// <returns></returns>
        public bool NeedRollback()
        {
            //add
            bool rollback = false;
            lock (this)
            {
                if (m_type == TaskType.NewBroadband || m_type == TaskType.NewIMS || m_type == TaskType.NewIPTV)
                    if (m_status == TaskStatus.Fail || m_netInterrupt)
                        rollback = true;
            }
            return rollback;
        }

        public Task GetRollbackTask()
        {
            if (NeedRollback() == false)
                return null;
            //
            Task rollbackTask;
            if (m_isRelocateTask)
            {
                rollbackTask = SQLUtil.GetRelocateRollbackTask(m_taskID);
            }
            else
            {
                if (m_type == TaskType.NewBroadband)
                {
                    rollbackTask = new Task(m_taskID, TaskType.DelBroadband, m_omc.City, m_omc.Manufacturer, m_omc.OmcName, m_oltID,
                                          null, m_ponID, m_onuID, null, m_svlan, m_cvlan, m_phone, DateTime.Now, m_muvlan,
                                          m_uvlan, m_mvlan, m_feNumber, m_potsNumber, m_isContainIMS, m_imssvlan, m_imscvlan, m_imsUV,
                                          m_isContainIPTV,m_iptvsvlan,m_iptvcvlan,m_iptvUV);
                }
                else if (m_type == TaskType.NewIMS)
                {
                    rollbackTask = new Task(m_taskID, TaskType.DelIMS,m_omc.City, m_omc.Manufacturer, m_omc.OmcName, m_oltID,
                           null, m_ponID, m_onuID, null, m_svlan, m_cvlan, m_phone, DateTime.Now, m_muvlan,
                           m_uvlan, m_mvlan, m_feNumber, m_potsNumber, m_isContainIMS, m_imssvlan, m_imscvlan, m_imsUV,
                           m_isContainIPTV, m_iptvsvlan, m_iptvcvlan, m_iptvUV);

                }
                else if (m_type == TaskType.NewIPTV)
                {
                    rollbackTask = new Task(m_taskID, TaskType.DelIPTV,m_omc.City, m_omc.Manufacturer, m_omc.OmcName, m_oltID,
                            null, m_ponID, m_onuID, null, m_svlan, m_cvlan, m_phone, DateTime.Now, m_muvlan,
                            m_uvlan, m_mvlan, m_feNumber, m_potsNumber, m_isContainIMS, m_imssvlan, m_imscvlan, m_imsUV,
                            m_isContainIPTV, m_iptvsvlan, m_iptvcvlan, m_iptvUV);
                }
                else
                {
                    rollbackTask = new Task(m_taskID, TaskType.DelONU, m_omc.City, m_omc.Manufacturer, m_omc.OmcName, m_oltID,
                                    null, m_ponID, m_onuID, null, m_svlan, m_cvlan, m_phone, DateTime.Now, m_muvlan,
                                   m_uvlan, m_mvlan, m_feNumber, m_potsNumber, m_isContainIMS, m_imssvlan, m_imscvlan, m_imsUV,
                                   m_isContainIPTV, m_iptvsvlan, m_iptvcvlan, m_iptvUV);
                }
                rollbackTask.m_oltName = m_oltName;
            }
            rollbackTask.m_isRollbackTask = true;
            return rollbackTask;
        }
        #endregion
    }
}
