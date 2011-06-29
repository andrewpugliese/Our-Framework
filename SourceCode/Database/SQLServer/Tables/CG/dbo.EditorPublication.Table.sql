SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EditorPublication](
	[PublicationCod] [smallint] NOT NULL,
	[EditorEntityCod] [int] NOT NULL,
	[EditorNam] [varchar](60) NOT NULL,
	[PublisherEntityCod] [int] NOT NULL,
	[IsSeniorEditorFlag] [smallint] NOT NULL,
	[ParentEditorEntityCod] [int] NULL,
	[EditorPublicationInactiveDate] [date] NULL,
	[EditorRem] [varchar](1000) NULL,
	[LastModUsrCod] [int] NOT NULL,
	[LastModDate] [date] NOT NULL,
	[LastModTim] [char](12) NOT NULL,
 CONSTRAINT [EditorPublication_PK] PRIMARY KEY NONCLUSTERED 
(
	[PublicationCod] ASC,
	[EditorEntityCod] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX [EditorPublication00] ON [dbo].[EditorPublication] 
(
	[LastModUsrCod] ASC,
	[LastModDate] ASC,
	[LastModTim] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[EditorPublication]  WITH CHECK ADD  CONSTRAINT [EditorPublication_FK01] FOREIGN KEY([PublicationCod])
REFERENCES [dbo].[PublicationMast] ([PublicationCod])
GO
ALTER TABLE [dbo].[EditorPublication] CHECK CONSTRAINT [EditorPublication_FK01]
GO
ALTER TABLE [dbo].[EditorPublication]  WITH CHECK ADD  CONSTRAINT [EditorPublication_FK02] FOREIGN KEY([EditorEntityCod])
REFERENCES [dbo].[EditorMast] ([EditorEntityCod])
GO
ALTER TABLE [dbo].[EditorPublication] CHECK CONSTRAINT [EditorPublication_FK02]
GO
ALTER TABLE [dbo].[EditorPublication]  WITH CHECK ADD  CONSTRAINT [EditorPublication_FK03] FOREIGN KEY([EditorNam])
REFERENCES [dbo].[EditorMast] ([EditorNam])
GO
ALTER TABLE [dbo].[EditorPublication] CHECK CONSTRAINT [EditorPublication_FK03]
GO
ALTER TABLE [dbo].[EditorPublication]  WITH CHECK ADD  CONSTRAINT [EditorPublication_FK04] FOREIGN KEY([PublisherEntityCod])
REFERENCES [dbo].[PublisherMast] ([PublisherEntityCod])
GO
ALTER TABLE [dbo].[EditorPublication] CHECK CONSTRAINT [EditorPublication_FK04]
GO
ALTER TABLE [dbo].[EditorPublication]  WITH CHECK ADD  CONSTRAINT [EditorPublication_FK05] FOREIGN KEY([ParentEditorEntityCod])
REFERENCES [dbo].[EditorMast] ([EditorEntityCod])
GO
ALTER TABLE [dbo].[EditorPublication] CHECK CONSTRAINT [EditorPublication_FK05]
GO
