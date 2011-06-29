-- Load ContentGalaxy initial data
--

INSERT INTO ShoppingCartStat (ShoppingCartStatCod,ShoppingCartStatNam,ShoppingCartStatRem,LastModUsrCod,LastModDate,LastModTim)VALUES(100,'100 - Empty','Shopping cart starts out empty and ends up empty when the purchase is successful.',0,'2011-06-24','143249000000')
INSERT INTO ShoppingCartStat (ShoppingCartStatCod,ShoppingCartStatNam,ShoppingCartStatRem,LastModUsrCod,LastModDate,LastModTim)VALUES(200,'200 - Has item','Shopping cart has one or more items (SubscriptionOffers) ready to purchase If payment fails, shopping cart is returned to this status as the last step of post payment processing.',0,'2011-06-24','143249000001')
INSERT INTO ShoppingCartStat (ShoppingCartStatCod,ShoppingCartStatNam,ShoppingCartStatRem,LastModUsrCod,LastModDate,LastModTim)VALUES(300,'300 - Locked for payment','Shopping cart is locked in this status until the chechout (purchase) returns, either successfully or not.',0,'2011-06-24','143249000002')
INSERT INTO ShoppingCartStat (ShoppingCartStatCod,ShoppingCartStatNam,ShoppingCartStatRem,LastModUsrCod,LastModDate,LastModTim)VALUES(400,'400 - Locked for completion','Shopping cart is locked in this status after payment processing returns and until all post payment processing completes.',0,'2011-06-24','143249000003')


INSERT INTO EntityTyp (EntityTypCod,EntityTypNam,EntityTypRem,LastModUsrCod,LastModDate,LastModTim)VALUES(100,'Person','ENTITY_TypPerson',0,'2011-06-24','143249000004')
INSERT INTO EntityTyp (EntityTypCod,EntityTypNam,EntityTypRem,LastModUsrCod,LastModDate,LastModTim)VALUES(300,'Corporation, USA','ENTITY_TypCorpUSA',0,'2011-06-24','143249000005')
INSERT INTO EntityTyp (EntityTypCod,EntityTypNam,EntityTypRem,LastModUsrCod,LastModDate,LastModTim)VALUES(800,'Other organization','ENTITY_TypOtherOrg e.g. individual(s) doing-business-as an entity',0,'2011-06-24','143249000006')
INSERT INTO EntityTyp (EntityTypCod,EntityTypNam,EntityTypRem,LastModUsrCod,LastModDate,LastModTim)VALUES(900,'CMP service','Entity_TypCMPService',0,'2011-06-24','143249000007')

INSERT INTO EntityStat (EntityStatCod,EntityStatNam,EntityStatRem,LastModUsrCod,LastModDate,LastModTim)VALUES(100,'100-Inactive','Inactive entities are prospects or former Active entities',0,'2011-06-24','143249000015')
INSERT INTO EntityStat (EntityStatCod,EntityStatNam,EntityStatRem,LastModUsrCod,LastModDate,LastModTim)VALUES(200,'200-Pending','Pending entities are either in the process of being activated or inactivated',0,'2011-06-24','143249000016')
INSERT INTO EntityStat (EntityStatCod,EntityStatNam,EntityStatRem,LastModUsrCod,LastModDate,LastModTim)VALUES(300,'300-Active','Active entities are fully functional',0,'2011-06-24','143249000017')

