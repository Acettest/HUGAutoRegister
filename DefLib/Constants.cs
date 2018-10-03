using System;

namespace TK_AlarmManagement
{
    /// <summary>
    /// Constants 的摘要说明。
    /// </summary>
    /// 
    public class Constants
    {
        public enum TK_CommandType
        {
            LOGIN = 1,
            LOGOUT = 2,
            RESPONSE = 3,
            ALARM_REPORT = 4,
            ALARM_ACK = 5,
            ALARM_REACK = 6,
            PROJECT_ADD = 7,
            PROJECT_REMOVE = 8,
            PROJECT_MODIFY = 9,
            REGISTERCLIENT = 10,					//向AlarmManager注册一个客户端
            UNREGISTERCLIENT = 11,				//向AlarmManager移除一个客户端
            KEEPALIVE = 12,						//客户端和服务器端保活包
            ALARM_ACK_CHANGE = 13,
            ALARM_ORDER_CHANGE = 14,
            ALARM_PROJECT_CHANGE = 15,
            SENDORDER = 16,						//客户端故障派单通知

            ALARM_HANG_UP = 100,              //告警挂起
            ALARM_HANG_DOWN = 101,
            ALARM_HANG_CHANGE = 102,

            //北向接口命令字段
            NI_ALARM_SYNC = 150, 
            NI_LOG_SYNC = 151,  
            NI_ALARM_SYNC_REPORTER = 152,
            NI_LOG_SYNC_REPORTER = 153,


            ALLOCATE_TKSN = 100001, // 分布式服务器体系通讯命令
            ADAPTER_LOGIN,
            ADAPTER_LOGOUT,
            ADAPTER_START,
            ADAPTER_STOP,
            ADAPTER_STATE_REPORT,
            ADAPTER_ALARM_REPORT,
            REGISTERADAPTER,
            UNREGISTERADAPTER,
            ADAPTER_GETOMCLIST,
            ADAPTER_GETRUNTIMEINFO,
            ADAPTER_GETCURLOG,
            ADAPTER_GETLOGFILES,
            ADAPTER_SHUTDOWN,
            SERVER_GETRUNTIMEINFO,
            SERVER_GETCURLOG,
            SERVER_GETLOGFILES,

            SMCDAEMON_GETCURLOG,
            SMCDAEMON_GETLOGFILES,
            SMCDAEMON_STAT,
            SMCDAEMON_SHUTDOWN,
            SMCDAEMON_GETRUNTIMEINFO,

            SMC_STAT,
            SMC_STOP,
            SMC_SHUTDOWN,
            SMC_GETRUNTIMEINFO,
            SMC_GETLIST,
            SMC_GETCURLOG,
            SMC_GETLOGFILES,

            //SPAM_FILTER_ENQUEUEGARBAGE = 200001,
            //SPAM_FILTER_ENQUEUEHTGARBAGE,
            //SPAM_FILTER_ENQUEUEOVERSENT,
            //SPAM_FILTER_ENQUEUEHTOVERSENT,
            DSPAM_REGISTER_FILTER = 200001,
            DSPAM_UNREGISTER_FILTER = 200002,
            DSPAM_REGISTER_OA = 200003,
            DSPAM_UNREGISTER_OA = 200004,
            DSPAM_GET_FILTERSTATUS = 200005,
            DSPAM_GET_OASTATUS = 200006,
            DSPAM_REQUEST_FILTER = 200007,
            DSPAM_ENQUEUE_OVERSENT = 200008,
            DSPAM_ENQUEUE_OA_GARBAGE = 200009,
            DSPAM_ENQUEUE_ADAPTER_GARBAGE = 200010,
            DSPAM_ENQUEUE_HTOVERSENT = 200011,
            DSPAM_EVALUATE_CECAPABILITY = 200012,
            DSPAM_GET_MASTERPERFORMANCE = 200013,

