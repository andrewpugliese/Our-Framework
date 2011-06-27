SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ShoppingCartItm](
	[UsrCod] [int] NOT NULL,
	[PublicationCod] [smallint] NOT NULL,
	[SubscriptionOfferCod] [smallint] NOT NULL,
	[LastModUsrCod] [int] NOT NULL,
	[LastModDate] [date] NOT NULL,
	[LastModTim] [char](12) NOT NULL,
 CONSTRAINT [ShoppingCartItm_PK] PRIMARY KEY NONCLUSTERED 
(
	[UsrCod] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX [ShoppingCartItm00] ON [dbo].[ShoppingCartItm] 
(
	[LastModUsrCod] ASC,
	[LastModDate] ASC,
	[LastModTim] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[ShoppingCartItm]  WITH CHECK ADD  CONSTRAINT [ShoppingCartItm_FK01] FOREIGN KEY([UsrCod])
REFERENCES [dbo].[MemberMast] ([UsrCod])
GO
ALTER TABLE [dbo].[ShoppingCartItm] CHECK CONSTRAINT [ShoppingCartItm_FK01]
GO
ALTER TABLE [dbo].[ShoppingCartItm]  WITH CHECK ADD  CONSTRAINT [ShoppingCartItm_FK02] FOREIGN KEY([PublicationCod], [SubscriptionOfferCod])
REFERENCES [dbo].[SubscriptionOffer] ([PublicationCod], [SubscriptionOfferCod])
GO
ALTER TABLE [dbo].[ShoppingCartItm] CHECK CONSTRAINT [ShoppingCartItm_FK02]
GO