INSERT INTO EntityMast (EntityCod,EntityNam,EntityTypCod,MainContactUsrSortNam,EntityEmailAddr,DemoEntityFlag,EntityIsPublisherFlag,EntityIsContentProviderFlag,EntityIsEditorFlag,EntityIsAffiliateFlag,EntityIsAdvertiserFlag,EntityIsFinancialServiceFlag,EntityIsCMPServiceFlag,EntityRegistrationDate,EntityInactiveDate,EntityRem,LastModUsrCod,LastModDate,LastModTim)VALUES(1,'Dummy Entity Master record',800,NULL,NULL,1,0,0,0,0,0,0,0,'2009-04-23',NULL,'An Entity Code of 1 (one) is used in Entity Access records to indicate that the associated user has access to all entities',0,'2011-06-24','143250000000')
INSERT INTO EntityMast (EntityCod,EntityNam,EntityTypCod,MainContactUsrSortNam,EntityEmailAddr,DemoEntityFlag,EntityIsPublisherFlag,EntityIsContentProviderFlag,EntityIsEditorFlag,EntityIsAffiliateFlag,EntityIsAdvertiserFlag,EntityIsFinancialServiceFlag,EntityIsCMPServiceFlag,EntityRegistrationDate,EntityInactiveDate,EntityRem,LastModUsrCod,LastModDate,LastModTim)VALUES(1001,'Content Galaxy Inc.',300,'asherman, steven, 1001','info@contentgalaxy.com',1,1,0,0,0,0,0,1,'2009-04-23',NULL,NULL,0,'2011-06-24','143250000069')
INSERT INTO EntityMast (EntityCod,EntityNam,EntityTypCod,MainContactUsrSortNam,EntityEmailAddr,DemoEntityFlag,EntityIsPublisherFlag,EntityIsContentProviderFlag,EntityIsEditorFlag,EntityIsAffiliateFlag,EntityIsAdvertiserFlag,EntityIsFinancialServiceFlag,EntityIsCMPServiceFlag,EntityRegistrationDate,EntityInactiveDate,EntityRem,LastModUsrCod,LastModDate,LastModTim)VALUES(1002,'Base One International Corporation',300,'asherman, steven, 1001','info@boic.com',1,0,1,0,1,0,0,0,'2009-04-23',NULL,NULL,0,'2011-06-24','143250000070')
INSERT INTO EntityMast (EntityCod,EntityNam,EntityTypCod,MainContactUsrSortNam,EntityEmailAddr,DemoEntityFlag,EntityIsPublisherFlag,EntityIsContentProviderFlag,EntityIsEditorFlag,EntityIsAffiliateFlag,EntityIsAdvertiserFlag,EntityIsFinancialServiceFlag,EntityIsCMPServiceFlag,EntityRegistrationDate,EntityInactiveDate,EntityRem,LastModUsrCod,LastModDate,LastModTim)VALUES(1003,'Victor Costa',100,'costa, victor, 1101','vcosta@abc.com',1,0,1,0,1,0,0,0,'2009-04-23',NULL,NULL,0,'2011-06-24','143250000071')
INSERT INTO EntityMast (EntityCod,EntityNam,EntityTypCod,MainContactUsrSortNam,EntityEmailAddr,DemoEntityFlag,EntityIsPublisherFlag,EntityIsContentProviderFlag,EntityIsEditorFlag,EntityIsAffiliateFlag,EntityIsAdvertiserFlag,EntityIsFinancialServiceFlag,EntityIsCMPServiceFlag,EntityRegistrationDate,EntityInactiveDate,EntityRem,LastModUsrCod,LastModDate,LastModTim)VALUES(501,'Sample Publisher Inc.',300,'miller, barney, 601','publisher01@abc.com',1,1,0,0,1,0,0,0,'2009-04-23',NULL,NULL,0,'2011-06-24','143259000000')
INSERT INTO EntityMast (EntityCod,EntityNam,EntityTypCod,MainContactUsrSortNam,EntityEmailAddr,DemoEntityFlag,EntityIsPublisherFlag,EntityIsContentProviderFlag,EntityIsEditorFlag,EntityIsAffiliateFlag,EntityIsAdvertiserFlag,EntityIsFinancialServiceFlag,EntityIsCMPServiceFlag,EntityRegistrationDate,EntityInactiveDate,EntityRem,LastModUsrCod,LastModDate,LastModTim)VALUES(551,'Leonardo Da Vinci',100,'da vinci, leonardo, 651','contentprovider01@abc.com',1,0,1,0,1,0,0,0,'2009-04-23',NULL,NULL,0,'2011-06-24','143259000001')
INSERT INTO EntityMast (EntityCod,EntityNam,EntityTypCod,MainContactUsrSortNam,EntityEmailAddr,DemoEntityFlag,EntityIsPublisherFlag,EntityIsContentProviderFlag,EntityIsEditorFlag,EntityIsAffiliateFlag,EntityIsAdvertiserFlag,EntityIsFinancialServiceFlag,EntityIsCMPServiceFlag,EntityRegistrationDate,EntityInactiveDate,EntityRem,LastModUsrCod,LastModDate,LastModTim)VALUES(601,'Paula Jones',100,'jones, paula, 701','editor01@abc.com',1,0,0,1,1,0,0,0,'2009-04-23',NULL,NULL,0,'2011-06-24','143259000002')
INSERT INTO EntityMast (EntityCod,EntityNam,EntityTypCod,MainContactUsrSortNam,EntityEmailAddr,DemoEntityFlag,EntityIsPublisherFlag,EntityIsContentProviderFlag,EntityIsEditorFlag,EntityIsAffiliateFlag,EntityIsAdvertiserFlag,EntityIsFinancialServiceFlag,EntityIsCMPServiceFlag,EntityRegistrationDate,EntityInactiveDate,EntityRem,LastModUsrCod,LastModDate,LastModTim)VALUES(602,'Archibald Cox III',100,'cox, archibald, 702','editor02@abc.com',1,0,0,1,1,0,0,0,'2009-04-23',NULL,NULL,0,'2011-06-24','143259000003')
INSERT INTO EntityMast (EntityCod,EntityNam,EntityTypCod,MainContactUsrSortNam,EntityEmailAddr,DemoEntityFlag,EntityIsPublisherFlag,EntityIsContentProviderFlag,EntityIsEditorFlag,EntityIsAffiliateFlag,EntityIsAdvertiserFlag,EntityIsFinancialServiceFlag,EntityIsCMPServiceFlag,EntityRegistrationDate,EntityInactiveDate,EntityRem,LastModUsrCod,LastModDate,LastModTim)VALUES(603,'Dylan Thomas',100,'thomas, dylan, 703','editor03@abc.com',1,0,0,1,1,0,0,0,'2009-04-23',NULL,NULL,0,'2011-06-24','143259000004')
INSERT INTO EntityMast (EntityCod,EntityNam,EntityTypCod,MainContactUsrSortNam,EntityEmailAddr,DemoEntityFlag,EntityIsPublisherFlag,EntityIsContentProviderFlag,EntityIsEditorFlag,EntityIsAffiliateFlag,EntityIsAdvertiserFlag,EntityIsFinancialServiceFlag,EntityIsCMPServiceFlag,EntityRegistrationDate,EntityInactiveDate,EntityRem,LastModUsrCod,LastModDate,LastModTim)VALUES(651,'Paul Getty',100,'getty, paul, 751','affiliate01@abc.com',1,0,0,0,1,0,0,0,'2009-04-23',NULL,NULL,0,'2011-06-24','143259000005')
INSERT INTO EntityMast (EntityCod,EntityNam,EntityTypCod,MainContactUsrSortNam,EntityEmailAddr,DemoEntityFlag,EntityIsPublisherFlag,EntityIsContentProviderFlag,EntityIsEditorFlag,EntityIsAffiliateFlag,EntityIsAdvertiserFlag,EntityIsFinancialServiceFlag,EntityIsCMPServiceFlag,EntityRegistrationDate,EntityInactiveDate,EntityRem,LastModUsrCod,LastModDate,LastModTim)VALUES(701,'Gilbert B. Hammer',100,'hammer, gilbert, 801','ghammer@abc.com',1,0,1,0,1,0,0,0,'2009-04-23',NULL,NULL,0,'2011-06-24','143259000006')
INSERT INTO EntityMast (EntityCod,EntityNam,EntityTypCod,MainContactUsrSortNam,EntityEmailAddr,DemoEntityFlag,EntityIsPublisherFlag,EntityIsContentProviderFlag,EntityIsEditorFlag,EntityIsAffiliateFlag,EntityIsAdvertiserFlag,EntityIsFinancialServiceFlag,EntityIsCMPServiceFlag,EntityRegistrationDate,EntityInactiveDate,EntityRem,LastModUsrCod,LastModDate,LastModTim)VALUES(801,'Anna Hazare',100,'hazare, anna, 901','financialservice01@abc.com',1,0,1,0,1,0,0,0,'2009-04-23',NULL,NULL,0,'2011-06-24','143259000007')
INSERT INTO EntityMast (EntityCod,EntityNam,EntityTypCod,MainContactUsrSortNam,EntityEmailAddr,DemoEntityFlag,EntityIsPublisherFlag,EntityIsContentProviderFlag,EntityIsEditorFlag,EntityIsAffiliateFlag,EntityIsAdvertiserFlag,EntityIsFinancialServiceFlag,EntityIsCMPServiceFlag,EntityRegistrationDate,EntityInactiveDate,EntityRem,LastModUsrCod,LastModDate,LastModTim)VALUES(802,'Prashant Bhushan',100,'bhushan, prashant, 902','financialservice02@abc.com',1,0,1,0,1,0,0,0,'2009-04-23',NULL,NULL,0,'2011-06-24','143259000008')

