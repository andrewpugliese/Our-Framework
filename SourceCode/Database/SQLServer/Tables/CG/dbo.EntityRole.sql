CREATE TABLE dbo.EntityRole(
	EntityRoleCod smallint NOT NULL,
	EntityRoleNam char(20) NOT NULL,
	EntityRoleRem varchar(100) NULL,
	LastModUsrCod int NOT NULL,
	LastModDate date NOT NULL,
	LastModTim char(12) NOT NULL,
	LastModifiedUserCode	INT,	-- Last User to Modify the record
	LastModifiedDateTime	DATETIME, -- Date/Time of the last modification
 CONSTRAINT EntityRole_PK PRIMARY KEY NONCLUSTERED (EntityRoleCod)
) ON [PRIMARY]

GO

CREATE UNIQUE NONCLUSTERED INDEX EntityRole00 ON dbo.EntityRole 
(
	LastModUsrCod ASC,
	LastModDate ASC,
	LastModTim ASC
) ON ContentGalaxyIdx
GO

CREATE UNIQUE NONCLUSTERED INDEX EntityRole02 ON dbo.EntityRole 
(
	EntityRoleNam ASC
) ON ContentGalaxyIdx
GO

ALTER TABLE dbo.EntityRole
ADD CONSTRAINT FK_EntityRole_UserMaster
FOREIGN KEY (LastModifiedUserCode)
REFERENCES B1.UserMaster(UserCode)
GO