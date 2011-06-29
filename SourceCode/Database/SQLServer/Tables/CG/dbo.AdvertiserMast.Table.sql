SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AdvertiserMast](
	[AdvertiserEntityCod] [int] NOT NULL,
	[AdvertiserNam] [varchar](60) NOT NULL,
	[AdvertiserContactUsrSortNam] [varchar](60) NULL,
	[AdvertiserEmailAddr] [varchar](64) NULL,
	[AdvertiserPausedFlag] [smallint] NOT NULL,
	[AdvertiserStatCod] [smallint] NOT NULL,
	[AdvertiserInactiveDate] [date] NULL,
	[LastModUsrCod] [int] NOT NULL,
	[LastModDate] [date] NOT NULL,
	[LastModTim] [char](12) NOT NULL,
 CONSTRAINT [AdvertiserMast_PK] PRIMARY KEY NONCLUSTERED 
(
	[AdvertiserEntityCod] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX [AdvertiserMast00] ON [dbo].[AdvertiserMast] 
(
	[LastModUsrCod] ASC,
	[LastModDate] ASC,
	[LastModTim] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX [AdvertiserMast02] ON [dbo].[AdvertiserMast] 
(
	[AdvertiserNam] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[AdvertiserMast]  WITH CHECK ADD  CONSTRAINT [AdvertiserMast_FK01] FOREIGN KEY([AdvertiserEntityCod])
REFERENCES [dbo].[EntityMast] ([EntityCod])
GO
ALTER TABLE [dbo].[AdvertiserMast] CHECK CONSTRAINT [AdvertiserMast_FK01]
GO
ALTER TABLE [dbo].[AdvertiserMast]  WITH CHECK ADD  CONSTRAINT [AdvertiserMast_FK02] FOREIGN KEY([AdvertiserNam])
REFERENCES [dbo].[EntityMast] ([EntityNam])
GO
ALTER TABLE [dbo].[AdvertiserMast] CHECK CONSTRAINT [AdvertiserMast_FK02]
GO
ALTER TABLE [dbo].[AdvertiserMast]  WITH CHECK ADD  CONSTRAINT [AdvertiserMast_FK03] FOREIGN KEY([AdvertiserContactUsrSortNam])
REFERENCES [dbo].[MemberMast] ([UsrSortNam])
GO
ALTER TABLE [dbo].[AdvertiserMast] CHECK CONSTRAINT [AdvertiserMast_FK03]
GO
ALTER TABLE [dbo].[AdvertiserMast]  WITH CHECK ADD  CONSTRAINT [AdvertiserMast_FK04] FOREIGN KEY([AdvertiserStatCod])
REFERENCES [dbo].[EntityStat] ([EntityStatCod])
GO
ALTER TABLE [dbo].[AdvertiserMast] CHECK CONSTRAINT [AdvertiserMast_FK04]
GO
ALTER TABLE [dbo].[AdvertiserMast] ADD  DEFAULT ('0') FOR [AdvertiserPausedFlag]
GO
ALTER TABLE [dbo].[AdvertiserMast] ADD  DEFAULT ('300') FOR [AdvertiserStatCod]
GO