DECLARE @agc INT

select @agc = AccessControlGroupCode from b1.AccessControlGroups where 
AccessControlGroupName = 'Guests'
 
DECLARE @uc INT

EXEC B1.usp_UniqueIdsGetNextBlock 'UserCode', 1, @uc out

INSERT INTO B1.UserMaster
(
	UserCode
	, UserId
	, UserPassword
	, PasswordSalt
	, DefaultAccessGroupCode
	, SignonRestricted
	, MultipleSignonAllowed
	, ForcePasswordChange
	, FailedSignonAttempts
	, NamePrefix
	, FirstName
	, MiddleName
	, LastName
	, NameSuffix
	, Remarks
)
VALUES
(
@uc
, 'test01'
, '4ejxiLVxr/CVUsjPjbks3QGxlamYrtJGKvK2lEbSP6YT5g31B9FXnzv7DV41OnXPbuLk+aahdxCbAh0bqYfp/w=='
, 'fpi8580t7r2FpeouIiPOKRGGfcLeOyIeg+qX03MymqJm0SoDAeMGWOovESyyaPTLAyvU'
, @agc
, 0
, 1
, 0
, 0
, 'Mr'
, 'Test'
, null
, '01'
, null
, 'A CG Test User'
)

INSERT INTO MemberMast (UsrCod,UsrSortNam,MemberEmailAddr,MemberIsPublisherFlag,MemberIsContentProviderFlag,MemberIsEditorFlag,MemberIsAffiliateFlag,MemberIsAdvertiserFlag,MemberIsFinancialServiceFlag,MemberIsCMPServiceStaffFlag,MemberRegistrationDate,MemberLastLogonDate,MemberInactiveDate,ShoppingCartStatCod,ShoppingCartOrigContentURL,ShoppingCartReturnURL,MemberRem,LastModUsrCod,LastModDate,LastModTim)VALUES(@uc,'test user 1','test01@abc.com',0,1,0,1,0,0,0,'2009-04-23',NULL,NULL,100,NULL,NULL,NULL,0,'2011-06-24','143258000028')

EXEC B1.usp_UniqueIdsGetNextBlock 'UserCode', 1, @uc out

INSERT INTO B1.UserMaster
(
	UserCode
	, UserId
	, UserPassword
	, PasswordSalt
	, DefaultAccessGroupCode
	, SignonRestricted
	, MultipleSignonAllowed
	, ForcePasswordChange
	, FailedSignonAttempts
	, NamePrefix
	, FirstName
	, MiddleName
	, LastName
	, NameSuffix
	, Remarks
)
VALUES
(
@uc
, 'test02'
, '4ejxiLVxr/CVUsjPjbks3QGxlamYrtJGKvK2lEbSP6YT5g31B9FXnzv7DV41OnXPbuLk+aahdxCbAh0bqYfp/w=='
, 'fpi8580t7r2FpeouIiPOKRGGfcLeOyIeg+qX03MymqJm0SoDAeMGWOovESyyaPTLAyvU'
, @agc
, 0
, 1
, 0
, 0
, 'Mr'
, 'Test'
, null
, '02'
, null
, 'A CG Test User'
)

INSERT INTO MemberMast (UsrCod,UsrSortNam,MemberEmailAddr,MemberIsPublisherFlag,MemberIsContentProviderFlag,MemberIsEditorFlag,MemberIsAffiliateFlag,MemberIsAdvertiserFlag,MemberIsFinancialServiceFlag,MemberIsCMPServiceStaffFlag,MemberRegistrationDate,MemberLastLogonDate,MemberInactiveDate,ShoppingCartStatCod,ShoppingCartOrigContentURL,ShoppingCartReturnURL,MemberRem,LastModUsrCod,LastModDate,LastModTim)VALUES(@uc,'test user 2, 952','test02@abc.com',0,1,0,1,0,0,0,'2009-04-23',NULL,NULL,100,NULL,NULL,NULL,0,'2011-06-24','143258000029')

INSERT INTO LedgerAccountMast (LedgerAccountNum,LedgerAccountNam,DebitAccountFlag,LedgerAccountRem,LastModUsrCod,LastModDate,LastModTim)VALUES(1500,'Accounts receivable, subscription sales',1,NULL,0,'2011-06-24','143250000001')
INSERT INTO LedgerAccountMast (LedgerAccountNum,LedgerAccountNam,DebitAccountFlag,LedgerAccountRem,LastModUsrCod,LastModDate,LastModTim)VALUES(2200,'Accounts payable, publisher subscription sales',0,NULL,0,'2011-06-24','143250000002')
INSERT INTO LedgerAccountMast (LedgerAccountNum,LedgerAccountNam,DebitAccountFlag,LedgerAccountRem,LastModUsrCod,LastModDate,LastModTim)VALUES(2250,'Accounts payable, content provider paid usage',0,NULL,0,'2011-06-24','143250000003')
INSERT INTO LedgerAccountMast (LedgerAccountNum,LedgerAccountNam,DebitAccountFlag,LedgerAccountRem,LastModUsrCod,LastModDate,LastModTim)VALUES(2300,'Accounts payable, editor paid usage',0,NULL,0,'2011-06-24','143250000004')
INSERT INTO LedgerAccountMast (LedgerAccountNum,LedgerAccountNam,DebitAccountFlag,LedgerAccountRem,LastModUsrCod,LastModDate,LastModTim)VALUES(2350,'Accounts payable, affiliate subscription sales',0,NULL,0,'2011-06-24','143250000005')
INSERT INTO LedgerAccountMast (LedgerAccountNum,LedgerAccountNam,DebitAccountFlag,LedgerAccountRem,LastModUsrCod,LastModDate,LastModTim)VALUES(2370,'Accounts payable, affiliate paid usage',0,NULL,0,'2011-06-24','143250000006')
INSERT INTO LedgerAccountMast (LedgerAccountNum,LedgerAccountNam,DebitAccountFlag,LedgerAccountRem,LastModUsrCod,LastModDate,LastModTim)VALUES(4200,'Sales, subscription',0,NULL,0,'2011-06-24','143250000007')
INSERT INTO LedgerAccountMast (LedgerAccountNum,LedgerAccountNam,DebitAccountFlag,LedgerAccountRem,LastModUsrCod,LastModDate,LastModTim)VALUES(5400,'Expense, subscription payment processing',1,NULL,0,'2011-06-24','143250000008')
INSERT INTO LedgerAccountMast (LedgerAccountNum,LedgerAccountNam,DebitAccountFlag,LedgerAccountRem,LastModUsrCod,LastModDate,LastModTim)VALUES(5450,'Expense, paid usage payment processing',1,NULL,0,'2011-06-24','143250000009')

