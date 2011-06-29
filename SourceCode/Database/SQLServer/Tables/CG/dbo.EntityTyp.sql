

CREATE TABLE dbo.EntityTyp(
	EntityTypCod smallint NOT NULL,
	EntityTypNam varchar(40) NOT NULL,
	EntityTypRem varchar(500) NULL,
	LastModUsrCod int NOT NULL,
	LastModDate date NOT NULL,
	LastModTim char(12) NOT NULL,
	LastModifiedUserCode	INT,	-- Last User to Modify the record
	LastModifiedDateTime	DATETIME, -- Date/Time of the last modification
 CONSTRAINT EntityTyp_PK PRIMARY KEY NONCLUSTERED (EntityTypCod)
) ON [PRIMARY]
GO

CREATE UNIQUE NONCLUSTERED INDEX [EntityTyp00] ON [dbo].[EntityTyp] 
(
	[LastModUsrCod] ASC,
	[LastModDate] ASC,
	[LastModTim] ASC
) ON ContentGalaxyIdx
GO

CREATE UNIQUE NONCLUSTERED INDEX [EntityTyp02] ON [dbo].[EntityTyp] 
(
	[EntityTypNam] ASC
) ON ContentGalaxyIdx
GO

ALTER TABLE dbo.EntityTyp
ADD CONSTRAINT FK_EntityTyp_UserMaster
FOREIGN KEY (LastModifiedUserCode)
REFERENCES B1.UserMaster(UserCode)
GO