            MON_GETTERMINALSINFO = 300001, // 客户端控制WLAN测试机状态
            MON_VERIFYTERMINALSSTATUS = 300002,

            MON_GETWLAN_AUTHSUMMARY = 300011,
            MON_GETWLAN_FTPSUMMARY = 300012,
            MON_GETWLAN_RSSISUMMARY = 300013,
            MON_GETWLAN_WEBSUMMARY = 300014,

            MON_GET_TABLUARINFO = 300015,
            MON_COMMIT_TABLUARINFO = 300016,
            MON_GET_TABLUARINFO_RAWQUERY = 300017,
            MON_EXECUTE_NONQUERY = 300018,

            MON_SURVEY_APLIST = 300100,

            MON_SETTESTMODE_AUTO = 300101,  //AP扫描:自动模式
            MON_SETTESTMODE_INSPECT = 300102,  //AP扫描:巡检模式
            MON_SETTESTMODE_LOCK = 300103,//AP扫描:锁定模式
            MON_SETTESTMODE_MANUAL = 300104,//AP扫描:手工模式

            //MON_GETWLAN_APINFO = 300015,
            //MON_COMMITWLAN_APINFO = 300016,
            //MON_GETWLAN_TERMINALINFO = 300017,
            //MON_COMMITWLAN_TERMINALINFO = 300018,
            //MON_GETWLAN_LINEINFO = 300019,
            //MON_COMMITWLAN_LINEINFO = 300020,
            //MON_GETWLAN_ALARMSEVERITY = 300021,
            //MON_COMMITWLAN_ALARMSEVERITY = 300023,
            //MON_GETWLAN_SCHEDULE = 300024,
            //MON_COMMITWLAN_SCHEDULE = 300025,
            //MON_GETWLAN_FTPTARGET = 300026,
            //MON_COMMITWLAN_FTPTARGET = 300027,
            //MON_GETWLAN_WEBTARGET = 300028,
            //MON_COMMITWLAN_WEBTARGET = 300029,
            //MON_GETWLAN_THRESHOLD = 300030,
            //MON_COMMITWLAN_THRESHOLD = 300031,


        };

        public const string MSG_START_ID = "<++++>\r\n";
        public const string MSG_DOUBLE_START_ID = "<++++>\r\n<++++>\r\n";
        public const string MSG_END_ID = "<---->\r\n";
        public const string MSG_DOUBLE_END_ID = "<---->\r\n<---->\r\n";
        public const string MSG_LINE_TERMINATOR = "\r\n";
        public const string MSG_DOUBLE_LINE_TERMINATOR = "\r\n\r\n";
        public const string MSG_NV_TERMINATOR = "=";
        public const string MSG_PARANAME_RESPONSE_TO = "RESPONSE_TO";
        public const string MSG_PARANAME_IMMEDIATE_ID = "IMMEDIATE_ID";
        public const string MSG_PARANAME_REASON = "REASON";

