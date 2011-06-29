
CREATE TABLE dbo.ShoppingCartStat(
	ShoppingCartStatCod smallint NOT NULL,
	ShoppingCartStatNam varchar(27) NOT NULL,
	ShoppingCartStatRem varchar(300) NULL,
	LastModUsrCod int NOT NULL,
	LastModDate date NOT NULL,
	LastModTim char(12) NOT NULL,
	LastModifiedUserCode	INT,	-- Last User to Modify the record
	LastModifiedDateTime	DATETIME, -- Date/Time of the last modification
 CONSTRAINT ShoppingCartStat_PK PRIMARY KEY NONCLUSTERED (ShoppingCartStatCod)
) ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX ShoppingCartStat00 ON dbo.ShoppingCartStat 
(
	LastModUsrCod ASC,
	LastModDate ASC,
	LastModTim ASC
) ON ContentGalaxyIdx
GO
CREATE UNIQUE NONCLUSTERED INDEX ShoppingCartStat02 ON dbo.ShoppingCartStat 
(
	ShoppingCartStatNam ASC
) ON ContentGalaxyIdx
GO
ALTER TABLE dbo.ShoppingCartStat
ADD CONSTRAINT FK_ShoppingCartStat_UserMaster
FOREIGN KEY (LastModifiedUserCode)
REFERENCES B1.UserMaster(UserCode)
GO
