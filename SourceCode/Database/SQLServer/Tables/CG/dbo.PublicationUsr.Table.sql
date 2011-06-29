SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PublicationUsr](
	[PublicationCod] [smallint] NOT NULL,
	[UsrCod] [int] NOT NULL,
	[UsrSortNam] [varchar](60) NULL,
	[MemberPurchaseTranCod] [smallint] NULL,
	[MemberPurchaseDetailCod] [smallint] NULL,
	[SubscriptionOfferCod] [smallint] NULL,
	[SubscriptionStreamOkFlag] [smallint] NOT NULL,
	[SubscriptionDownloadOkFlag] [smallint] NOT NULL,
	[SubscriptionUsageExceededFlag] [smallint] NOT NULL,
	[SubscriptionExpirationDate] [date] NULL,
	[SubscriptionStartDate] [date] NULL,
	[UsrIsContentProviderFlag] [smallint] NOT NULL,
	[UsrIsEditorFlag] [smallint] NOT NULL,
	[UsrIsPublisherFlag] [smallint] NOT NULL,
	[UsrIsAdvertiserFlag] [smallint] NOT NULL,
	[UsrIsCMPServiceStaffFlag] [smallint] NOT NULL,
	[LastModUsrCod] [int] NOT NULL,
	[LastModDate] [date] NOT NULL,
	[LastModTim] [char](12) NOT NULL,
 CONSTRAINT [PublicationUsr_PK] PRIMARY KEY NONCLUSTERED 
(
	[PublicationCod] ASC,
	[UsrCod] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX [PublicationUsr00] ON [dbo].[PublicationUsr] 
(
	[LastModUsrCod] ASC,
	[LastModDate] ASC,
	[LastModTim] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[PublicationUsr]  WITH CHECK ADD  CONSTRAINT [PublicationUsr_FK01] FOREIGN KEY([PublicationCod])
REFERENCES [dbo].[PublicationMast] ([PublicationCod])
GO
ALTER TABLE [dbo].[PublicationUsr] CHECK CONSTRAINT [PublicationUsr_FK01]
GO
ALTER TABLE [dbo].[PublicationUsr]  WITH CHECK ADD  CONSTRAINT [PublicationUsr_FK02] FOREIGN KEY([UsrCod])
REFERENCES [dbo].[B1_USRMAST] ([USRCOD])
GO
ALTER TABLE [dbo].[PublicationUsr] CHECK CONSTRAINT [PublicationUsr_FK02]
GO
ALTER TABLE [dbo].[PublicationUsr]  WITH CHECK ADD  CONSTRAINT [PublicationUsr_FK03] FOREIGN KEY([UsrSortNam])
REFERENCES [dbo].[MemberMast] ([UsrSortNam])
GO
ALTER TABLE [dbo].[PublicationUsr] CHECK CONSTRAINT [PublicationUsr_FK03]
GO