        public const string MSG_PARANAME_COMMAND = "COMMAND";
        public const string MSG_PARANAME_SEQUENCE_ID = "SEQUENCE_ID";
        public const string MSG_PARANAME_ADAPTER_ID = "ADAPTER_ID";
        public const string MSG_PARANAME_ADAPTER_NAME = "NAME";
        public const string MSG_PARANAME_ADAPTER_ADDRESS = "ADAPTER_ADDRESS";
        public const string MSG_PARANAME_ADAPTER_EXTRAINFO = "ADAPTER_EXTRAINFO";
        public const string MSG_PARANAME_ADAPTER_CONTROLLER_PORT = "ADAPTER_CONTROLLER_PORT";
        public const string MSG_PARANAME_TERMINALS_INFO = "TERMINALS_INFO";
        public const string MSG_PARANAME_REPEATER_ID = "REPEATER_ID";
        public const string MSG_PARANAME_RESULT = "RESULT";
        public const string MSG_PARANAME_ADAPTER_STATE = "ADAPTER_STATE";
        public const string MSG_PARANAME_TKSN_NUM = "TKSN_NUM";
        public const string MSG_PARANAME_TKSN_START = "TKSN_START";
        public const string MSG_PARANAME_TKSN_END = "TKSN_END";
        public const string MSG_PARANAME_AUTHORIZED = "AUTHORIZED";
        public const string MSG_PARANAME_USER = "用户名";
        public const string MSG_PARANAME_PWD = "密码";
        public const string MSG_PARANAME_CITY = "CITY";
        public const string MSG_PARANAME_RIGHT = "RIGHT";
        public const string MSG_PARANAME_ORDERID = "派单号";
        public const string MSG_PARANAME_ORDEROPERATOR = "派单人";
        public const string MSG_PARANAME_ORDERTIME = "派单时间";
        public const string MSG_PARANAME_ORDERLEVER = "派单人级别";
        public const string MSG_PARANAME_ALARMID = "告警ID";
        public const string MSG_PARANAME_ALARMSTATUS = "告警状态";

        public const string MSG_PARANAME_CHARTRESULT = "TABLE_RESULT";
        public const string MSG_PARANAME_GAUGERESULT = "GAUGE_RESULT";
        public const string MSG_PARANAME_TABLENAME = "TABLE_NAME";
        public const string MSG_PARANAME_TABLEFILTER = "TABLE_FILTER";
        public const string MSG_PARANAME_TABLEPKCOL = "TABLE_PKCOL";
        public const string MSG_PARANAME_TABLECATALOG = "TABLE_CATALOG";
        public const string MSG_PARANAME_RAWSQL = "RAWSQL";

        public const string MSG_PARANAME_LAST_SEQ_ID = "LAST_SEQ_ID";

        public const string MSG_PARANAME_SMSLIST = "SMSLIST";

        public const string MSG_PARANAME_APLINEINFO = "AP_LINEINFO";
        public const string MSG_PARANAME_APSELECTINFO = "AP_SELECTINFO";

        public const string MSG_PARANAME_COMPUTINGEND_STATUS = "CE_STATUS";
        public const string MSG_PARANAME_COMPUTINGEND_TASKREQUEST = "CE_TASKREQUEST";
        public const string MSG_PARANAME_COMPUTINGEND_DISTRIBUTION = "CE_DISTRIBUTION";
        public const string MSG_PARANAME_COMPUTINGEND_PERFORMANCE = "CE_PERFORMANCE";

