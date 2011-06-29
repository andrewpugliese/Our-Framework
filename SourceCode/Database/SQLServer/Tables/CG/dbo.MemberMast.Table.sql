SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MemberMast](
	[UsrCod] [int] NOT NULL,
	[UsrSortNam] [varchar](60) NOT NULL,
	[MemberEmailAddr] [varchar](64) NOT NULL,
	[MemberIsPublisherFlag] [smallint] NOT NULL,
	[MemberIsContentProviderFlag] [smallint] NOT NULL,
	[MemberIsEditorFlag] [smallint] NOT NULL,
	[MemberIsAffiliateFlag] [smallint] NOT NULL,
	[MemberIsAdvertiserFlag] [smallint] NOT NULL,
	[MemberIsFinancialServiceFlag] [smallint] NOT NULL,
	[MemberIsCMPServiceStaffFlag] [smallint] NOT NULL,
	[MemberRegistrationDate] [date] NOT NULL,
	[MemberLastLogonDate] [date] NULL,
	[MemberInactiveDate] [date] NULL,
	[ShoppingCartStatCod] [smallint] NOT NULL,
	[ShoppingCartOrigContentURL] [varchar](300) NULL,
	[ShoppingCartReturnURL] [varchar](300) NULL,
	[MemberRem] [varchar](1000) NULL,
	[LastModUsrCod] [int] NOT NULL,
	[LastModDate] [date] NOT NULL,
	[LastModTim] [char](12) NOT NULL,
 CONSTRAINT [MemberMast_PK] PRIMARY KEY NONCLUSTERED 
(
	[UsrCod] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX [MemberMast00] ON [dbo].[MemberMast] 
(
	[LastModUsrCod] ASC,
	[LastModDate] ASC,
	[LastModTim] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX [MemberMast02] ON [dbo].[MemberMast] 
(
	[UsrSortNam] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[MemberMast]  WITH CHECK ADD  CONSTRAINT [MemberMast_FK01] FOREIGN KEY([UsrCod])
REFERENCES [dbo].[B1_USRMAST] ([USRCOD])
GO
ALTER TABLE [dbo].[MemberMast] CHECK CONSTRAINT [MemberMast_FK01]
GO
ALTER TABLE [dbo].[MemberMast]  WITH CHECK ADD  CONSTRAINT [MemberMast_FK02] FOREIGN KEY([UsrSortNam])
REFERENCES [dbo].[B1_USRMAST] ([USRSORTNAM])
GO
ALTER TABLE [dbo].[MemberMast] CHECK CONSTRAINT [MemberMast_FK02]
GO
ALTER TABLE [dbo].[MemberMast]  WITH CHECK ADD  CONSTRAINT [MemberMast_FK03] FOREIGN KEY([ShoppingCartStatCod])
REFERENCES [dbo].[ShoppingCartStat] ([ShoppingCartStatCod])
GO
ALTER TABLE [dbo].[MemberMast] CHECK CONSTRAINT [MemberMast_FK03]
GO
