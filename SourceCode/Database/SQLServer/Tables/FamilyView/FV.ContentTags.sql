--
-- Contains system defined tags to classify content
-- Father, Mother, Daughter, Son, Husband, Wife, Spouse
-- Plumber, Doctor, Teacher, etc.
-- As well as Family Defined Tags
-- NOTE:
-- TagId NOT derived from ContentIds
--
CREATE TABLE FV.ContentTags
(
	TagId			BIGINT NOT NULL
	, Tag			NVARCHAR(64) NOT NULL
	, ViewBookId	BIGINT
	, TagOfUserCode INT 
	, Description	NVARCHAR(128)
	CONSTRAINT ContentTags_PK PRIMARY KEY (TagId) 
) ON FamilyViewData

GO

CREATE UNIQUE INDEX ContentTags_UX_ViewBookId_TagId
ON FV.ContentTags(ViewBookId, TagId)
ON FamilyViewIdx

GO

CREATE UNIQUE INDEX ContentTags_UX_Tag_ViewBookId
ON FV.ContentTags(Tag, ViewBookId)
ON FamilyViewIdx

GO

CREATE INDEX ContentTags_IX_ViewBookId_TagOfUserCode
ON FV.ContentTags(ViewBookId, TagOfUserCode)
ON FamilyViewIdx

GO

ALTER TABLE FV.ContentTags
ADD CONSTRAINT ContentTags_FK_ViewBooks
FOREIGN KEY (ViewBookId)
REFERENCES FV.ViewBooks(ViewBookId)

GO

ALTER TABLE FV.ContentTags
ADD CONSTRAINT ContentTags_FK_UserMaster
FOREIGN KEY (TagOfUserCode)
REFERENCES B1.UserMaster(UserCode)

GO
