SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SubscriptionOffer](
	[PublicationCod] [smallint] NOT NULL,
	[SubscriptionOfferCod] [smallint] NOT NULL,
	[SubscriptionOfferDescription] [varchar](60) NOT NULL,
	[SubscriptionOfferActiveFlag] [smallint] NOT NULL,
	[SubscriptionLstPrice] [numeric](9, 4) NOT NULL,
	[RecurringBillingFlag] [smallint] NOT NULL,
	[SalesTaxFlag] [smallint] NOT NULL,
	[SubscriptionPeriodTypCod] [smallint] NOT NULL,
	[SubscriptionStreamOkFlag] [smallint] NOT NULL,
	[SubscriptionDownloadOkFlag] [smallint] NOT NULL,
	[MaxStreamDurationSeconds] [numeric](12, 0) NULL,
	[MaxDownloadDownloadByteCt] [numeric](15, 0) NULL,
	[MaxDownloadCt] [int] NULL,
	[DiscountPriceThisPublisher] [numeric](9, 4) NULL,
	[DiscountPriceCrossSellPublisher] [numeric](9, 4) NULL,
	[SubscriptionOfferRem] [varchar](1000) NULL,
	[LastModUsrCod] [int] NOT NULL,
	[LastModDate] [date] NOT NULL,
	[LastModTim] [char](12) NOT NULL,
 CONSTRAINT [SubscriptionOffer_PK] PRIMARY KEY NONCLUSTERED 
(
	[PublicationCod] ASC,
	[SubscriptionOfferCod] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX [SubscriptionOffer00] ON [dbo].[SubscriptionOffer] 
(
	[LastModUsrCod] ASC,
	[LastModDate] ASC,
	[LastModTim] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[SubscriptionOffer]  WITH CHECK ADD  CONSTRAINT [SubscriptionOffer_FK01] FOREIGN KEY([PublicationCod])
REFERENCES [dbo].[PublicationMast] ([PublicationCod])
GO
ALTER TABLE [dbo].[SubscriptionOffer] CHECK CONSTRAINT [SubscriptionOffer_FK01]
GO
ALTER TABLE [dbo].[SubscriptionOffer]  WITH CHECK ADD  CONSTRAINT [SubscriptionOffer_FK02] FOREIGN KEY([SubscriptionPeriodTypCod])
REFERENCES [dbo].[SubscriptionPeriodTyp] ([SubscriptionPeriodTypCod])
GO
ALTER TABLE [dbo].[SubscriptionOffer] CHECK CONSTRAINT [SubscriptionOffer_FK02]
GO
