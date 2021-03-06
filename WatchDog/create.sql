if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[WatchDog]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[WatchDog]
GO

CREATE TABLE [dbo].[WatchDog] (
	[host] [varchar] (64) COLLATE Chinese_PRC_CI_AS NOT NULL ,
	[component] [varchar] (64) COLLATE Chinese_PRC_CI_AS NOT NULL ,
	[refresh_interval] [int] NULL ,
	[update_time] [datetime] NULL ,
	[check_time] [datetime] NULL ,
	[pid] [int] NULL 
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[WatchDog] WITH NOCHECK ADD 
	CONSTRAINT [PK_WatchDog] PRIMARY KEY  CLUSTERED 
	(
		[host],
		[component]
	)  ON [PRIMARY] 
GO