INSERT INTO PaymentTranTyp (PaymentTranTypCod,PaymentTranTypNam,LedgerAccountNum,DebitAccountFlag,DebitTranFlag,OkForMemberFlag,OkForPublisherFlag,OkForContentProviderFlag,OkForEditorFlag,OkForAffiliateFlag,OkForFinancialServiceFlag,PaymentTranTypRem,LastModUsrCod,LastModDate,LastModTim)VALUES(100,'Subscription sales from member',4200,0,0,1,0,0,0,0,0,'Cr sales, subscription (a Cr account)',0,'2011-06-24','143250000018')
INSERT INTO PaymentTranTyp (PaymentTranTypCod,PaymentTranTypNam,LedgerAccountNum,DebitAccountFlag,DebitTranFlag,OkForMemberFlag,OkForPublisherFlag,OkForContentProviderFlag,OkForEditorFlag,OkForAffiliateFlag,OkForFinancialServiceFlag,PaymentTranTypRem,LastModUsrCod,LastModDate,LastModTim)VALUES(110,'Publisher subscription sales',2200,0,0,0,1,0,0,0,0,'Cr accounts payable, publisher subscription sales (a Cr account)',0,'2011-06-24','143250000019')
INSERT INTO PaymentTranTyp (PaymentTranTypCod,PaymentTranTypNam,LedgerAccountNum,DebitAccountFlag,DebitTranFlag,OkForMemberFlag,OkForPublisherFlag,OkForContentProviderFlag,OkForEditorFlag,OkForAffiliateFlag,OkForFinancialServiceFlag,PaymentTranTypRem,LastModUsrCod,LastModDate,LastModTim)VALUES(120,'Affiliate subscription sales',2350,0,0,0,0,0,0,1,0,'Cr accounts payable, affiliate subscription sales (a Cr account)',0,'2011-06-24','143250000020')
INSERT INTO PaymentTranTyp (PaymentTranTypCod,PaymentTranTypNam,LedgerAccountNum,DebitAccountFlag,DebitTranFlag,OkForMemberFlag,OkForPublisherFlag,OkForContentProviderFlag,OkForEditorFlag,OkForAffiliateFlag,OkForFinancialServiceFlag,PaymentTranTypRem,LastModUsrCod,LastModDate,LastModTim)VALUES(130,'Subscription payment processing',5400,1,1,0,0,0,0,0,1,'Dr expense, subscription payment processing (a Dr account)',0,'2011-06-24','143250000021')
INSERT INTO PaymentTranTyp (PaymentTranTypCod,PaymentTranTypNam,LedgerAccountNum,DebitAccountFlag,DebitTranFlag,OkForMemberFlag,OkForPublisherFlag,OkForContentProviderFlag,OkForEditorFlag,OkForAffiliateFlag,OkForFinancialServiceFlag,PaymentTranTypRem,LastModUsrCod,LastModDate,LastModTim)VALUES(140,'Subscription sales from financial service',1500,1,1,0,0,0,0,0,1,'Dr accounts receivable, subscription sales (a Dr account)',0,'2011-06-24','143250000022')
INSERT INTO PaymentTranTyp (PaymentTranTypCod,PaymentTranTypNam,LedgerAccountNum,DebitAccountFlag,DebitTranFlag,OkForMemberFlag,OkForPublisherFlag,OkForContentProviderFlag,OkForEditorFlag,OkForAffiliateFlag,OkForFinancialServiceFlag,PaymentTranTypRem,LastModUsrCod,LastModDate,LastModTim)VALUES(300,'Content provider paid usage',2250,0,0,0,0,1,0,0,0,'Cr accounts payable, content provider paid usage (a Cr account)',0,'2011-06-24','143250000023')
INSERT INTO PaymentTranTyp (PaymentTranTypCod,PaymentTranTypNam,LedgerAccountNum,DebitAccountFlag,DebitTranFlag,OkForMemberFlag,OkForPublisherFlag,OkForContentProviderFlag,OkForEditorFlag,OkForAffiliateFlag,OkForFinancialServiceFlag,PaymentTranTypRem,LastModUsrCod,LastModDate,LastModTim)VALUES(310,'Paid usage to primary editor',2300,0,0,0,0,0,1,0,0,'Cr accounts payable, primary editor paid usage (a Cr account)',0,'2011-06-24','143250000024')
INSERT INTO PaymentTranTyp (PaymentTranTypCod,PaymentTranTypNam,LedgerAccountNum,DebitAccountFlag,DebitTranFlag,OkForMemberFlag,OkForPublisherFlag,OkForContentProviderFlag,OkForEditorFlag,OkForAffiliateFlag,OkForFinancialServiceFlag,PaymentTranTypRem,LastModUsrCod,LastModDate,LastModTim)VALUES(320,'Paid usage to senior editor',2300,0,0,0,0,0,1,0,0,'Cr accounts payable, senior editor paid usage (a Cr account)',0,'2011-06-24','143250000025')
INSERT INTO PaymentTranTyp (PaymentTranTypCod,PaymentTranTypNam,LedgerAccountNum,DebitAccountFlag,DebitTranFlag,OkForMemberFlag,OkForPublisherFlag,OkForContentProviderFlag,OkForEditorFlag,OkForAffiliateFlag,OkForFinancialServiceFlag,PaymentTranTypRem,LastModUsrCod,LastModDate,LastModTim)VALUES(330,'Paid usage to affiliate',2370,0,0,0,0,0,0,1,0,'Cr accounts payable, affiliate paid usage (a Cr account)',0,'2011-06-24','143250000026')
INSERT INTO PaymentTranTyp (PaymentTranTypCod,PaymentTranTypNam,LedgerAccountNum,DebitAccountFlag,DebitTranFlag,OkForMemberFlag,OkForPublisherFlag,OkForContentProviderFlag,OkForEditorFlag,OkForAffiliateFlag,OkForFinancialServiceFlag,PaymentTranTypRem,LastModUsrCod,LastModDate,LastModTim)VALUES(340,'Paid usage payment processing',5450,1,1,0,0,0,0,0,1,'Dr expense, paid usage payment processing (a Dr account)',0,'2011-06-24','143250000027')


