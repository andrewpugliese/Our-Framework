
CREATE TABLE dbo.PaymentMethod(
	PaymentMethodCod smallint NOT NULL,
	PaymentMethodNam varchar(30) NOT NULL,
	OkForMemberFlag smallint NOT NULL,
	PaymentMethodRem varchar(300) NULL,
	LastModUsrCod int NOT NULL,
	LastModDate date NOT NULL,
	LastModTim char(12) NOT NULL,
	LastModifiedUserCode	INT,	-- Last User to Modify the record
	LastModifiedDateTime	DATETIME, -- Date/Time of the last modification
 CONSTRAINT PaymentMethod_PK PRIMARY KEY NONCLUSTERED (PaymentMethodCod)
)ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX PaymentMethod00 ON dbo.PaymentMethod 
(
	LastModUsrCod ASC,
	LastModDate ASC,
	LastModTim ASC
) ON ContentGalaxyIdx
GO
CREATE UNIQUE NONCLUSTERED INDEX PaymentMethod02 ON dbo.PaymentMethod 
(
	PaymentMethodNam ASC
) ON ContentGalaxyIdx
GO
ALTER TABLE dbo.PaymentMethod
ADD CONSTRAINT FK_PaymentMethod_UserMaster
FOREIGN KEY (LastModifiedUserCode)
REFERENCES B1.UserMaster(UserCode)
GO