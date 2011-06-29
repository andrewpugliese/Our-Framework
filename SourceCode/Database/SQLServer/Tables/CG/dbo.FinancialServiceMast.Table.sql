SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FinancialServiceMast](
	[FinancialServiceEntityCod] [int] NOT NULL,
	[FinancialServiceNam] [varchar](60) NOT NULL,
	[FinancialServiceContactUsrSortNam] [varchar](60) NULL,
	[FinancialServiceEmailAddr] [varchar](64) NULL,
	[FinancialServicePausedFlag] [smallint] NOT NULL,
	[FinancialServiceStatCod] [smallint] NOT NULL,
	[FinancialServiceInactiveDate] [date] NULL,
	[LastModUsrCod] [int] NOT NULL,
	[LastModDate] [date] NOT NULL,
	[LastModTim] [char](12) NOT NULL,
 CONSTRAINT [FinancialServiceMast_PK] PRIMARY KEY NONCLUSTERED 
(
	[FinancialServiceEntityCod] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX [FinancialServiceMast00] ON [dbo].[FinancialServiceMast] 
(
	[LastModUsrCod] ASC,
	[LastModDate] ASC,
	[LastModTim] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX [FinancialServiceMast02] ON [dbo].[FinancialServiceMast] 
(
	[FinancialServiceNam] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[FinancialServiceMast]  WITH CHECK ADD  CONSTRAINT [FinancialServiceMast_FK01] FOREIGN KEY([FinancialServiceEntityCod])
REFERENCES [dbo].[EntityMast] ([EntityCod])
GO
ALTER TABLE [dbo].[FinancialServiceMast] CHECK CONSTRAINT [FinancialServiceMast_FK01]
GO
ALTER TABLE [dbo].[FinancialServiceMast]  WITH CHECK ADD  CONSTRAINT [FinancialServiceMast_FK02] FOREIGN KEY([FinancialServiceContactUsrSortNam])
REFERENCES [dbo].[MemberMast] ([UsrSortNam])
GO
ALTER TABLE [dbo].[FinancialServiceMast] CHECK CONSTRAINT [FinancialServiceMast_FK02]
GO
ALTER TABLE [dbo].[FinancialServiceMast]  WITH CHECK ADD  CONSTRAINT [FinancialServiceMast_FK03] FOREIGN KEY([FinancialServiceStatCod])
REFERENCES [dbo].[EntityStat] ([EntityStatCod])
GO
ALTER TABLE [dbo].[FinancialServiceMast] CHECK CONSTRAINT [FinancialServiceMast_FK03]
GO
ALTER TABLE [dbo].[FinancialServiceMast] ADD  DEFAULT ('0') FOR [FinancialServicePausedFlag]
GO
ALTER TABLE [dbo].[FinancialServiceMast] ADD  DEFAULT ('300') FOR [FinancialServiceStatCod]
GO
