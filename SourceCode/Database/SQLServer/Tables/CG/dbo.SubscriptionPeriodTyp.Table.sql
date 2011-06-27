SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SubscriptionPeriodTyp](
	[SubscriptionPeriodTypCod] [smallint] NOT NULL,
	[SubscriptionPeriodTypNam] [char](20) NOT NULL,
	[SubscriptionPeriodTypRem] [varchar](300) NULL,
	[LastModUsrCod] [int] NOT NULL,
	[LastModDate] [date] NOT NULL,
	[LastModTim] [char](12) NOT NULL,
 CONSTRAINT [SubscriptionPeriodTyp_PK] PRIMARY KEY NONCLUSTERED 
(
	[SubscriptionPeriodTypCod] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX [SubscriptionPeriodTyp00] ON [dbo].[SubscriptionPeriodTyp] 
(
	[LastModUsrCod] ASC,
	[LastModDate] ASC,
	[LastModTim] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX [SubscriptionPeriodTyp02] ON [dbo].[SubscriptionPeriodTyp] 
(
	[SubscriptionPeriodTypNam] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
