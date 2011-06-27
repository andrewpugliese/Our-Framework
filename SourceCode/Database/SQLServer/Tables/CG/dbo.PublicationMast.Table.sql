SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PublicationMast](
	[PublicationCod] [smallint] NOT NULL,
	[PublicationNam] [varchar](60) NOT NULL,
	[PublicationShortNam] [varchar](30) NOT NULL,
	[PublisherEntityCod] [int] NOT NULL,
	[PublicationTagLin] [varchar](150) NULL,
	[PublicationImageFilNam] [varchar](64) NULL,
	[PublicationHomeCMPServiceURL] [varchar](150) NULL,
	[PublicationStreamOkFlag] [smallint] NOT NULL,
	[PublicationDownloadOkFlag] [smallint] NOT NULL,
	[RepeatDownloadClickDurationDayCt] [smallint] NULL,
	[PublicationAffiliateOkFlag] [smallint] NOT NULL,
	[PurchasePercentShareCMPService] [smallint] NULL,
	[PurchasePercentSharePool] [smallint] NULL,
	[PurchasePercentSharePublisher] [smallint] NULL,
	[PoolPercentShareContentProvider] [smallint] NULL,
	[PoolPercentSharePrimaryEditor] [smallint] NULL,
	[PoolPercentShareSeniorEditor] [smallint] NULL,
	[PublicationContactUsrSortNam] [varchar](60) NULL,
	[PublicationEmailAddr] [varchar](64) NULL,
	[PublicationDescription] [varchar](1000) NULL,
	[PublicationPausedFlag] [smallint] NOT NULL,
	[PublicationStatCod] [smallint] NOT NULL,
	[PublicationInactiveDate] [date] NULL,
	[LastModUsrCod] [int] NOT NULL,
	[LastModDate] [date] NOT NULL,
	[LastModTim] [char](12) NOT NULL,
 CONSTRAINT [PublicationMast_PK] PRIMARY KEY NONCLUSTERED 
(
	[PublicationCod] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX [PublicationMast00] ON [dbo].[PublicationMast] 
(
	[LastModUsrCod] ASC,
	[LastModDate] ASC,
	[LastModTim] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX [PublicationMast02] ON [dbo].[PublicationMast] 
(
	[PublicationNam] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX [PublicationMast03] ON [dbo].[PublicationMast] 
(
	[PublicationShortNam] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX [PublicationMast04] ON [dbo].[PublicationMast] 
(
	[PublisherEntityCod] ASC,
	[PublicationCod] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[PublicationMast]  WITH CHECK ADD  CONSTRAINT [PublicationMast_FK01] FOREIGN KEY([PublisherEntityCod])
REFERENCES [dbo].[PublisherMast] ([PublisherEntityCod])
GO
ALTER TABLE [dbo].[PublicationMast] CHECK CONSTRAINT [PublicationMast_FK01]
GO
ALTER TABLE [dbo].[PublicationMast]  WITH CHECK ADD  CONSTRAINT [PublicationMast_FK02] FOREIGN KEY([PublicationContactUsrSortNam])
REFERENCES [dbo].[B1_USRMAST] ([USRSORTNAM])
GO
ALTER TABLE [dbo].[PublicationMast] CHECK CONSTRAINT [PublicationMast_FK02]
GO
ALTER TABLE [dbo].[PublicationMast]  WITH CHECK ADD  CONSTRAINT [PublicationMast_FK03] FOREIGN KEY([PublicationStatCod])
REFERENCES [dbo].[EntityStat] ([EntityStatCod])
GO
ALTER TABLE [dbo].[PublicationMast] CHECK CONSTRAINT [PublicationMast_FK03]
GO
