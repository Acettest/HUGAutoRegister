USE [master]
GO
/****** Object:  Database [YWT]    Script Date: 07/23/2014 17:00:07 ******/
CREATE DATABASE [YWT] ON  PRIMARY 
( NAME = N'YWT', FILENAME = N'D:\一网通自动激活\Database\YWT.mdf' , SIZE = 76160KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
 LOG ON 
( NAME = N'YWT_log', FILENAME = N'D:\一网通自动激活\Database\YWT_log.ldf' , SIZE = 2048KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)
GO
ALTER DATABASE [YWT] SET COMPATIBILITY_LEVEL = 90
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [YWT].[dbo].[sp_fulltext_database] @action = 'disable'
end
GO
ALTER DATABASE [YWT] SET ANSI_NULL_DEFAULT OFF
GO
ALTER DATABASE [YWT] SET ANSI_NULLS OFF
GO
ALTER DATABASE [YWT] SET ANSI_PADDING OFF
GO
ALTER DATABASE [YWT] SET ANSI_WARNINGS OFF
GO
ALTER DATABASE [YWT] SET ARITHABORT OFF
GO
ALTER DATABASE [YWT] SET AUTO_CLOSE OFF
GO
ALTER DATABASE [YWT] SET AUTO_CREATE_STATISTICS ON
GO
ALTER DATABASE [YWT] SET AUTO_SHRINK OFF
GO
ALTER DATABASE [YWT] SET AUTO_UPDATE_STATISTICS ON
GO
ALTER DATABASE [YWT] SET CURSOR_CLOSE_ON_COMMIT OFF
GO
ALTER DATABASE [YWT] SET CURSOR_DEFAULT  GLOBAL
GO
ALTER DATABASE [YWT] SET CONCAT_NULL_YIELDS_NULL OFF
GO
ALTER DATABASE [YWT] SET NUMERIC_ROUNDABORT OFF
GO
ALTER DATABASE [YWT] SET QUOTED_IDENTIFIER OFF
GO
ALTER DATABASE [YWT] SET RECURSIVE_TRIGGERS OFF
GO
ALTER DATABASE [YWT] SET  DISABLE_BROKER
GO
ALTER DATABASE [YWT] SET AUTO_UPDATE_STATISTICS_ASYNC OFF
GO
ALTER DATABASE [YWT] SET DATE_CORRELATION_OPTIMIZATION OFF
GO
ALTER DATABASE [YWT] SET TRUSTWORTHY OFF
GO
ALTER DATABASE [YWT] SET ALLOW_SNAPSHOT_ISOLATION OFF
GO
ALTER DATABASE [YWT] SET PARAMETERIZATION SIMPLE
GO
ALTER DATABASE [YWT] SET READ_COMMITTED_SNAPSHOT OFF
GO
ALTER DATABASE [YWT] SET  READ_WRITE
GO
ALTER DATABASE [YWT] SET RECOVERY FULL
GO
ALTER DATABASE [YWT] SET  MULTI_USER
GO
ALTER DATABASE [YWT] SET PAGE_VERIFY CHECKSUM
GO
ALTER DATABASE [YWT] SET DB_CHAINING OFF
GO
USE [YWT]
GO
/****** Object:  Table [dbo].[RollbackHisTask]    Script Date: 07/23/2014 17:00:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[RollbackHisTask](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[taskID] [varchar](32) NOT NULL,
	[taskType] [varchar](16) NULL,
	[rollbackTaskType] [varchar](16) NULL,
	[taskStatus] [varchar](8) NULL,
	[city] [varchar](16) NULL,
	[manufacturer] [varchar](16) NULL,
	[omcName] [varchar](32) NULL,
	[oltID] [varchar](32) NULL,
	[ponID] [varchar](32) NULL,
	[onuID] [varchar](32) NULL,
	[onuType] [varchar](32) NULL,
	[onuPort] [varchar](32) NULL,
	[svlan] [int] NULL,
	[cvlan] [int] NULL,
	[phone] [varchar](32) NULL,
	[sipRegDM] [varchar](32) NULL,
	[sipUserName] [varchar](64) NULL,
	[sipUserPWD] [varchar](16) NULL,
	[responseBoss] [bit] NULL,
	[responseMsg] [varchar](512) NULL,
	[responseXML] [varchar](1024) NULL,
	[bossReply] [varchar](512) NULL,
	[bossReplyStatus] [bit] NULL,
	[receiveTime] [datetime] NULL,
	[executeTime] [datetime] NULL,
	[completeTime] [datetime] NULL,
	[responseTime] [datetime] NULL,
	[isRollback] [bit] NULL,
	[netInterrupt] [bit] NULL,
	[errorDesc] [varchar](32) NULL,
 CONSTRAINT [PK_ROLLBACKHISTASK] PRIMARY KEY CLUSTERED 
(
	[taskID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'标识列' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTask', @level2type=N'COLUMN',@level2name=N'id'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'工单号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTask', @level2type=N'COLUMN',@level2name=N'taskID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'工单类型' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTask', @level2type=N'COLUMN',@level2name=N'taskType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'回滚工单类型' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTask', @level2type=N'COLUMN',@level2name=N'rollbackTaskType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'工单状态' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTask', @level2type=N'COLUMN',@level2name=N'taskStatus'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'城市' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTask', @level2type=N'COLUMN',@level2name=N'city'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'厂商' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTask', @level2type=N'COLUMN',@level2name=N'manufacturer'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'omc' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTask', @level2type=N'COLUMN',@level2name=N'omcName'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'OLT IP' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTask', @level2type=N'COLUMN',@level2name=N'oltID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'PON口' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTask', @level2type=N'COLUMN',@level2name=N'ponID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'LOID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTask', @level2type=N'COLUMN',@level2name=N'onuID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ONU类型' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTask', @level2type=N'COLUMN',@level2name=N'onuType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ONU端口' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTask', @level2type=N'COLUMN',@level2name=N'onuPort'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'SVLAN' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTask', @level2type=N'COLUMN',@level2name=N'svlan'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'CVLAN' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTask', @level2type=N'COLUMN',@level2name=N'cvlan'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'电话号码' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTask', @level2type=N'COLUMN',@level2name=N'phone'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'SIP注册服务器' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTask', @level2type=N'COLUMN',@level2name=N'sipRegDM'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'SIP用户端口对应的用户' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTask', @level2type=N'COLUMN',@level2name=N'sipUserName'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'SIP用户端口对应的用户密码' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTask', @level2type=N'COLUMN',@level2name=N'sipUserPWD'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'是否已回复boss' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTask', @level2type=N'COLUMN',@level2name=N'responseBoss'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'回复boss内容' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTask', @level2type=N'COLUMN',@level2name=N'responseMsg'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'回复boss报文内容' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTask', @level2type=N'COLUMN',@level2name=N'responseXML'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'回复boss后返回的信息' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTask', @level2type=N'COLUMN',@level2name=N'bossReply'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'boss是否正确接收' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTask', @level2type=N'COLUMN',@level2name=N'bossReplyStatus'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'接收时间' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTask', @level2type=N'COLUMN',@level2name=N'receiveTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'执行时间' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTask', @level2type=N'COLUMN',@level2name=N'executeTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'完成时间' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTask', @level2type=N'COLUMN',@level2name=N'completeTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'回复时间' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTask', @level2type=N'COLUMN',@level2name=N'responseTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'是否已回滚' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTask', @level2type=N'COLUMN',@level2name=N'isRollback'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'是否网络中断' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTask', @level2type=N'COLUMN',@level2name=N'netInterrupt'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'错误描述' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTask', @level2type=N'COLUMN',@level2name=N'errorDesc'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'PBOSS系统下发的回滚工单信息表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTask'
GO
/****** Object:  Table [dbo].[ResumeAlarm]    Script Date: 07/23/2014 17:00:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ResumeAlarm](
	[TKsn] [bigint] NOT NULL,
	[ManuSn] [bigint] NULL,
	[City] [varchar](20) NULL,
	[Manufacturer] [varchar](16) NULL,
	[BusinessType] [varchar](16) NULL,
	[NeName] [varchar](100) NULL,
	[NeType] [varchar](20) NULL,
	[ObjName] [varchar](255) NULL,
	[ObjType] [varchar](50) NULL,
	[AlarmName] [varchar](1024) NULL,
	[Redefinition] [varchar](1024) NULL,
	[Severity] [varchar](8) NULL,
	[OccurTime] [datetime] NULL,
	[AckTimeLV1] [datetime] NULL,
	[AckAgainTimeLV1] [datetime] NULL,
	[AckTimeLV2] [datetime] NULL,
	[AckAgainTimeLV2] [datetime] NULL,
	[ClearTime] [datetime] NULL,
	[Location] [varchar](2048) NULL,
	[OperatorLV11] [varchar](16) NULL,
	[OperatorLV12] [varchar](16) NULL,
	[OperatorLV21] [varchar](16) NULL,
	[OperatorLV22] [varchar](16) NULL,
	[ProjectInfo] [varchar](64) NULL,
	[OrderOperatorLV1] [varchar](16) NULL,
	[OrderIDLV1] [varchar](64) NULL,
	[OrderTimeLV1] [datetime] NULL,
	[OrderOperatorLV2] [varchar](16) NULL,
	[OrderIDLV2] [varchar](64) NULL,
	[OrderTimeLV2] [datetime] NULL,
	[OMCName] [varchar](32) NULL,
	[Reserved2] [varchar](255) NULL,
	[Reserved3] [varchar](255) NULL,
	[ReceiveTime] [datetime] NULL,
 CONSTRAINT [PK_ResumeAlarm] PRIMARY KEY CLUSTERED 
(
	[TKsn] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[OperLog]    Script Date: 07/23/2014 17:00:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[OperLog](
	[rec_no] [int] IDENTITY(1,1) NOT NULL,
	[create_date] [datetime] NULL,
	[login_name] [varchar](16) NULL,
	[operation] [varchar](32) NULL,
	[detail] [varchar](1024) NULL,
	[result] [varchar](256) NULL,
 CONSTRAINT [PK_OperLog] PRIMARY KEY CLUSTERED 
(
	[rec_no] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[OltInfo]    Script Date: 07/23/2014 17:00:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[OltInfo](
	[CompanyNo] [varchar](20) NULL,
	[CITYNAME] [varchar](50) NOT NULL,
	[SVRNAME] [varchar](50) NOT NULL,
	[SvrID] [int] NULL,
	[County] [varchar](20) NULL,
	[DevName] [varchar](255) NULL,
	[DevIp] [varchar](30) NULL,
	[DT] [varchar](50) NULL,
	[DeVER] [varchar](100) NULL,
	[receiveTime] [datetime] NULL
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[MaxTKSN]    Script Date: 07/23/2014 17:00:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MaxTKSN](
	[AllocatedTKSN] [bigint] NULL,
	[AllocateStep] [int] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Maintenance]    Script Date: 07/23/2014 17:00:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Maintenance](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[type] [varchar](32) NULL,
	[start_time] [datetime] NULL,
	[end_time] [datetime] NULL,
	[ne_name] [varchar](50) NULL,
	[create_date] [datetime] NULL,
	[operator] [varchar](16) NULL,
	[phone_no] [varchar](16) NULL,
	[department] [varchar](64) NULL,
	[report_msg] [varchar](256) NULL,
	[Redefinition] [varchar](256) NULL,
 CONSTRAINT [PK_Maintenance] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[GPONServer]    Script Date: 07/23/2014 17:00:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[GPONServer](
	[SvrID] [int] NULL,
	[city] [varchar](16) NULL,
	[manufacturer] [varchar](16) NULL,
	[omcName] [varchar](32) NULL,
	[omcIP] [varchar](16) NULL,
	[omcPort] [int] NULL,
	[userName] [varchar](16) NULL,
	[pwd] [varchar](16) NULL,
	[connectStatus] [bit] NULL,
	[statusDesc] [varchar](32) NULL,
	[connectTime] [datetime] NULL,
	[lastOffTime] [datetime] NULL,
	[gponIP] [varchar](16) NULL
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'SvrID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'GPONServer', @level2type=N'COLUMN',@level2name=N'SvrID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'城市' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'GPONServer', @level2type=N'COLUMN',@level2name=N'city'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'厂商' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'GPONServer', @level2type=N'COLUMN',@level2name=N'manufacturer'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'网管名称' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'GPONServer', @level2type=N'COLUMN',@level2name=N'omcName'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'网管IP' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'GPONServer', @level2type=N'COLUMN',@level2name=N'omcIP'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'端口' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'GPONServer', @level2type=N'COLUMN',@level2name=N'omcPort'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'用户名' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'GPONServer', @level2type=N'COLUMN',@level2name=N'userName'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'密码' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'GPONServer', @level2type=N'COLUMN',@level2name=N'pwd'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'连接状态' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'GPONServer', @level2type=N'COLUMN',@level2name=N'connectStatus'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'状态描述' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'GPONServer', @level2type=N'COLUMN',@level2name=N'statusDesc'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'连接时间' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'GPONServer', @level2type=N'COLUMN',@level2name=N'connectTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最后一次断开时间' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'GPONServer', @level2type=N'COLUMN',@level2name=N'lastOffTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'GPON IP' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'GPONServer', @level2type=N'COLUMN',@level2name=N'gponIP'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'GPON服务器表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'GPONServer'
GO
/****** Object:  Table [dbo].[GarbageAlarm]    Script Date: 07/23/2014 17:00:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[GarbageAlarm](
	[TKsn] [bigint] NOT NULL,
	[ManuSn] [bigint] NULL,
	[City] [varchar](20) NULL,
	[Manufacturer] [varchar](16) NULL,
	[BusinessType] [varchar](16) NULL,
	[NeName] [varchar](40) NULL,
	[NeType] [varchar](20) NULL,
	[ObjName] [varchar](256) NULL,
	[ObjType] [varchar](50) NULL,
	[AlarmName] [varchar](1024) NULL,
	[Redefinition] [varchar](1024) NULL,
	[Severity] [varchar](8) NULL,
	[OccurTime] [datetime] NULL,
	[AckTimeLV1] [datetime] NULL,
	[AckAgainTimeLV1] [datetime] NULL,
	[AckTimeLV2] [datetime] NULL,
	[AckAgainTimeLV2] [datetime] NULL,
	[ClearTime] [datetime] NULL,
	[Location] [varchar](2048) NULL,
	[OperatorLV11] [varchar](16) NULL,
	[OperatorLV12] [varchar](16) NULL,
	[OperatorLV21] [varchar](16) NULL,
	[OperatorLV22] [varchar](16) NULL,
	[ProjectInfo] [varchar](64) NULL,
	[OrderOperatorLV1] [varchar](16) NULL,
	[OrderIDLV1] [varchar](64) NULL,
	[OrderTimeLV1] [datetime] NULL,
	[OrderOperatorLV2] [varchar](16) NULL,
	[OrderIDLV2] [varchar](64) NULL,
	[OrderTimeLV2] [datetime] NULL,
	[OMCName] [varchar](32) NULL,
	[Reserved2] [varchar](255) NULL,
	[Reserved3] [varchar](255) NULL,
	[ReceiveTime] [datetime] NULL,
 CONSTRAINT [PK_GarbageAlarm] PRIMARY KEY CLUSTERED 
(
	[TKsn] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[ftth_resumealarm]    Script Date: 07/23/2014 17:00:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ftth_resumealarm](
	[id] [bigint] NOT NULL,
	[CompanyNo] [varchar](20) NULL,
	[CITYNAME] [varchar](50) NULL,
	[SVRNAME] [varchar](50) NULL,
	[description] [varchar](512) NULL,
	[OMCName] [nvarchar](50) NULL,
	[raisetime] [datetime] NULL,
	[cleartime] [datetime] NULL,
	[Severity] [int] NULL,
	[ftthTaskID] [varchar](32) NULL,
	[taskid] [varchar](50) NULL,
	[type] [varchar](8) NULL,
	[tasker] [varchar](50) NULL,
	[TIMELONG] [varchar](50) NULL,
	[receivetime] [datetime] NULL,
	[neid] [varchar](32) NULL,
 CONSTRAINT [PK_ftth_resumealarm] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
CREATE NONCLUSTERED INDEX [IX_ftth_resumealarm_raisetime] ON [dbo].[ftth_resumealarm] 
(
	[raisetime] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_ftth_resumealarm_taskid] ON [dbo].[ftth_resumealarm] 
(
	[taskid] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ftth_activealarm]    Script Date: 07/23/2014 17:00:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ftth_activealarm](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[CompanyNo] [varchar](20) NULL,
	[CITYNAME] [varchar](50) NULL,
	[SVRNAME] [varchar](50) NULL,
	[description] [varchar](512) NULL,
	[OMCName] [nvarchar](50) NULL,
	[raisetime] [datetime] NULL,
	[cleartime] [datetime] NULL,
	[Severity] [int] NULL,
	[ftthTaskID] [varchar](32) NULL,
	[taskid] [varchar](50) NULL,
	[type] [varchar](8) NULL,
	[tasker] [varchar](50) NULL,
	[neid] [varchar](32) NULL,
 CONSTRAINT [PK_ftth_activealarm] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  UserDefinedFunction [dbo].[F_DIFF_TWODATE]    Script Date: 07/23/2014 17:01:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[F_DIFF_TWODATE]
 (@DTBEGINDATE DATETIME , @DTENDDATE DATETIME )
RETURNS VARCHAR(100)
AS
--获取指定两时间内的持续时长，格式：**天**小时**分**秒
BEGIN
	DECLARE @IDAY INT,@IMINUTE INT,@IHOUR INT,@ISECOND INT,@VCHDIFF VARCHAR(50)
	--SET @DTBEGINDATE='2007-09-09 18:21:11'
	--SET @DTENDDATE = '2007-09-19 18:21:10'
	SET @IDAY = DATEDIFF(SECOND,@DTBEGINDATE,@DTENDDATE) / ( 24*60*60 )
	SET @IHOUR = DATEDIFF(SECOND,@DTBEGINDATE,@DTENDDATE) / ( 60*60 ) - @IDAY*24
	SET @IMINUTE = DATEDIFF(SECOND,@DTBEGINDATE,@DTENDDATE) / 60 - @IDAY*24*60 - @IHOUR*60
	SET @ISECOND = DATEDIFF(SECOND,@DTBEGINDATE,@DTENDDATE) - @IDAY*24*60*60 - @IHOUR*60*60 - @IMINUTE*60
	SET @VCHDIFF = CONVERT(VARCHAR,@IDAY)+'天'+CONVERT(VARCHAR,@IHOUR)+'小时'+CONVERT(VARCHAR,@IMINUTE)+'分'+CONVERT(VARCHAR,@ISECOND)+'秒'
	RETURN @VCHDIFF
END
GO
/****** Object:  Table [dbo].[Errlog]    Script Date: 07/23/2014 17:01:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Errlog](
	[rec_no] [int] IDENTITY(1,1) NOT NULL,
	[create_date] [datetime] NULL,
	[detail] [varchar](500) NULL,
 CONSTRAINT [PK_Errlog] PRIMARY KEY CLUSTERED 
(
	[rec_no] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[DetailConf]    Script Date: 07/23/2014 17:01:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[DetailConf](
	[OMCName] [varchar](32) NOT NULL,
	[ConnString] [varchar](255) NULL,
	[ActiveTable] [varchar](64) NULL,
	[ResumeTable] [varchar](64) NULL,
 CONSTRAINT [PK_DetailConf] PRIMARY KEY CLUSTERED 
(
	[OMCName] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[BusinessType]    Script Date: 07/23/2014 17:01:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[BusinessType](
	[rec_no] [int] IDENTITY(1,1) NOT NULL,
	[BusinessType] [varchar](32) NOT NULL,
 CONSTRAINT [PK_BusinessType] PRIMARY KEY CLUSTERED 
(
	[rec_no] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[ActiveAlarm]    Script Date: 07/23/2014 17:01:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ActiveAlarm](
	[TKsn] [bigint] NOT NULL,
	[ManuSn] [bigint] NULL,
	[City] [varchar](20) NULL,
	[Manufacturer] [varchar](16) NULL,
	[BusinessType] [varchar](16) NULL,
	[NeName] [varchar](100) NULL,
	[NeType] [varchar](20) NULL,
	[ObjName] [varchar](256) NULL,
	[ObjType] [varchar](50) NULL,
	[AlarmName] [varchar](1024) NULL,
	[Redefinition] [varchar](1024) NULL,
	[Severity] [varchar](8) NULL,
	[OccurTime] [datetime] NULL,
	[AckTimeLV1] [datetime] NULL,
	[AckAgainTimeLV1] [datetime] NULL,
	[AckTimeLV2] [datetime] NULL,
	[AckAgainTimeLV2] [datetime] NULL,
	[ClearTime] [datetime] NULL,
	[Location] [varchar](2048) NULL,
	[OperatorLV11] [varchar](16) NULL,
	[OperatorLV12] [varchar](16) NULL,
	[OperatorLV21] [varchar](16) NULL,
	[OperatorLV22] [varchar](16) NULL,
	[ProjectInfo] [varchar](64) NULL,
	[OrderOperatorLV1] [varchar](16) NULL,
	[OrderIDLV1] [varchar](64) NULL,
	[OrderTimeLV1] [datetime] NULL,
	[OrderOperatorLV2] [varchar](16) NULL,
	[OrderIDLV2] [varchar](64) NULL,
	[OrderTimeLV2] [datetime] NULL,
	[OMCName] [varchar](32) NULL,
	[Reserved2] [varchar](255) NULL,
	[Reserved3] [varchar](255) NULL,
	[ReceiveTime] [datetime] NULL,
 CONSTRAINT [PK_ActiveAlarm] PRIMARY KEY CLUSTERED 
(
	[TKsn] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  StoredProcedure [dbo].[uspClearLog]    Script Date: 07/23/2014 17:01:38 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-----------------------------------------------------
--	目的：截断收缩日志，由作业调用
--
--	维护日志
--
--	维护人		维护时间		描叙
-- ------		--------		----------------
--	汪传龙		2014-06-27		创建
------------------------------------------------------
CREATE proc [dbo].[uspClearLog]
as
backup log YWT with no_log
dbcc shrinkfile(YWT_log)
GO
/****** Object:  Table [dbo].[TK_ResumeAlarm]    Script Date: 07/23/2014 17:01:38 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[TK_ResumeAlarm](
	[TKSn] [bigint] NOT NULL,
	[SourceOMC] [varchar](32) NULL,
	[Object] [varchar](32) NULL,
	[AlarmName] [varchar](255) NULL,
	[OccurTime] [datetime] NULL,
	[LastOccurTime] [datetime] NULL,
	[ClearTime] [datetime] NULL,
	[Severity] [varchar](16) NULL,
	[Detail] [varchar](1024) NULL,
	[Location] [varchar](255) NULL,
	[OMCName] [varchar](32) NULL,
	[BusinessType] [varchar](16) NULL,
	[Manufacturer] [varchar](16) NULL,
 CONSTRAINT [PK_TK_ResumeAlarm] PRIMARY KEY CLUSTERED 
(
	[TKSn] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[TK_ActiveAlarm]    Script Date: 07/23/2014 17:01:38 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[TK_ActiveAlarm](
	[TKSn] [bigint] NOT NULL,
	[SourceOMC] [varchar](32) NULL,
	[Object] [varchar](32) NULL,
	[AlarmName] [varchar](255) NULL,
	[OccurTime] [datetime] NULL,
	[LastOccurTime] [datetime] NULL,
	[ClearTime] [datetime] NULL,
	[Severity] [varchar](16) NULL,
	[Detail] [varchar](1024) NULL,
	[Location] [varchar](255) NULL,
	[OMCName] [varchar](32) NULL,
	[BusinessType] [varchar](16) NULL,
	[Manufacturer] [varchar](16) NULL,
 CONSTRAINT [PK_TK_ActiveAlarm] PRIMARY KEY CLUSTERED 
(
	[TKSn] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY],
 CONSTRAINT [IX_TK_ActiveAlarm] UNIQUE NONCLUSTERED 
(
	[SourceOMC] ASC,
	[Object] ASC,
	[AlarmName] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[TaskHistory_bak]    Script Date: 07/23/2014 17:01:38 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[TaskHistory_bak](
	[id] [bigint] NOT NULL,
	[taskID] [varchar](32) NOT NULL,
	[taskType] [varchar](16) NULL,
	[taskStatus] [varchar](8) NULL,
	[city] [varchar](16) NULL,
	[manufacturer] [varchar](16) NULL,
	[omcName] [varchar](32) NULL,
	[oltID] [varchar](32) NULL,
	[ponID] [varchar](32) NULL,
	[onuID] [varchar](32) NULL,
	[onuType] [varchar](32) NULL,
	[onuPort] [varchar](32) NULL,
	[svlan] [int] NULL,
	[cvlan] [int] NULL,
	[phone] [varchar](32) NULL,
	[sipRegDM] [varchar](32) NULL,
	[sipUserName] [varchar](64) NULL,
	[sipUserPWD] [varchar](16) NULL,
	[responseBoss] [bit] NULL,
	[responseMsg] [varchar](512) NULL,
	[responseXML] [varchar](1024) NULL,
	[bossReply] [varchar](512) NULL,
	[bossReplyStatus] [bit] NULL,
	[receiveTime] [datetime] NULL,
	[executeTime] [datetime] NULL,
	[completeTime] [datetime] NULL,
	[responseTime] [datetime] NULL,
	[isRollback] [bit] NULL,
	[netInterrupt] [bit] NULL,
	[netDelay] [bit] NULL,
	[errorDesc] [varchar](32) NULL
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'标识列' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory_bak', @level2type=N'COLUMN',@level2name=N'id'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'工单号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory_bak', @level2type=N'COLUMN',@level2name=N'taskID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'工单类型' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory_bak', @level2type=N'COLUMN',@level2name=N'taskType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'工单状态' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory_bak', @level2type=N'COLUMN',@level2name=N'taskStatus'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'城市' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory_bak', @level2type=N'COLUMN',@level2name=N'city'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'厂商' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory_bak', @level2type=N'COLUMN',@level2name=N'manufacturer'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'omc' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory_bak', @level2type=N'COLUMN',@level2name=N'omcName'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'OLT IP' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory_bak', @level2type=N'COLUMN',@level2name=N'oltID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'PON口' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory_bak', @level2type=N'COLUMN',@level2name=N'ponID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'LOID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory_bak', @level2type=N'COLUMN',@level2name=N'onuID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ONU类型' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory_bak', @level2type=N'COLUMN',@level2name=N'onuType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ONU端口' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory_bak', @level2type=N'COLUMN',@level2name=N'onuPort'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'SVLAN' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory_bak', @level2type=N'COLUMN',@level2name=N'svlan'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'CVLAN' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory_bak', @level2type=N'COLUMN',@level2name=N'cvlan'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'电话号码' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory_bak', @level2type=N'COLUMN',@level2name=N'phone'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'SIP注册服务器' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory_bak', @level2type=N'COLUMN',@level2name=N'sipRegDM'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'SIP用户端口对应的用户' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory_bak', @level2type=N'COLUMN',@level2name=N'sipUserName'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'SIP用户端口对应的用户密码' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory_bak', @level2type=N'COLUMN',@level2name=N'sipUserPWD'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'是否已回复boss' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory_bak', @level2type=N'COLUMN',@level2name=N'responseBoss'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'回复boss内容' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory_bak', @level2type=N'COLUMN',@level2name=N'responseMsg'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'回复boss报文内容' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory_bak', @level2type=N'COLUMN',@level2name=N'responseXML'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'回复boss后返回的信息' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory_bak', @level2type=N'COLUMN',@level2name=N'bossReply'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'boss是否正确接收' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory_bak', @level2type=N'COLUMN',@level2name=N'bossReplyStatus'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'接收时间' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory_bak', @level2type=N'COLUMN',@level2name=N'receiveTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'执行时间' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory_bak', @level2type=N'COLUMN',@level2name=N'executeTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'完成时间' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory_bak', @level2type=N'COLUMN',@level2name=N'completeTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'回复时间' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory_bak', @level2type=N'COLUMN',@level2name=N'responseTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'是否已回滚' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory_bak', @level2type=N'COLUMN',@level2name=N'isRollback'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'是否网络中断' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory_bak', @level2type=N'COLUMN',@level2name=N'netInterrupt'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'是否网络延迟回复' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory_bak', @level2type=N'COLUMN',@level2name=N'netDelay'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'错误描述' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory_bak', @level2type=N'COLUMN',@level2name=N'errorDesc'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'PBOSS系统下发的工单信息表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory_bak'
GO
/****** Object:  Table [dbo].[TaskHistory]    Script Date: 07/23/2014 17:01:38 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[TaskHistory](
	[id] [bigint] NOT NULL,
	[taskID] [varchar](32) NOT NULL,
	[taskType] [varchar](16) NULL,
	[taskStatus] [varchar](8) NULL,
	[city] [varchar](16) NULL,
	[manufacturer] [varchar](16) NULL,
	[omcName] [varchar](32) NULL,
	[oltID] [varchar](32) NULL,
	[ponID] [varchar](32) NULL,
	[onuID] [varchar](32) NULL,
	[onuType] [varchar](32) NULL,
	[onuPort] [varchar](32) NULL,
	[svlan] [int] NULL,
	[cvlan] [int] NULL,
	[phone] [varchar](32) NULL,
	[sipRegDM] [varchar](32) NULL,
	[sipUserName] [varchar](64) NULL,
	[sipUserPWD] [varchar](16) NULL,
	[responseBoss] [bit] NULL,
	[responseMsg] [varchar](512) NULL,
	[responseXML] [varchar](1024) NULL,
	[bossReply] [varchar](512) NULL,
	[bossReplyStatus] [bit] NULL,
	[receiveTime] [datetime] NULL,
	[executeTime] [datetime] NULL,
	[completeTime] [datetime] NULL,
	[responseTime] [datetime] NULL,
	[isRollback] [bit] NULL,
	[netInterrupt] [bit] NULL,
	[netDelay] [bit] NULL,
	[errorDesc] [varchar](32) NULL,
 CONSTRAINT [PK_TaskHistory] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'标识列' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory', @level2type=N'COLUMN',@level2name=N'id'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'工单号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory', @level2type=N'COLUMN',@level2name=N'taskID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'工单类型' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory', @level2type=N'COLUMN',@level2name=N'taskType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'工单状态' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory', @level2type=N'COLUMN',@level2name=N'taskStatus'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'城市' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory', @level2type=N'COLUMN',@level2name=N'city'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'厂商' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory', @level2type=N'COLUMN',@level2name=N'manufacturer'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'omc' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory', @level2type=N'COLUMN',@level2name=N'omcName'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'OLT IP' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory', @level2type=N'COLUMN',@level2name=N'oltID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'PON口' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory', @level2type=N'COLUMN',@level2name=N'ponID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'LOID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory', @level2type=N'COLUMN',@level2name=N'onuID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ONU类型' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory', @level2type=N'COLUMN',@level2name=N'onuType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ONU端口' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory', @level2type=N'COLUMN',@level2name=N'onuPort'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'SVLAN' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory', @level2type=N'COLUMN',@level2name=N'svlan'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'CVLAN' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory', @level2type=N'COLUMN',@level2name=N'cvlan'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'电话号码' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory', @level2type=N'COLUMN',@level2name=N'phone'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'SIP注册服务器' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory', @level2type=N'COLUMN',@level2name=N'sipRegDM'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'SIP用户端口对应的用户' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory', @level2type=N'COLUMN',@level2name=N'sipUserName'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'SIP用户端口对应的用户密码' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory', @level2type=N'COLUMN',@level2name=N'sipUserPWD'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'是否已回复boss' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory', @level2type=N'COLUMN',@level2name=N'responseBoss'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'回复boss内容' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory', @level2type=N'COLUMN',@level2name=N'responseMsg'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'回复boss报文内容' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory', @level2type=N'COLUMN',@level2name=N'responseXML'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'回复boss后返回的信息' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory', @level2type=N'COLUMN',@level2name=N'bossReply'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'boss是否正确接收' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory', @level2type=N'COLUMN',@level2name=N'bossReplyStatus'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'接收时间' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory', @level2type=N'COLUMN',@level2name=N'receiveTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'执行时间' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory', @level2type=N'COLUMN',@level2name=N'executeTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'完成时间' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory', @level2type=N'COLUMN',@level2name=N'completeTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'回复时间' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory', @level2type=N'COLUMN',@level2name=N'responseTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'是否已回滚' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory', @level2type=N'COLUMN',@level2name=N'isRollback'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'是否网络中断' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory', @level2type=N'COLUMN',@level2name=N'netInterrupt'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'是否网络延迟回复' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory', @level2type=N'COLUMN',@level2name=N'netDelay'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'错误描述' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory', @level2type=N'COLUMN',@level2name=N'errorDesc'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'PBOSS系统下发的工单信息表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskHistory'
GO
/****** Object:  Table [dbo].[Task]    Script Date: 07/23/2014 17:01:38 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Task](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[taskID] [varchar](32) NOT NULL,
	[taskType] [varchar](16) NULL,
	[taskStatus] [varchar](8) NULL,
	[city] [varchar](16) NULL,
	[manufacturer] [varchar](16) NULL,
	[omcName] [varchar](32) NULL,
	[oltID] [varchar](32) NULL,
	[ponID] [varchar](32) NULL,
	[onuID] [varchar](32) NULL,
	[onuType] [varchar](32) NULL,
	[onuPort] [varchar](32) NULL,
	[svlan] [int] NULL,
	[cvlan] [int] NULL,
	[phone] [varchar](32) NULL,
	[sipRegDM] [varchar](32) NULL,
	[sipUserName] [varchar](64) NULL,
	[sipUserPWD] [varchar](16) NULL,
	[responseBoss] [bit] NULL,
	[responseMsg] [varchar](512) NULL,
	[responseXML] [varchar](1024) NULL,
	[bossReply] [varchar](512) NULL,
	[bossReplyStatus] [bit] NULL,
	[receiveTime] [datetime] NULL,
	[executeTime] [datetime] NULL,
	[completeTime] [datetime] NULL,
	[responseTime] [datetime] NULL,
	[isRollback] [bit] NULL,
	[netInterrupt] [bit] NULL,
	[netDelay] [bit] NULL,
	[errorDesc] [varchar](32) NULL,
 CONSTRAINT [PK_TASK] PRIMARY KEY CLUSTERED 
(
	[taskID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'标识列' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Task', @level2type=N'COLUMN',@level2name=N'id'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'工单号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Task', @level2type=N'COLUMN',@level2name=N'taskID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'工单类型' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Task', @level2type=N'COLUMN',@level2name=N'taskType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'工单状态' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Task', @level2type=N'COLUMN',@level2name=N'taskStatus'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'城市' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Task', @level2type=N'COLUMN',@level2name=N'city'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'厂商' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Task', @level2type=N'COLUMN',@level2name=N'manufacturer'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'omc' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Task', @level2type=N'COLUMN',@level2name=N'omcName'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'OLT IP' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Task', @level2type=N'COLUMN',@level2name=N'oltID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'PON口' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Task', @level2type=N'COLUMN',@level2name=N'ponID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'LOID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Task', @level2type=N'COLUMN',@level2name=N'onuID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ONU类型' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Task', @level2type=N'COLUMN',@level2name=N'onuType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ONU端口' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Task', @level2type=N'COLUMN',@level2name=N'onuPort'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'SVLAN' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Task', @level2type=N'COLUMN',@level2name=N'svlan'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'CVLAN' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Task', @level2type=N'COLUMN',@level2name=N'cvlan'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'电话号码' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Task', @level2type=N'COLUMN',@level2name=N'phone'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'SIP注册服务器' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Task', @level2type=N'COLUMN',@level2name=N'sipRegDM'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'SIP用户端口对应的用户' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Task', @level2type=N'COLUMN',@level2name=N'sipUserName'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'SIP用户端口对应的用户密码' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Task', @level2type=N'COLUMN',@level2name=N'sipUserPWD'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'是否已回复boss' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Task', @level2type=N'COLUMN',@level2name=N'responseBoss'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'回复boss内容' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Task', @level2type=N'COLUMN',@level2name=N'responseMsg'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'回复boss报文内容' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Task', @level2type=N'COLUMN',@level2name=N'responseXML'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'回复boss后返回的信息' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Task', @level2type=N'COLUMN',@level2name=N'bossReply'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'boss是否正确接收' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Task', @level2type=N'COLUMN',@level2name=N'bossReplyStatus'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'接收时间' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Task', @level2type=N'COLUMN',@level2name=N'receiveTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'执行时间' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Task', @level2type=N'COLUMN',@level2name=N'executeTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'完成时间' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Task', @level2type=N'COLUMN',@level2name=N'completeTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'回复时间' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Task', @level2type=N'COLUMN',@level2name=N'responseTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'是否已回滚' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Task', @level2type=N'COLUMN',@level2name=N'isRollback'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'是否网络中断' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Task', @level2type=N'COLUMN',@level2name=N'netInterrupt'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'是否网络延迟回复' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Task', @level2type=N'COLUMN',@level2name=N'netDelay'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'错误描述' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Task', @level2type=N'COLUMN',@level2name=N'errorDesc'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'PBOSS系统下发的工单信息表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Task'
GO
/****** Object:  Table [dbo].[SysLog]    Script Date: 07/23/2014 17:01:38 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[SysLog](
	[rec_no] [int] IDENTITY(1,1) NOT NULL,
	[create_date] [datetime] NULL,
	[detail] [varchar](256) NULL,
 CONSTRAINT [PK_SysLog] PRIMARY KEY CLUSTERED 
(
	[rec_no] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[WatchDog]    Script Date: 07/23/2014 17:01:38 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[WatchDog](
	[host] [varchar](64) NOT NULL,
	[component] [varchar](64) NOT NULL,
	[refresh_interval] [int] NULL,
	[update_time] [datetime] NULL,
	[check_time] [datetime] NULL,
	[pid] [int] NULL,
 CONSTRAINT [PK_WatchDog] PRIMARY KEY CLUSTERED 
(
	[host] ASC,
	[component] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Command]    Script Date: 07/23/2014 17:01:38 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Command](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[taskID] [varchar](32) NULL,
	[commandStep] [int] NULL,
	[commandText] [varchar](512) NULL,
	[completionCode] [varchar](16) NULL,
	[additionalInfo] [varchar](1024) NULL,
	[generateTime] [datetime] NULL,
	[executeTime] [datetime] NULL,
	[omcTime] [datetime] NULL,
	[localTime] [datetime] NULL,
	[replyContent] [varchar](2048) NULL,
	[isRollback] [bit] NULL,
 CONSTRAINT [PK_COMMAND] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'标识列' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Command', @level2type=N'COLUMN',@level2name=N'id'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'工单号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Command', @level2type=N'COLUMN',@level2name=N'taskID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'命令步骤' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Command', @level2type=N'COLUMN',@level2name=N'commandStep'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'命令正文' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Command', @level2type=N'COLUMN',@level2name=N'commandText'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'omc完成标识' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Command', @level2type=N'COLUMN',@level2name=N'completionCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'omc附加信息' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Command', @level2type=N'COLUMN',@level2name=N'additionalInfo'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'命令生成时间' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Command', @level2type=N'COLUMN',@level2name=N'generateTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'命令执行时间' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Command', @level2type=N'COLUMN',@level2name=N'executeTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'omc完成时间' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Command', @level2type=N'COLUMN',@level2name=N'omcTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'omc回复接收时间' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Command', @level2type=N'COLUMN',@level2name=N'localTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'omc回复原文' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Command', @level2type=N'COLUMN',@level2name=N'replyContent'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'是否是回滚命令' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Command', @level2type=N'COLUMN',@level2name=N'isRollback'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'向omc下发的命令表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Command'
GO
/****** Object:  Table [dbo].[CommandHistory]    Script Date: 07/23/2014 17:01:38 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[CommandHistory](
	[id] [bigint] NOT NULL,
	[taskID] [varchar](32) NULL,
	[commandStep] [int] NULL,
	[commandText] [varchar](512) NULL,
	[completionCode] [varchar](16) NULL,
	[additionalInfo] [varchar](1024) NULL,
	[generateTime] [datetime] NULL,
	[executeTime] [datetime] NULL,
	[omcTime] [datetime] NULL,
	[localTime] [datetime] NULL,
	[replyContent] [varchar](2048) NULL,
	[isRollback] [bit] NULL,
 CONSTRAINT [PK_CommandHistory] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'标识列' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CommandHistory', @level2type=N'COLUMN',@level2name=N'id'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'工单号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CommandHistory', @level2type=N'COLUMN',@level2name=N'taskID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'命令步骤' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CommandHistory', @level2type=N'COLUMN',@level2name=N'commandStep'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'命令正文' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CommandHistory', @level2type=N'COLUMN',@level2name=N'commandText'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'omc完成标识' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CommandHistory', @level2type=N'COLUMN',@level2name=N'completionCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'omc附加信息' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CommandHistory', @level2type=N'COLUMN',@level2name=N'additionalInfo'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'命令生成时间' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CommandHistory', @level2type=N'COLUMN',@level2name=N'generateTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'命令执行时间' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CommandHistory', @level2type=N'COLUMN',@level2name=N'executeTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'omc完成时间' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CommandHistory', @level2type=N'COLUMN',@level2name=N'omcTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'omc回复接收时间' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CommandHistory', @level2type=N'COLUMN',@level2name=N'localTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'omc回复原文' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CommandHistory', @level2type=N'COLUMN',@level2name=N'replyContent'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'是否是回滚命令' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CommandHistory', @level2type=N'COLUMN',@level2name=N'isRollback'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'向omc下发的命令表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CommandHistory'
GO
/****** Object:  Table [dbo].[SeverityConfig]    Script Date: 07/23/2014 17:01:38 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[SeverityConfig](
	[AlarmName] [varchar](512) NOT NULL,
	[Redefinition] [varchar](1024) NULL,
	[Severity] [varchar](8) NULL,
	[DefaultSeverity] [varchar](8) NULL,
 CONSTRAINT [PK_SeverityConfig_1] PRIMARY KEY CLUSTERED 
(
	[AlarmName] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[RollbackHisTaskHistory_bak]    Script Date: 07/23/2014 17:01:38 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[RollbackHisTaskHistory_bak](
	[id] [bigint] NOT NULL,
	[taskID] [varchar](32) NOT NULL,
	[taskType] [varchar](16) NULL,
	[rollbackTaskType] [varchar](16) NULL,
	[taskStatus] [varchar](8) NULL,
	[city] [varchar](16) NULL,
	[manufacturer] [varchar](16) NULL,
	[omcName] [varchar](32) NULL,
	[oltID] [varchar](32) NULL,
	[ponID] [varchar](32) NULL,
	[onuID] [varchar](32) NULL,
	[onuType] [varchar](32) NULL,
	[onuPort] [varchar](32) NULL,
	[svlan] [int] NULL,
	[cvlan] [int] NULL,
	[phone] [varchar](32) NULL,
	[sipRegDM] [varchar](32) NULL,
	[sipUserName] [varchar](64) NULL,
	[sipUserPWD] [varchar](16) NULL,
	[responseBoss] [bit] NULL,
	[responseMsg] [varchar](512) NULL,
	[responseXML] [varchar](1024) NULL,
	[bossReply] [varchar](512) NULL,
	[bossReplyStatus] [bit] NULL,
	[receiveTime] [datetime] NULL,
	[executeTime] [datetime] NULL,
	[completeTime] [datetime] NULL,
	[responseTime] [datetime] NULL,
	[isRollback] [bit] NULL,
	[netInterrupt] [bit] NULL,
	[errorDesc] [varchar](32) NULL
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'标识列' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory_bak', @level2type=N'COLUMN',@level2name=N'id'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'工单号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory_bak', @level2type=N'COLUMN',@level2name=N'taskID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'工单类型' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory_bak', @level2type=N'COLUMN',@level2name=N'taskType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'回滚工单类型' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory_bak', @level2type=N'COLUMN',@level2name=N'rollbackTaskType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'工单状态' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory_bak', @level2type=N'COLUMN',@level2name=N'taskStatus'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'城市' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory_bak', @level2type=N'COLUMN',@level2name=N'city'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'厂商' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory_bak', @level2type=N'COLUMN',@level2name=N'manufacturer'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'omc' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory_bak', @level2type=N'COLUMN',@level2name=N'omcName'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'OLT IP' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory_bak', @level2type=N'COLUMN',@level2name=N'oltID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'PON口' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory_bak', @level2type=N'COLUMN',@level2name=N'ponID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'LOID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory_bak', @level2type=N'COLUMN',@level2name=N'onuID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ONU类型' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory_bak', @level2type=N'COLUMN',@level2name=N'onuType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ONU端口' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory_bak', @level2type=N'COLUMN',@level2name=N'onuPort'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'SVLAN' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory_bak', @level2type=N'COLUMN',@level2name=N'svlan'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'CVLAN' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory_bak', @level2type=N'COLUMN',@level2name=N'cvlan'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'电话号码' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory_bak', @level2type=N'COLUMN',@level2name=N'phone'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'SIP注册服务器' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory_bak', @level2type=N'COLUMN',@level2name=N'sipRegDM'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'SIP用户端口对应的用户' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory_bak', @level2type=N'COLUMN',@level2name=N'sipUserName'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'SIP用户端口对应的用户密码' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory_bak', @level2type=N'COLUMN',@level2name=N'sipUserPWD'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'是否已回复boss' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory_bak', @level2type=N'COLUMN',@level2name=N'responseBoss'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'回复boss内容' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory_bak', @level2type=N'COLUMN',@level2name=N'responseMsg'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'回复boss报文内容' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory_bak', @level2type=N'COLUMN',@level2name=N'responseXML'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'回复boss后返回的信息' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory_bak', @level2type=N'COLUMN',@level2name=N'bossReply'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'boss是否正确接收' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory_bak', @level2type=N'COLUMN',@level2name=N'bossReplyStatus'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'接收时间' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory_bak', @level2type=N'COLUMN',@level2name=N'receiveTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'执行时间' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory_bak', @level2type=N'COLUMN',@level2name=N'executeTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'完成时间' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory_bak', @level2type=N'COLUMN',@level2name=N'completeTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'回复时间' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory_bak', @level2type=N'COLUMN',@level2name=N'responseTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'是否已回滚' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory_bak', @level2type=N'COLUMN',@level2name=N'isRollback'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'是否网络中断' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory_bak', @level2type=N'COLUMN',@level2name=N'netInterrupt'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'错误描述' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory_bak', @level2type=N'COLUMN',@level2name=N'errorDesc'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'PBOSS系统下发的回滚工单信息表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory_bak'
GO
/****** Object:  Table [dbo].[RollbackHisTaskHistory]    Script Date: 07/23/2014 17:01:38 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[RollbackHisTaskHistory](
	[id] [bigint] NOT NULL,
	[taskID] [varchar](32) NOT NULL,
	[taskType] [varchar](16) NULL,
	[rollbackTaskType] [varchar](16) NULL,
	[taskStatus] [varchar](8) NULL,
	[city] [varchar](16) NULL,
	[manufacturer] [varchar](16) NULL,
	[omcName] [varchar](32) NULL,
	[oltID] [varchar](32) NULL,
	[ponID] [varchar](32) NULL,
	[onuID] [varchar](32) NULL,
	[onuType] [varchar](32) NULL,
	[onuPort] [varchar](32) NULL,
	[svlan] [int] NULL,
	[cvlan] [int] NULL,
	[phone] [varchar](32) NULL,
	[sipRegDM] [varchar](32) NULL,
	[sipUserName] [varchar](64) NULL,
	[sipUserPWD] [varchar](16) NULL,
	[responseBoss] [bit] NULL,
	[responseMsg] [varchar](512) NULL,
	[responseXML] [varchar](1024) NULL,
	[bossReply] [varchar](512) NULL,
	[bossReplyStatus] [bit] NULL,
	[receiveTime] [datetime] NULL,
	[executeTime] [datetime] NULL,
	[completeTime] [datetime] NULL,
	[responseTime] [datetime] NULL,
	[isRollback] [bit] NULL,
	[netInterrupt] [bit] NULL,
	[errorDesc] [varchar](32) NULL,
 CONSTRAINT [PK_RollbackHisTaskHistory] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'标识列' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory', @level2type=N'COLUMN',@level2name=N'id'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'工单号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory', @level2type=N'COLUMN',@level2name=N'taskID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'工单类型' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory', @level2type=N'COLUMN',@level2name=N'taskType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'回滚工单类型' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory', @level2type=N'COLUMN',@level2name=N'rollbackTaskType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'工单状态' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory', @level2type=N'COLUMN',@level2name=N'taskStatus'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'城市' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory', @level2type=N'COLUMN',@level2name=N'city'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'厂商' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory', @level2type=N'COLUMN',@level2name=N'manufacturer'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'omc' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory', @level2type=N'COLUMN',@level2name=N'omcName'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'OLT IP' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory', @level2type=N'COLUMN',@level2name=N'oltID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'PON口' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory', @level2type=N'COLUMN',@level2name=N'ponID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'LOID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory', @level2type=N'COLUMN',@level2name=N'onuID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ONU类型' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory', @level2type=N'COLUMN',@level2name=N'onuType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ONU端口' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory', @level2type=N'COLUMN',@level2name=N'onuPort'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'SVLAN' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory', @level2type=N'COLUMN',@level2name=N'svlan'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'CVLAN' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory', @level2type=N'COLUMN',@level2name=N'cvlan'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'电话号码' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory', @level2type=N'COLUMN',@level2name=N'phone'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'SIP注册服务器' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory', @level2type=N'COLUMN',@level2name=N'sipRegDM'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'SIP用户端口对应的用户' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory', @level2type=N'COLUMN',@level2name=N'sipUserName'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'SIP用户端口对应的用户密码' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory', @level2type=N'COLUMN',@level2name=N'sipUserPWD'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'是否已回复boss' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory', @level2type=N'COLUMN',@level2name=N'responseBoss'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'回复boss内容' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory', @level2type=N'COLUMN',@level2name=N'responseMsg'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'回复boss报文内容' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory', @level2type=N'COLUMN',@level2name=N'responseXML'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'回复boss后返回的信息' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory', @level2type=N'COLUMN',@level2name=N'bossReply'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'boss是否正确接收' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory', @level2type=N'COLUMN',@level2name=N'bossReplyStatus'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'接收时间' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory', @level2type=N'COLUMN',@level2name=N'receiveTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'执行时间' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory', @level2type=N'COLUMN',@level2name=N'executeTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'完成时间' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory', @level2type=N'COLUMN',@level2name=N'completeTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'回复时间' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory', @level2type=N'COLUMN',@level2name=N'responseTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'是否已回滚' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory', @level2type=N'COLUMN',@level2name=N'isRollback'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'是否网络中断' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory', @level2type=N'COLUMN',@level2name=N'netInterrupt'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'错误描述' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory', @level2type=N'COLUMN',@level2name=N'errorDesc'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'PBOSS系统下发的回滚工单信息表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RollbackHisTaskHistory'
GO
/****** Object:  View [dbo].[vRollbackTaskReplyBossMsg]    Script Date: 07/23/2014 17:01:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE view [dbo].[vRollbackTaskReplyBossMsg]
as
select 
	[taskID],
	taskStatus,
	responseMsg,
	1 as isRollback,  
	receiveTime,
	NULL AS netDelay
	from RollbackHisTask
	where (responseBoss is null or responseBoss=0) and (taskStatus='成功' or taskStatus='失败') AND taskid NOT LIKE 'Test%'
GO
/****** Object:  StoredProcedure [dbo].[spRollbackHistoryTask]    Script Date: 07/23/2014 17:01:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-----------------------------------------------------
-- 
--
-- 维护日志
--
-- 维护人		维护时间		描叙
-- ------		--------		----------------
-- 汪传龙		2014-07-03		创建
------------------------------------------------------
CREATE proc [dbo].[spRollbackHistoryTask]
@taskID varchar(32),
@flag int output,
@msg varchar(64) output
as
declare @taskType varchar(8)
declare @city varchar(16)
declare @manufacturer varchar(16)
declare @omcName varchar(50)
declare @oltID varchar(32)
declare @ponID varchar(32)
declare @onuID varchar(32)
declare @onuPort varchar(32)
declare @onuType varchar(32)
declare @svlan int
declare @cvlan int
declare @phone varchar(20)
if not exists(select * from TaskHistory where taskID=@taskID and taskStatus='成功')  
begin
	if exists(select * from TaskHistory where taskID=@taskID and taskStatus='失败' AND (taskType ='宽带新装' OR taskType='IMS新装' OR taskType='宽带加装' OR taskType='IMS加装'))
	begin
		select @taskType=[taskType] from dbo.TaskHistory where taskID=@taskID
		if(@taskType='宽带新装' OR @taskType='IMS新装')
		BEGIN
		set @msg='原工单执行失败，已回滚'
		END
		ELSE
		BEGIN
		SET @msg='原工单执行失败，无需回滚'
		END
		set @flag=1; 
		select @phone=phone from dbo.TaskHistory where taskID=@taskID and taskStatus='失败'
		insert into RollbackHisTask([taskID],[responseMsg],[taskStatus],[phone])
			values(@taskID,@msg,'成功',@phone)
	END
	ELSE if exists(select * from TaskHistory where taskID=@taskID and taskStatus='失败')
	begin
		set @msg='此工单不具备回滚条件'
		set @flag=0; 
	end
	else
	begin 
		set @msg='找不到此单号工单'
		set @flag=0; 
	end
end
else 
begin
	select @taskType=[taskType],@city=[city],@manufacturer=[manufacturer],@omcName=[omcName],
		@oltID=[oltID],@ponID=[ponID],@onuID=[onuID],@onuPort=[onuPort],
		@onuType=[onuType],@svlan=[svlan],@cvlan=[cvlan],@phone=[phone]
		from dbo.TaskHistory where taskID=@taskID
--宽带
	if(@taskType='宽带新装' OR @taskType='宽带加装')
	begin
		insert into RollbackHisTask([taskID],[taskType],[rollbackTaskType],
			[city],[manufacturer],[omcName],[oltID],[ponID],[onuID],[onuPort],
			[onuType],[svlan],[cvlan],[phone])
			values(@taskID,@taskType,'宽带拆机',
			@city,@manufacturer,@omcName,@oltID,@ponID,@onuID,@onuPort,
			@onuType,@svlan,@cvlan,@phone)
		set @msg='成功';
		set @flag=1;
	end
--IMS
	else if(@taskType='IMS新装' OR @taskType='IMS加装')
	begin
		insert into RollbackHisTask([taskID],[taskType],[rollbackTaskType],
			[city],[manufacturer],[omcName],[oltID],[ponID],[onuID],[onuPort],
			[onuType],[svlan],[cvlan],[phone])
			values(@taskID,@taskType,'IMS拆机',
			@city,@manufacturer,@omcName,@oltID,@ponID,@onuID,@onuPort,
			@onuType,@svlan,@cvlan,@phone)
		set @msg='成功';
		set @flag=1;		
	end
	else
	begin
		set @msg='此工单不具备回滚条件'
		set @flag=0; 
	end
end
GO
/****** Object:  StoredProcedure [dbo].[spClearRollbackTask]    Script Date: 07/23/2014 17:01:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-----------------------------------------------------
-- 
--
-- 维护日志
--
-- 维护人		维护时间		描叙
-- ------		--------		----------------
-- 汪传龙		2014-06-17		创建
------------------------------------------------------
CREATE proc [dbo].[spClearRollbackTask]
@taskID varchar(32) 
as
insert into RollbackHisTaskHistory
	(id,taskID,taskType,rollbackTaskType,taskStatus,city,manufacturer,omcName,oltID,ponID,onuID,onuType,onuPort,svlan,cvlan
	,phone,sipRegDM,sipUserName,sipUserPWD,responseBoss,responseMsg,responseXML,bossReply,bossReplyStatus,receiveTime
	,executeTime,completeTime,responseTime,isRollback,netInterrupt)
	select id,taskID,taskType,rollbackTaskType,taskStatus,city,manufacturer,omcName,oltID,ponID,onuID,onuType,onuPort,svlan,cvlan
	,phone,sipRegDM,sipUserName,sipUserPWD,responseBoss,responseMsg,responseXML,bossReply,bossReplyStatus,receiveTime
	,executeTime,completeTime,responseTime,isRollback,netInterrupt
		from RollbackHisTask
		where taskID=@taskID;

insert into CommandHistory
	(id,taskID,commandStep,commandText,completionCode,additionalInfo,generateTime,executeTime,omcTime,localTime,replyContent,isRollback)
	select id,taskID,commandStep,commandText,completionCode,additionalInfo,generateTime,executeTime,omcTime,localTime,replyContent,1
		from Command
		where taskID=@taskID;
		
delete from Command where taskID=@taskID;

delete from RollbackHisTask where taskID=@taskID;

update TaskHistory set isRollback=1 where taskID=@taskID;
GO
/****** Object:  View [dbo].[vRollbackHisTask]    Script Date: 07/23/2014 17:01:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-----------------------------------------------------
--
--
-- 维护日志
--
-- 维护人		维护时间		描叙
-- ------		--------		----------------
-- 汪传龙		2014-06-19		创建
------------------------------------------------------
CREATE view [dbo].[vRollbackHisTask]
as
select 
	id,
	[taskID],
	[rollbackTaskType] as taskType,
	city,
	manufacturer,
	omcName,
	oltID,
	ponID,
	onuID,
	onuPort,
	onuType,
	svlan,
	cvlan,	
	phone ,
	sipRegDM ,
	sipUserName ,
	sipUserPWD ,
	'false' as isRelocateTask,
	'true' as isRollbackHisTask,
	[receiveTime],
	netInterrupt,
	taskType as oldTaskType
	from RollbackHisTask
	where taskStatus is null
GO
/****** Object:  StoredProcedure [dbo].[uspRefreshOlt]    Script Date: 07/23/2014 17:01:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[uspRefreshOlt]
as
declare @Today datetime
set @Today=convert(datetime,convert(varchar(10),getdate(),120))

insert into oltinfo(CompanyNo,cityname,svrname,svrid,county,devname,devip,dt,dever)
	select CompanyNo,cityname,svrname,svrid,county,devname,devip,dt,dever from [188].easyams.dbo.V_OLT_Info_YWT
	where CompanyNo!='Eric';

delete from oltinfo
where (receivetime<@Today) --or (receivetime is null)
GO
/****** Object:  View [dbo].[V_AlarmHistory_Query]    Script Date: 07/23/2014 17:01:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[V_AlarmHistory_Query]
AS
SELECT     CompanyNo,cityname,svrname as servername,null as sn, id AS AlarmID,null as serverid, neid AS oldNeID, null AS NeID, null AS NeExtID, 
                      null AS shelfid,null as bid,  NULL  AS PID,null as shelftype,description as cause, 
                      Severity,raisetime as alarmtime, 
                      null AS Alarmacktime, 
                      cleartime AS AlarmRecovertime, 
                      null as status,null as filtrate,null as voice_flag,null as alarmreasonbs, taskid, tasker,null as LEVEL_NO,null as BoardName,
					 TIMELONG,null as marker, receivetime
FROM         ftth_resumealarm
GO
/****** Object:  StoredProcedure [dbo].[spClearOMCAlarm]    Script Date: 07/23/2014 17:01:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[spClearOMCAlarm]
@omcName varchar(32) 
as
update dbo.ftth_activealarm 
	set cleartime=GETDATE()
	where SVRNAME=@omcName and type='omc';

insert into ftth_resumealarm
	(id,CompanyNo,CITYNAME,SVRNAME,description,OMCName,raisetime,cleartime,Severity,ftthTaskID,taskid,type,neid,tasker,TIMELONG, receivetime)
	select id,CompanyNo,CITYNAME,SVRNAME,description,OMCName,raisetime,cleartime,Severity,ftthTaskID,taskid,type,neid,tasker,dbo.F_DIFF_TWODATE(raisetime,cleartime),GETDATE()
		from ftth_activealarm
		where SVRNAME=@omcName and type='omc';
		
delete from ftth_activealarm where SVRNAME=@omcName and type='omc';
GO
/****** Object:  StoredProcedure [dbo].[spClearOltAlarm]    Script Date: 07/23/2014 17:01:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[spClearOltAlarm]
@ftthTaskID varchar(32) 
as
update dbo.ftth_activealarm 
	set cleartime=GETDATE()
	where ftthTaskID=@ftthTaskID;

insert into ftth_resumealarm
	(id,CompanyNo,CITYNAME,SVRNAME,description,OMCName,raisetime,cleartime,Severity,ftthTaskID,taskid,type,neid,tasker,TIMELONG, receivetime)
	select id,CompanyNo,CITYNAME,SVRNAME,description,OMCName,raisetime,cleartime,Severity,ftthTaskID,taskid,type,neid,tasker,dbo.F_DIFF_TWODATE(raisetime,cleartime),GETDATE()
		from ftth_activealarm
		where ftthTaskID=@ftthTaskID;
		
delete from ftth_activealarm where ftthTaskID=@ftthTaskID;
GO
/****** Object:  View [dbo].[V_AlarmData_query]    Script Date: 07/23/2014 17:01:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[V_AlarmData_query]
AS
SELECT     
			CompanyNo,cityname,svrname as servername,null as sn, id AS AlarmID,null as serverid, neid AS oldNeID, null AS NeID, null AS NeExtID, 
                      null AS shelfid,null as bid,  NULL  AS PID,null as shelftype,description as cause, 
                      Severity,raisetime as alarmtime, 
                      null AS Alarmacktime, 
                      cleartime AS AlarmRecovertime, 
                      null as status,null as filtrate,null as voice_flag,null as alarmreasonbs, taskid, tasker,null as LEVEL_NO,null as BoardName,
                      NULL AS CIRCUITNO,NULL AS A_DevType,NULL AS A_DevAddr,NULL AS Z_DevType,NULL AS Z_DevAddr,NULL AS Note
FROM         ftth_activealarm
GO
/****** Object:  View [dbo].[V_AlarmData]    Script Date: 07/23/2014 17:01:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[V_AlarmData]
AS
SELECT     
			CompanyNo,null as sn, id AS AlarmID,null as serverid, neid AS oldNeID, null AS NeID, null AS NeExtID, 
                      null AS shelfid,null as bid,  NULL  AS PID,null as shelftype,description as cause, 
                      Severity,raisetime as alarmtime, 
                      null AS Alarmacktime, 
                      cleartime AS AlarmRecovertime, 
                      null as status,null as filtrate,null as voice_flag,null as alarmreasonbs, taskid, tasker,null as LEVEL_NO,null as BoardName,
                      NULL AS CIRCUITNO,NULL AS A_DevType,NULL AS A_DevAddr,NULL AS Z_DevType,NULL AS Z_DevAddr,NULL AS Note
FROM         ftth_activealarm
GO
/****** Object:  StoredProcedure [dbo].[spClearTask]    Script Date: 07/23/2014 17:01:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-----------------------------------------------------
-- 
--
-- 维护日志
--
-- 维护人		维护时间		描叙
-- ------		--------		----------------
-- 汪传龙		2014-06-17		创建
------------------------------------------------------
CREATE proc [dbo].[spClearTask]
@taskID varchar(32) 
as
insert into TaskHistory
	(id,taskID,taskType,taskStatus,city,manufacturer,omcName,oltID,ponID,onuID,onuType,onuPort,svlan,cvlan
	,phone,sipRegDM,sipUserName,sipUserPWD,responseBoss,responseMsg,responseXML,bossReply,bossReplyStatus,receiveTime
	,executeTime,completeTime,responseTime,isRollback,netInterrupt,netDelay)
	select id,taskID,taskType,taskStatus,city,manufacturer,omcName,oltID,ponID,onuID,onuType,onuPort,svlan,cvlan
	,phone,sipRegDM,sipUserName,sipUserPWD,responseBoss,responseMsg,responseXML,bossReply,bossReplyStatus,receiveTime
	,executeTime,completeTime,responseTime,isRollback,netInterrupt,netDelay
		from Task
		where taskID=@taskID;

insert into CommandHistory
	(id,taskID,commandStep,commandText,completionCode,additionalInfo,generateTime,executeTime,omcTime,localTime,replyContent,isRollback)
	select id,taskID,commandStep,commandText,completionCode,additionalInfo,generateTime,executeTime,omcTime,localTime,replyContent,isRollback
		from Command
		where taskID=@taskID;
		
delete from Command where taskID=@taskID;

delete from Task where taskID=@taskID;
GO
/****** Object:  View [dbo].[vTaskReplyBossMsg]    Script Date: 07/23/2014 17:01:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-----------------------------------------------------
--
--
-- 维护日志
--
-- 维护人		维护时间		描叙
-- ------		--------		----------------
-- 汪传龙		2014-07-07		创建
------------------------------------------------------
CREATE view [dbo].[vTaskReplyBossMsg]
as
select 
	[taskID],
	taskStatus,
	responseMsg,
	CASE WHEN isRollback is NULL then 0
		when isRollback=0 then -1
		else 1 end as isRollback,  
	receiveTime,
	netDelay
	from Task
	where (taskStatus='成功' or taskStatus='失败') AND taskid NOT LIKE 'Test%'
GO
/****** Object:  View [dbo].[vNormalTask]    Script Date: 07/23/2014 17:01:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-----------------------------------------------------
--
--
-- 维护日志
--
-- 维护人		维护时间		描叙
-- ------		--------		----------------
-- 汪传龙		2014-06-19		创建
------------------------------------------------------
CREATE view [dbo].[vNormalTask]
as
select 
	id,
	[taskID],
	taskType,
	city,
	manufacturer,
	omcName,
	oltID,
	ponID,
	onuID,
	onuPort,
	onuType,
	svlan,
	cvlan,	
	phone ,
	sipRegDM ,
	sipUserName ,
	sipUserPWD ,
	'false' as isRelocateTask,
	[receiveTime],
	netInterrupt
	from Task
	where taskType!='移机' and (taskStatus is null or taskStatus='待执行')
GO
/****** Object:  View [dbo].[vNetDelayReplyBossMsg]    Script Date: 07/23/2014 17:01:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE view [dbo].[vNetDelayReplyBossMsg]
as
select 
	[taskID],
	taskStatus,
	responseMsg,
	isRollback,  
	receiveTime,
	netDelay
	from Task
	where (responseBoss is null or responseBoss=0) and (netDelay=1)
GO
/****** Object:  Default [DF_RollbackHisTask_ReceiveTime]    Script Date: 07/23/2014 17:00:33 ******/
ALTER TABLE [dbo].[RollbackHisTask] ADD  CONSTRAINT [DF_RollbackHisTask_ReceiveTime]  DEFAULT (getdate()) FOR [receiveTime]
GO
/****** Object:  Default [DF_OltInfo_ReceiveTime]    Script Date: 07/23/2014 17:00:33 ******/
ALTER TABLE [dbo].[OltInfo] ADD  CONSTRAINT [DF_OltInfo_ReceiveTime]  DEFAULT (getdate()) FOR [receiveTime]
GO