INSERT INTO PaymentMethod (PaymentMethodCod,PaymentMethodNam,OkForMemberFlag,PaymentMethodRem,LastModUsrCod,LastModDate,LastModTim)VALUES(110,'ACH payment from CMP service',0,NULL,0,'2011-06-24','143250000028')
INSERT INTO PaymentMethod (PaymentMethodCod,PaymentMethodNam,OkForMemberFlag,PaymentMethodRem,LastModUsrCod,LastModDate,LastModTim)VALUES(111,'ACH payment to CMP service',0,NULL,0,'2011-06-24','143250000029')
INSERT INTO PaymentMethod (PaymentMethodCod,PaymentMethodNam,OkForMemberFlag,PaymentMethodRem,LastModUsrCod,LastModDate,LastModTim)VALUES(150,'Amazon payments',0,NULL,0,'2011-06-24','143250000030')
INSERT INTO PaymentMethod (PaymentMethodCod,PaymentMethodNam,OkForMemberFlag,PaymentMethodRem,LastModUsrCod,LastModDate,LastModTim)VALUES(180,'American Express credit card',1,NULL,0,'2011-06-24','143250000031')
INSERT INTO PaymentMethod (PaymentMethodCod,PaymentMethodNam,OkForMemberFlag,PaymentMethodRem,LastModUsrCod,LastModDate,LastModTim)VALUES(300,'Check from CMP service',0,NULL,0,'2011-06-24','143250000032')
INSERT INTO PaymentMethod (PaymentMethodCod,PaymentMethodNam,OkForMemberFlag,PaymentMethodRem,LastModUsrCod,LastModDate,LastModTim)VALUES(301,'Check to CMP service',0,NULL,0,'2011-06-24','143250000033')
INSERT INTO PaymentMethod (PaymentMethodCod,PaymentMethodNam,OkForMemberFlag,PaymentMethodRem,LastModUsrCod,LastModDate,LastModTim)VALUES(331,'Diners Club credit card',1,NULL,0,'2011-06-24','143250000034')
INSERT INTO PaymentMethod (PaymentMethodCod,PaymentMethodNam,OkForMemberFlag,PaymentMethodRem,LastModUsrCod,LastModDate,LastModTim)VALUES(341,'Discover credit card',1,NULL,0,'2011-06-24','143250000035')
INSERT INTO PaymentMethod (PaymentMethodCod,PaymentMethodNam,OkForMemberFlag,PaymentMethodRem,LastModUsrCod,LastModDate,LastModTim)VALUES(400,'Mastercard credit card',1,NULL,0,'2011-06-24','143250000036')
INSERT INTO PaymentMethod (PaymentMethodCod,PaymentMethodNam,OkForMemberFlag,PaymentMethodRem,LastModUsrCod,LastModDate,LastModTim)VALUES(700,'Paypal',0,NULL,0,'2011-06-24','143250000037')
INSERT INTO PaymentMethod (PaymentMethodCod,PaymentMethodNam,OkForMemberFlag,PaymentMethodRem,LastModUsrCod,LastModDate,LastModTim)VALUES(900,'Visa credit card',1,NULL,0,'2011-06-24','143250000038')

INSERT INTO FinancialServiceMast (FinancialServiceEntityCod,FinancialServiceNam,FinancialServiceContactUsrSortNam,FinancialServiceEmailAddr,FinancialServicePausedFlag,FinancialServiceStatCod,FinancialServiceInactiveDate,LastModUsrCod,LastModDate,LastModTim)VALUES(801,'Sample Payment Processor Inc.',NULL,NULL,0,300,NULL,0,'2011-06-24','143259000031')
INSERT INTO FinancialServiceMast (FinancialServiceEntityCod,FinancialServiceNam,FinancialServiceContactUsrSortNam,FinancialServiceEmailAddr,FinancialServicePausedFlag,FinancialServiceStatCod,FinancialServiceInactiveDate,LastModUsrCod,LastModDate,LastModTim)VALUES(802,'Global Payments',NULL,NULL,0,300,NULL,0,'2011-06-24','143259000032')

INSERT INTO PublisherMast (PublisherEntityCod,PublisherNam,PublisherContactUsrSortNam,PublisherEmailAddr,PublisherPausedFlag,PublisherStatCod,PublisherInactiveDate,LastModUsrCod,LastModDate,LastModTim)VALUES(1001,'Content Galaxy Inc.','asherman, steven, 1001','info@contentgalaxy.com',0,300,NULL,0,'2011-06-24','143250000073')
INSERT INTO PublisherMast (PublisherEntityCod,PublisherNam,PublisherContactUsrSortNam,PublisherEmailAddr,PublisherPausedFlag,PublisherStatCod,PublisherInactiveDate,LastModUsrCod,LastModDate,LastModTim)VALUES(501,'Sample Publisher Inc.','miller, barney, 601','publisher01@abc.com',0,300,NULL,0,'2011-06-24','143259000009')