        public const string MSG_TYPE_RESPONSE = "RESPONSE";
        public const string MSG_TYPE_ALARM_REPORT = "ALARM_REPORT";
        public const string MSG_TYPE_LOGIN = "LOGIN";
        public const string MSG_TYPE_LOGOUT = "LOGOUT";
        public const string MSG_TYPE_ALARM_ACK = "ALARM_ACK";
        public const string MSG_TYPE_ALARM_REACK = "ALARM_REACK";
        public const string MSG_TYPE_PROJECT_ADD = "PROJECT_ADD";
        public const string MSG_TYPE_PROJECT_REMOVE = "PROJECT_REMOVE";
        public const string MSG_TYPE_PROJECT_MODIFY = "PROJECT_MODIFY";
        public const string MSG_TYPE_KEEPALIVE = "KEEPALIVE";
        public const string MSG_TYPE_ALARM_ACK_CHANGE = "ALARM_ACK_CHANGE";
        public const string MSG_TYPE_ALARM_ORDER_CHANGE = "ALARM_ORDER_CHANGE";
        public const string MSG_TYPE_SENDORDER = "SENDORDER";
        public const string MSG_TYPE_ALLOCATE_TKSN = "ALLOCATE_TKSN";
        public const string MSG_TYPE_ADAPTER_LOGIN = "ADAPTER_LOGIN";
        public const string MSG_TYPE_ADAPTER_LOGOUT = "ADAPTER_LOGOUT";
        public const string MSG_TYPE_ADAPTER_ALARM_REPORT = "ADAPTER_ALARM_REPORT";
        public const string MSG_TYPE_ADAPTER_START = "ADAPTER_START";
        public const string MSG_TYPE_ADAPTER_STOP = "ADAPTER_STOP";
        public const string MSG_TYPE_ADAPTER_STATE_REPORT = "ADAPTER_STATE_REPORT";
        public const string MSG_TYPE_ADAPTER_GETOMCLIST = "ADAPTER_GETOMCLIST";
        public const string MSG_TYPE_ADAPTER_GETCURLOG = "ADAPTER_GETCURLOG";
        public const string MSG_TYPE_ADAPTER_GETLOGFILES = "ADAPTER_GETLOGFILES";
        public const string MSG_TYPE_ADAPTER_GETRUNTIMEINFO = "ADAPTER_GETRUNTIMEINFO";
        public const string MSG_TYPE_ADAPTER_SHUTDOWN = "ADAPTER_SHUTDOWN";
        public const string MSG_TYPE_SERVER_GETCURLOG = "SERVER_GETCURLOG";
        public const string MSG_TYPE_SERVER_GETRUNTIMEINFO = "SERVER_GETRUNTIMEINFO";
        public const string MSG_TYPE_SERVER_GETLOGFILES = "SERVER_GETLOGFILES";
        public const string MSG_TYPE_ALARM_PROJECT_CHANGE = "ALARM_PROJECT_CHANGE";  //工程超时告警取消工程标识

        public const string MSG_TYPE_ALARM_HANG_UP = "ALARM_HANG_UP";                //告警挂起
        public const string MSG_TYPE_ALARM_HANG_DOWN = "ALARM_HANG_DOWN";
        public const string MSG_TYPE_ALARM_HANG_CHANGE = "ALARM_HANG_CHANGE";

        public const string MSG_TYPE_SMCDAEMON_GETCURLOG = "SMCDAEMON_GETCURLOG";
        public const string MSG_TYPE_SMCDAEMON_GETLOGFILES = "SMCDAEMON_GETLOGFILES";
        public const string MSG_TYPE_SMCDAEMON_STAT = "SMCDAEMON_STAT";
        public const string MSG_TYPE_SMCDAEMON_SHUTDOWN = "SMCDAEMON_SHUTDOWN";
        public const string MSG_TYPE_SMCDAEMON_GETRUNTIMEINFO = "SMCDAEMON_GETRUNTIMEINFO";

        public const string MSG_TYPE_SMC_STAT = "SMC_STAT";
        public const string MSG_TYPE_SMC_STOP = "SMC_STOP";
        public const string MSG_TYPE_SMC_SHUTDOWN = "SMC_SHUTDOWN";
        public const string MSG_TYPE_SMC_GETRUNTIMEINFO = "SMC_GETRUNTIMEINFO";
        public const string MSG_TYPE_SMC_GETLIST = "SMC_GETLIST";
        public const string MSG_TYPE_SMC_GETCURLOG = "SMC_GETCURLOG";
        public const string MSG_TYPE_SMC_GETLOGFILES = "SMC_GETLOGFILES";

        public const string MSG_RESULT_OK = "OK";
        public const string MSG_RESULT_NOK = "NOK";

        public const long BOARDCAST_CLIENT_ID = -1;

        public const string MSG_PARANAME_CLIENTINFO = "CLIENTINFO";
        public const string NI_NORTHSERVER_IP = "NORTHSERVER_IP";
        public const string NI_NORTHSERVER_NAME = "NORTHSERVER_NAME";
        public const string NI_MAX_EVENT_ID = "MAX_EVENT_ID";
        public const string NI_LOG_INFO = "LOG_INFO";
        public const string NI_ALARM_STATE = "ALARM_STATE"; //1:恢复告警 0:活动告警 2:保留


