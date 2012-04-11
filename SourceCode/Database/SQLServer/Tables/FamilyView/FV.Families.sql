--
-- must enter with DATEOfBirth, gender etc
-- The SystemTagId in this table refers to the relationship between the
-- person starting the view and their family. (e.g. Father, Husband, 
--
CREATE TABLE FV.Families
(
	FamilyId			BIGINT NOT NULL
	, Title				NVARCHAR(64) NOT NULL
	, StartedByUserCode INT NOT NULL
	, StartedOn			DATETIME NOT NULL DEFAULT(GETUTCDATE())
	, UpdatedByUserCode	INT NOT NULL
	, UpdatedOn			DATETIME
	, EndedOn			DATETIME -- can only be ended by StartedBy person && if all members agree ??
	CONSTRAINT Families_PK PRIMARY KEY (FamilyId) 
) ON FamilyViewData

GO

CREATE UNIQUE INDEX Families_UX_Title_FamilyId
ON FV.Families(Title, FamilyId)
ON FamilyViewIdx

GO

ALTER TABLE FV.Families
ADD CONSTRAINT Families_FK_UserMaster_StartedByUserCode
FOREIGN KEY (StartedByUserCode)
REFERENCES B1.UserMaster(UserCode)

GO

ALTER TABLE FV.Families
ADD CONSTRAINT Families_FK_UserMaster_UpdatedByUserCode
FOREIGN KEY (UpdatedByUserCode)
REFERENCES B1.UserMaster(UserCode)

GO