INSERT INTO PublicationMast (PublicationCod,PublicationNam,PublicationShortNam,PublisherEntityCod,PublicationTagLin,PublicationImageFilNam,PublicationHomeCMPServiceURL,PublicationStreamOkFlag,PublicationDownloadOkFlag,RepeatDownloadClickDurationDayCt,PublicationAffiliateOkFlag,PurchasePercentShareCMPService,PurchasePercentSharePool,PurchasePercentSharePublisher,PoolPercentShareContentProvider,PoolPercentSharePrimaryEditor,PoolPercentShareSeniorEditor,PublicationContactUsrSortNam,PublicationEmailAddr,PublicationDescription,PublicationPausedFlag,PublicationStatCod,PublicationInactiveDate,LastModUsrCod,LastModDate,LastModTim)VALUES(1001,'Content Galaxy Software Library','Content Galaxy Software',1001,'Developer tools and desktop applications','GreatCloud134_99.jpg',NULL,0,1,NULL,1,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,0,300,NULL,0,'2011-06-24','143250000074')
INSERT INTO PublicationMast (PublicationCod,PublicationNam,PublicationShortNam,PublisherEntityCod,PublicationTagLin,PublicationImageFilNam,PublicationHomeCMPServiceURL,PublicationStreamOkFlag,PublicationDownloadOkFlag,RepeatDownloadClickDurationDayCt,PublicationAffiliateOkFlag,PurchasePercentShareCMPService,PurchasePercentSharePool,PurchasePercentSharePublisher,PoolPercentShareContentProvider,PoolPercentSharePrimaryEditor,PoolPercentShareSeniorEditor,PublicationContactUsrSortNam,PublicationEmailAddr,PublicationDescription,PublicationPausedFlag,PublicationStatCod,PublicationInactiveDate,LastModUsrCod,LastModDate,LastModTim)VALUES(1002,'Content Galaxy Sports and Fitness Videos','CG Sports and Fitness Video',1001,'Fitness videos','GreatCloud134_99.jpg',NULL,1,0,NULL,1,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,0,300,NULL,0,'2011-06-24','143250000075')
INSERT INTO PublicationMast (PublicationCod,PublicationNam,PublicationShortNam,PublisherEntityCod,PublicationTagLin,PublicationImageFilNam,PublicationHomeCMPServiceURL,PublicationStreamOkFlag,PublicationDownloadOkFlag,RepeatDownloadClickDurationDayCt,PublicationAffiliateOkFlag,PurchasePercentShareCMPService,PurchasePercentSharePool,PurchasePercentSharePublisher,PoolPercentShareContentProvider,PoolPercentSharePrimaryEditor,PoolPercentShareSeniorEditor,PublicationContactUsrSortNam,PublicationEmailAddr,PublicationDescription,PublicationPausedFlag,PublicationStatCod,PublicationInactiveDate,LastModUsrCod,LastModDate,LastModTim)VALUES(1003,'Content Galaxy Sports and Fitness Articles','CG Sports and Fitness Articles',1001,'Fitness Articles','GreatCloud134_99.jpg',NULL,0,1,NULL,1,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,0,300,NULL,0,'2011-06-24','143250000076')
INSERT INTO PublicationMast (PublicationCod,PublicationNam,PublicationShortNam,PublisherEntityCod,PublicationTagLin,PublicationImageFilNam,PublicationHomeCMPServiceURL,PublicationStreamOkFlag,PublicationDownloadOkFlag,RepeatDownloadClickDurationDayCt,PublicationAffiliateOkFlag,PurchasePercentShareCMPService,PurchasePercentSharePool,PurchasePercentSharePublisher,PoolPercentShareContentProvider,PoolPercentSharePrimaryEditor,PoolPercentShareSeniorEditor,PublicationContactUsrSortNam,PublicationEmailAddr,PublicationDescription,PublicationPausedFlag,PublicationStatCod,PublicationInactiveDate,LastModUsrCod,LastModDate,LastModTim)VALUES(1004,'Content Galaxy Martial Arts Videos','CG Martial Arts Videos',1001,'Workouts for martial arts','GreatCloud134_99.jpg',NULL,1,0,NULL,1,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,0,300,NULL,0,'2011-06-24','143250000077')
INSERT INTO PublicationMast (PublicationCod,PublicationNam,PublicationShortNam,PublisherEntityCod,PublicationTagLin,PublicationImageFilNam,PublicationHomeCMPServiceURL,PublicationStreamOkFlag,PublicationDownloadOkFlag,RepeatDownloadClickDurationDayCt,PublicationAffiliateOkFlag,PurchasePercentShareCMPService,PurchasePercentSharePool,PurchasePercentSharePublisher,PoolPercentShareContentProvider,PoolPercentSharePrimaryEditor,PoolPercentShareSeniorEditor,PublicationContactUsrSortNam,PublicationEmailAddr,PublicationDescription,PublicationPausedFlag,PublicationStatCod,PublicationInactiveDate,LastModUsrCod,LastModDate,LastModTim)VALUES(1005,'Content Galaxy Martial Arts Articles','CG Martial Arts Articles',1001,'Articles on martial arts','GreatCloud134_99.jpg',NULL,0,1,NULL,1,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,0,300,NULL,0,'2011-06-24','143250000078')
INSERT INTO PublicationMast (PublicationCod,PublicationNam,PublicationShortNam,PublisherEntityCod,PublicationTagLin,PublicationImageFilNam,PublicationHomeCMPServiceURL,PublicationStreamOkFlag,PublicationDownloadOkFlag,RepeatDownloadClickDurationDayCt,PublicationAffiliateOkFlag,PurchasePercentShareCMPService,PurchasePercentSharePool,PurchasePercentSharePublisher,PoolPercentShareContentProvider,PoolPercentSharePrimaryEditor,PoolPercentShareSeniorEditor,PublicationContactUsrSortNam,PublicationEmailAddr,PublicationDescription,PublicationPausedFlag,PublicationStatCod,PublicationInactiveDate,LastModUsrCod,LastModDate,LastModTim)VALUES(501,'Sample Music Video Publication','Sample Music Videos',501,'Music videos of all sorts','GreatCloud134_99.jpg',NULL,1,0,NULL,1,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,0,300,NULL,0,'2011-06-24','143259000010')
INSERT INTO PublicationMast (PublicationCod,PublicationNam,PublicationShortNam,PublisherEntityCod,PublicationTagLin,PublicationImageFilNam,PublicationHomeCMPServiceURL,PublicationStreamOkFlag,PublicationDownloadOkFlag,RepeatDownloadClickDurationDayCt,PublicationAffiliateOkFlag,PurchasePercentShareCMPService,PurchasePercentSharePool,PurchasePercentSharePublisher,PoolPercentShareContentProvider,PoolPercentSharePrimaryEditor,PoolPercentShareSeniorEditor,PublicationContactUsrSortNam,PublicationEmailAddr,PublicationDescription,PublicationPausedFlag,PublicationStatCod,PublicationInactiveDate,LastModUsrCod,LastModDate,LastModTim)VALUES(502,'Sample Physical Therapy Video Publication','Sample Physical Therapy Videos',501,'Exercise videos for speeding recovery','GreatCloud134_99.jpg',NULL,1,0,NULL,1,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,0,300,NULL,0,'2011-06-24','143259000011')



