SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ShoppingCartStat](
	[ShoppingCartStatCod] [smallint] NOT NULL,
	[ShoppingCartStatNam] [varchar](27) NOT NULL,
	[ShoppingCartStatRem] [varchar](300) NULL,
	[LastModUsrCod] [int] NOT NULL,
	[LastModDate] [date] NOT NULL,
	[LastModTim] [char](12) NOT NULL,
 CONSTRAINT [ShoppingCartStat_PK] PRIMARY KEY NONCLUSTERED 
(
	[ShoppingCartStatCod] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX [ShoppingCartStat00] ON [dbo].[ShoppingCartStat] 
(
	[LastModUsrCod] ASC,
	[LastModDate] ASC,
	[LastModTim] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX [ShoppingCartStat02] ON [dbo].[ShoppingCartStat] 
(
	[ShoppingCartStatNam] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO