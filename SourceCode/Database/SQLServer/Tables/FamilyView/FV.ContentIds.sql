--
-- Contains all the unique identifiers that will be used by a family
-- across ALL their content.  All content child tables will have FK to this
-- table for the Id.
-- NOTE:
-- This table does NOT contain ids for people
--
CREATE TABLE FV.ContentIds
(
	ViewBookId				BIGINT NOT NULL -- Clustering key 
	, ContentId				BIGINT NOT NULL -- unique id
	, ContentTypeCode		INT NOT NULL -- classification
	, AddedByUserCode		INT NOT NULL
	, CONSTRAINT ContentIds_PK PRIMARY KEY NONCLUSTERED (ContentId) 
) ON FamilyViewData

GO


CREATE UNIQUE CLUSTERED INDEX ContentIds_UX_ViewBookId_ContentTypeCode_ContentId
ON FV.ContentIds(ViewBookId, ContentTypeCode, ContentId)
ON FamilyViewIdx

GO

ALTER TABLE FV.ContentIds
ADD CONSTRAINT ContentIds_FK_ViewBooks
FOREIGN KEY (ViewBookId)
REFERENCES FV.ViewBooks(ViewBookId)

GO

ALTER TABLE FV.ContentIds
ADD CONSTRAINT ContentIds_FK_ContentTypes
FOREIGN KEY (ContentTypeCode)
REFERENCES FV.ContentTypes(ContentTypeCode)

GO


ALTER TABLE FV.ContentIds
ADD CONSTRAINT ContentIds_FK_UserMaster
FOREIGN KEY (AddedByUserCode)
REFERENCES B1.UserMaster(UserCode)

GO