INSERT INTO SubscriptionPeriodTyp (SubscriptionPeriodTypCod,SubscriptionPeriodTypNam,SubscriptionPeriodTypRem,LastModUsrCod,LastModDate,LastModTim)VALUES(300,'Monthly',NULL,0,'2011-06-24','143250000039')
INSERT INTO SubscriptionPeriodTyp (SubscriptionPeriodTypCod,SubscriptionPeriodTypNam,SubscriptionPeriodTypRem,LastModUsrCod,LastModDate,LastModTim)VALUES(500,'Quarterly',NULL,0,'2011-06-24','143250000040')
INSERT INTO SubscriptionPeriodTyp (SubscriptionPeriodTypCod,SubscriptionPeriodTypNam,SubscriptionPeriodTypRem,LastModUsrCod,LastModDate,LastModTim)VALUES(700,'Semi-annual',NULL,0,'2011-06-24','143250000041')
INSERT INTO SubscriptionPeriodTyp (SubscriptionPeriodTypCod,SubscriptionPeriodTypNam,SubscriptionPeriodTypRem,LastModUsrCod,LastModDate,LastModTim)VALUES(900,'Annual',NULL,0,'2011-06-24','143250000042')

INSERT INTO SubscriptionOffer (PublicationCod,SubscriptionOfferCod,SubscriptionOfferDescription,SubscriptionOfferActiveFlag,SubscriptionLstPrice,RecurringBillingFlag,SalesTaxFlag,SubscriptionPeriodTypCod,SubscriptionStreamOkFlag,SubscriptionDownloadOkFlag,MaxStreamDurationSeconds,MaxDownloadDownloadByteCt,MaxDownloadCt,DiscountPriceThisPublisher,DiscountPriceCrossSellPublisher,SubscriptionOfferRem,LastModUsrCod,LastModDate,LastModTim)VALUES(1001,1,'Six month subscription',1,29.9500,0,0,700,0,1,NULL,NULL,NULL,24.9500,24.9500,NULL,0,'2011-06-24','143250000083')
INSERT INTO SubscriptionOffer (PublicationCod,SubscriptionOfferCod,SubscriptionOfferDescription,SubscriptionOfferActiveFlag,SubscriptionLstPrice,RecurringBillingFlag,SalesTaxFlag,SubscriptionPeriodTypCod,SubscriptionStreamOkFlag,SubscriptionDownloadOkFlag,MaxStreamDurationSeconds,MaxDownloadDownloadByteCt,MaxDownloadCt,DiscountPriceThisPublisher,DiscountPriceCrossSellPublisher,SubscriptionOfferRem,LastModUsrCod,LastModDate,LastModTim)VALUES(1001,2,'One year subscription',1,49.9500,0,0,900,0,1,NULL,NULL,NULL,44.9500,44.9500,NULL,0,'2011-06-24','143250000084')
INSERT INTO SubscriptionOffer (PublicationCod,SubscriptionOfferCod,SubscriptionOfferDescription,SubscriptionOfferActiveFlag,SubscriptionLstPrice,RecurringBillingFlag,SalesTaxFlag,SubscriptionPeriodTypCod,SubscriptionStreamOkFlag,SubscriptionDownloadOkFlag,MaxStreamDurationSeconds,MaxDownloadDownloadByteCt,MaxDownloadCt,DiscountPriceThisPublisher,DiscountPriceCrossSellPublisher,SubscriptionOfferRem,LastModUsrCod,LastModDate,LastModTim)VALUES(1002,1,'Six month subscription',1,19.9500,0,0,700,0,1,NULL,NULL,NULL,14.9500,14.9500,NULL,0,'2011-06-24','143250000085')
INSERT INTO SubscriptionOffer (PublicationCod,SubscriptionOfferCod,SubscriptionOfferDescription,SubscriptionOfferActiveFlag,SubscriptionLstPrice,RecurringBillingFlag,SalesTaxFlag,SubscriptionPeriodTypCod,SubscriptionStreamOkFlag,SubscriptionDownloadOkFlag,MaxStreamDurationSeconds,MaxDownloadDownloadByteCt,MaxDownloadCt,DiscountPriceThisPublisher,DiscountPriceCrossSellPublisher,SubscriptionOfferRem,LastModUsrCod,LastModDate,LastModTim)VALUES(1002,2,'One year subscription',1,29.9500,0,0,900,0,1,NULL,NULL,NULL,24.9500,24.9500,NULL,0,'2011-06-24','143250000086')
INSERT INTO SubscriptionOffer (PublicationCod,SubscriptionOfferCod,SubscriptionOfferDescription,SubscriptionOfferActiveFlag,SubscriptionLstPrice,RecurringBillingFlag,SalesTaxFlag,SubscriptionPeriodTypCod,SubscriptionStreamOkFlag,SubscriptionDownloadOkFlag,MaxStreamDurationSeconds,MaxDownloadDownloadByteCt,MaxDownloadCt,DiscountPriceThisPublisher,DiscountPriceCrossSellPublisher,SubscriptionOfferRem,LastModUsrCod,LastModDate,LastModTim)VALUES(1003,1,'Six month subscription',1,19.9500,0,0,700,0,1,NULL,NULL,NULL,14.9500,14.9500,NULL,0,'2011-06-24','143250000087')
INSERT INTO SubscriptionOffer (PublicationCod,SubscriptionOfferCod,SubscriptionOfferDescription,SubscriptionOfferActiveFlag,SubscriptionLstPrice,RecurringBillingFlag,SalesTaxFlag,SubscriptionPeriodTypCod,SubscriptionStreamOkFlag,SubscriptionDownloadOkFlag,MaxStreamDurationSeconds,MaxDownloadDownloadByteCt,MaxDownloadCt,DiscountPriceThisPublisher,DiscountPriceCrossSellPublisher,SubscriptionOfferRem,LastModUsrCod,LastModDate,LastModTim)VALUES(1003,2,'One year subscription',1,29.9500,0,0,900,0,1,NULL,NULL,NULL,24.9500,24.9500,NULL,0,'2011-06-24','143250000088')
INSERT INTO SubscriptionOffer (PublicationCod,SubscriptionOfferCod,SubscriptionOfferDescription,SubscriptionOfferActiveFlag,SubscriptionLstPrice,RecurringBillingFlag,SalesTaxFlag,SubscriptionPeriodTypCod,SubscriptionStreamOkFlag,SubscriptionDownloadOkFlag,MaxStreamDurationSeconds,MaxDownloadDownloadByteCt,MaxDownloadCt,DiscountPriceThisPublisher,DiscountPriceCrossSellPublisher,SubscriptionOfferRem,LastModUsrCod,LastModDate,LastModTim)VALUES(1004,1,'Six month subscription',1,19.9500,0,0,700,1,0,NULL,NULL,NULL,14.9500,14.9500,NULL,0,'2011-06-24','143250000089')
INSERT INTO SubscriptionOffer (PublicationCod,SubscriptionOfferCod,SubscriptionOfferDescription,SubscriptionOfferActiveFlag,SubscriptionLstPrice,RecurringBillingFlag,SalesTaxFlag,SubscriptionPeriodTypCod,SubscriptionStreamOkFlag,SubscriptionDownloadOkFlag,MaxStreamDurationSeconds,MaxDownloadDownloadByteCt,MaxDownloadCt,DiscountPriceThisPublisher,DiscountPriceCrossSellPublisher,SubscriptionOfferRem,LastModUsrCod,LastModDate,LastModTim)VALUES(1004,2,'One year subscription',1,29.9500,0,0,900,1,0,NULL,NULL,NULL,24.9500,24.9500,NULL,0,'2011-06-24','143250000090')
INSERT INTO SubscriptionOffer (PublicationCod,SubscriptionOfferCod,SubscriptionOfferDescription,SubscriptionOfferActiveFlag,SubscriptionLstPrice,RecurringBillingFlag,SalesTaxFlag,SubscriptionPeriodTypCod,SubscriptionStreamOkFlag,SubscriptionDownloadOkFlag,MaxStreamDurationSeconds,MaxDownloadDownloadByteCt,MaxDownloadCt,DiscountPriceThisPublisher,DiscountPriceCrossSellPublisher,SubscriptionOfferRem,LastModUsrCod,LastModDate,LastModTim)VALUES(1005,1,'Six month subscription',1,19.9500,0,0,700,0,1,NULL,NULL,NULL,14.9500,14.9500,NULL,0,'2011-06-24','143250000091')
INSERT INTO SubscriptionOffer (PublicationCod,SubscriptionOfferCod,SubscriptionOfferDescription,SubscriptionOfferActiveFlag,SubscriptionLstPrice,RecurringBillingFlag,SalesTaxFlag,SubscriptionPeriodTypCod,SubscriptionStreamOkFlag,SubscriptionDownloadOkFlag,MaxStreamDurationSeconds,MaxDownloadDownloadByteCt,MaxDownloadCt,DiscountPriceThisPublisher,DiscountPriceCrossSellPublisher,SubscriptionOfferRem,LastModUsrCod,LastModDate,LastModTim)VALUES(1005,2,'One year subscription',1,29.9500,0,0,900,0,1,NULL,NULL,NULL,24.9500,24.9500,NULL,0,'2011-06-24','143250000092')
INSERT INTO SubscriptionOffer (PublicationCod,SubscriptionOfferCod,SubscriptionOfferDescription,SubscriptionOfferActiveFlag,SubscriptionLstPrice,RecurringBillingFlag,SalesTaxFlag,SubscriptionPeriodTypCod,SubscriptionStreamOkFlag,SubscriptionDownloadOkFlag,MaxStreamDurationSeconds,MaxDownloadDownloadByteCt,MaxDownloadCt,DiscountPriceThisPublisher,DiscountPriceCrossSellPublisher,SubscriptionOfferRem,LastModUsrCod,LastModDate,LastModTim)VALUES(501,1,'Six month subscription',1,19.9500,0,0,700,1,0,NULL,NULL,NULL,14.9500,14.9500,NULL,0,'2011-06-24','143259000024')
INSERT INTO SubscriptionOffer (PublicationCod,SubscriptionOfferCod,SubscriptionOfferDescription,SubscriptionOfferActiveFlag,SubscriptionLstPrice,RecurringBillingFlag,SalesTaxFlag,SubscriptionPeriodTypCod,SubscriptionStreamOkFlag,SubscriptionDownloadOkFlag,MaxStreamDurationSeconds,MaxDownloadDownloadByteCt,MaxDownloadCt,DiscountPriceThisPublisher,DiscountPriceCrossSellPublisher,SubscriptionOfferRem,LastModUsrCod,LastModDate,LastModTim)VALUES(501,2,'One year subscription',1,29.9500,0,0,900,1,0,NULL,NULL,NULL,24.9500,24.9500,NULL,0,'2011-06-24','143259000025')
INSERT INTO SubscriptionOffer (PublicationCod,SubscriptionOfferCod,SubscriptionOfferDescription,SubscriptionOfferActiveFlag,SubscriptionLstPrice,RecurringBillingFlag,SalesTaxFlag,SubscriptionPeriodTypCod,SubscriptionStreamOkFlag,SubscriptionDownloadOkFlag,MaxStreamDurationSeconds,MaxDownloadDownloadByteCt,MaxDownloadCt,DiscountPriceThisPublisher,DiscountPriceCrossSellPublisher,SubscriptionOfferRem,LastModUsrCod,LastModDate,LastModTim)VALUES(502,1,'Six month',1,29.9500,0,0,700,1,0,NULL,NULL,NULL,24.9500,24.9500,NULL,0,'2011-06-24','143259000026')
INSERT INTO SubscriptionOffer (PublicationCod,SubscriptionOfferCod,SubscriptionOfferDescription,SubscriptionOfferActiveFlag,SubscriptionLstPrice,RecurringBillingFlag,SalesTaxFlag,SubscriptionPeriodTypCod,SubscriptionStreamOkFlag,SubscriptionDownloadOkFlag,MaxStreamDurationSeconds,MaxDownloadDownloadByteCt,MaxDownloadCt,DiscountPriceThisPublisher,DiscountPriceCrossSellPublisher,SubscriptionOfferRem,LastModUsrCod,LastModDate,LastModTim)VALUES(502,2,'One year subscription',1,49.9500,0,0,700,1,0,NULL,NULL,NULL,39.9500,39.9500,NULL,0,'2011-06-24','143259000027')