        public const int ACTIVEALARM_MAXLENGTH = 100000;

        public const string GARBAGE_CLEARTIME = "2099-12-31 23:59:59";

        public const string ALARM_SERVERNAME = "告警通讯服务器";
        public const string COMPRERSSED_ALARM_SERVERNAME = "告警通讯服务器(压缩)";
        public const string ADAPTER_SERVERNAME = "采集通讯服务器";
        public const string MONITOR_SERVERNAME = "监控通讯服务器";
        public const string NORTH_SERVERNAME = "北向接口服务器";
        public const string SMC_SERVERNAME = "服务管理中心";
        public const string SMCDAEMON_SERVERNAME = "SMC守护中心";

        public const string MO_SOURCEOMC = "采集源";
        public const string MO_SYSDB = "系统数据库";
        public const string MO_PROCESS = "采集进程";

        public const string TKALM_OMCALM = "采集源通讯异常";
        public const string TKALM_SYSDBALM = "系统数据库操作异常";
        public const string TKALM_PROCESSALM = "进程处理异常";

        /// <summary>
        /// for client
        /// </summary>
        /// 
        public static string CONF_FILENAME = "conf.xml";

        public const string PARANAME_SERVER_IP = "Server IP";
        public const string PARANAME_SERVER_PORT = "Server Port";
        public const string PARANAME_COM_LOGDIR = "Com LogDir";
        public const string PARANAME_ALARM_CAPACITY = "Alarm Capacity";
        public const string PARANAME_COM_TIMEOUT = "Comm Timeout";
        public const string PARANAME_TRANSIENT_THRESHOLD = "Transient Threshold"; // 秒为单位
        public const string PARANAME_LOG_DAYS = "Log Keep Days";
        public const string PARANAME_COMPRESS_DATA = "Compress Data";


        public static string COMPANY_LV1 = "安徽省公司";
        public enum UserOperation
        {
            AckAlarm,
            Order,
            ViewLog,
            UserManage,
            Project,
            AlarmCase
        };

        public enum AlarmSeverity
        {
            Critical = 1,
            Major = 2,
            Minor = 3,
            Warning = 4,
            None = 5
        };

        public const string SEVERITY_CRITICAL = "紧急告警";
        public const string SEVERITY_MAJOR = "重要告警";
        public const string SEVERITY_MINOR = "一般告警";
        public const string SEVERITY_WARNING = "提示告警";
        public const string SEVERITY_NONE = "无告警";

        public enum AlarmState
        {
            UnClear_UnAcked = 1,
            UnClear_Acked = 2,
            Clear_UnAcked = 3,
            Change_OrderStatus = 4,
            Change_Project = 5
        };

        /// <summary>
        /// 告警DataTable列名
        /// </summary>
        /// 
        public static string COLNAME_ALARM_TKSN = "集中告警流水号";
        public static string COLNAME_ALARM_OCCURTIME = "告警发生时间";
        public static string COLNAME_ALARM_CLEARTIME = "告警恢复时间";
        public static string COLNAME_ALARM_ACKTIME = "告警确认时间";
        public static string COLNAME_ALARM_OPERATOR = "操作员信息";
        public static string COLNAME_ALARM_RECEIVETIME = "接收时间";

        public static string CUSTOMFILTER_NAME = "自定义";
        public static string SMS_FILTERGROUP_NAME = "短信提示过滤器";
        public static string VOICE_FILTERGROUP_NAME = "声音提示过滤器";

        public static string MOMENT_FILTERGROUP_NAME = "瞬断告警过滤器";
        public static string OVERTIME_FILTERGROUP_NAME = "超时告警过滤器";
        public static string OVERSOUL_FILTERGROUP_NAME = "超量告警过滤器";
        public static string DELAY_FILTERGROUP_NAME = "延迟告警过滤器";

        public Constants()
        {
        }
    }
}
