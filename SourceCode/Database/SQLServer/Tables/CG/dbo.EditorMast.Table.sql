SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EditorMast](
	[EditorEntityCod] [int] NOT NULL,
	[EditorNam] [varchar](60) NOT NULL,
	[EditorContactUsrSortNam] [varchar](60) NULL,
	[EditorEmailAddr] [varchar](64) NULL,
	[EditorPausedFlag] [smallint] NOT NULL,
	[EditorStatCod] [smallint] NOT NULL,
	[EditorInactiveDate] [date] NULL,
	[LastModUsrCod] [int] NOT NULL,
	[LastModDate] [date] NOT NULL,
	[LastModTim] [char](12) NOT NULL,
 CONSTRAINT [EditorMast_PK] PRIMARY KEY NONCLUSTERED 
(
	[EditorEntityCod] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX [EditorMastMast00] ON [dbo].[EditorMast] 
(
	[LastModUsrCod] ASC,
	[LastModDate] ASC,
	[LastModTim] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX [EditorMastMast02] ON [dbo].[EditorMast] 
(
	[EditorNam] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[EditorMast]  WITH CHECK ADD  CONSTRAINT [EditorMast_FK01] FOREIGN KEY([EditorEntityCod])
REFERENCES [dbo].[EntityMast] ([EntityCod])
GO
ALTER TABLE [dbo].[EditorMast] CHECK CONSTRAINT [EditorMast_FK01]
GO
ALTER TABLE [dbo].[EditorMast]  WITH CHECK ADD  CONSTRAINT [EditorMast_FK02] FOREIGN KEY([EditorNam])
REFERENCES [dbo].[EntityMast] ([EntityNam])
GO
ALTER TABLE [dbo].[EditorMast] CHECK CONSTRAINT [EditorMast_FK02]
GO
ALTER TABLE [dbo].[EditorMast]  WITH CHECK ADD  CONSTRAINT [EditorMast_FK03] FOREIGN KEY([EditorContactUsrSortNam])
REFERENCES [dbo].[MemberMast] ([UsrSortNam])
GO
ALTER TABLE [dbo].[EditorMast] CHECK CONSTRAINT [EditorMast_FK03]
GO
ALTER TABLE [dbo].[EditorMast]  WITH CHECK ADD  CONSTRAINT [EditorMast_FK04] FOREIGN KEY([EditorStatCod])
REFERENCES [dbo].[EntityStat] ([EntityStatCod])
GO
ALTER TABLE [dbo].[EditorMast] CHECK CONSTRAINT [EditorMast_FK04]
GO
ALTER TABLE [dbo].[EditorMast] ADD  DEFAULT ('0') FOR [EditorPausedFlag]
GO
ALTER TABLE [dbo].[EditorMast] ADD  DEFAULT ('300') FOR [EditorStatCod]
GO
