
CREATE TABLE FV.ContentTagging
(
	ViewBookId		BIGINT NOT NULL 
	, ContentId		BIGINT NOT NULL 
	, Tag			BIGINT NOT NULL 
	, TaggedBy		BIGINT NOT NULL
	, TaggedOn		DATETIME NOT NULL
	CONSTRAINT ContentTagging_PK PRIMARY KEY (ViewBookId, ContentId, Tag, TaggedBy) 
) ON FamilyViewData
GO

CREATE UNIQUE INDEX ContentTagging_UX_ViewBookId_TaggedBy_Tag_ContentId
ON FV.ContentTagging(ViewBookId, TaggedBy, Tag, ContentId)
ON FamilyViewIdx
GO

ALTER TABLE FV.ContentTagging
ADD CONSTRAINT ContentTagging_FK_ContentIds
FOREIGN KEY (ContentId)
REFERENCES FV.ContentIds(ContentId)
GO

ALTER TABLE FV.ContentTagging
ADD CONSTRAINT ContentTagging_FK_ViewBooks
FOREIGN KEY (ViewBookId)
REFERENCES FV.ViewBooks(ViewBookId)
GO

ALTER TABLE FV.ContentTagging
ADD CONSTRAINT ContentTagging_FK_People
FOREIGN KEY (TaggedBy)
REFERENCES FV.People(PersonId)

GO

ALTER TABLE FV.ContentTagging
ADD CONSTRAINT ContentTagging_FK_ContentTags
FOREIGN KEY (Tag)
REFERENCES FV.ContentTags(TagId)

GO
