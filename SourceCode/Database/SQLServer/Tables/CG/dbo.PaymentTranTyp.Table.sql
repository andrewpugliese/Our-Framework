SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PaymentTranTyp](
	[PaymentTranTypCod] [smallint] NOT NULL,
	[PaymentTranTypNam] [varchar](60) NOT NULL,
	[LedgerAccountNum] [smallint] NOT NULL,
	[DebitAccountFlag] [smallint] NOT NULL,
	[DebitTranFlag] [smallint] NOT NULL,
	[OkForMemberFlag] [smallint] NOT NULL,
	[OkForPublisherFlag] [smallint] NOT NULL,
	[OkForContentProviderFlag] [smallint] NOT NULL,
	[OkForEditorFlag] [smallint] NOT NULL,
	[OkForAffiliateFlag] [smallint] NOT NULL,
	[OkForFinancialServiceFlag] [smallint] NOT NULL,
	[PaymentTranTypRem] [varchar](500) NULL,
	[LastModUsrCod] [int] NOT NULL,
	[LastModDate] [date] NOT NULL,
	[LastModTim] [char](12) NOT NULL,
 CONSTRAINT [PaymentTranTyp_PK] PRIMARY KEY NONCLUSTERED 
(
	[PaymentTranTypCod] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX [PaymentTranTyp00] ON [dbo].[PaymentTranTyp] 
(
	[LastModUsrCod] ASC,
	[LastModDate] ASC,
	[LastModTim] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX [PaymentTranTyp02] ON [dbo].[PaymentTranTyp] 
(
	[PaymentTranTypNam] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[PaymentTranTyp]  WITH CHECK ADD  CONSTRAINT [PaymentTranTyp_FK01] FOREIGN KEY([LedgerAccountNum])
REFERENCES [dbo].[LedgerAccountMast] ([LedgerAccountNum])
GO
ALTER TABLE [dbo].[PaymentTranTyp] CHECK CONSTRAINT [PaymentTranTyp_FK01]
GO
