CREATE TABLE FV.Events
(
	ViewBookId				BIGINT NOT NULL 
	, EventId				BIGINT NOT NULL 
	, Name					NVARCHAR(96) NOT NULL
	, Tag					BIGINT NOT NULL 
	, WhenFrom				DATETIME
	, WhenTo				DATETIME
	, Remarks				NVARCHAR(512)
	, Picture				BINARY
	CONSTRAINT Events_PK PRIMARY KEY NONCLUSTERED (EventId) 
) ON FamilyViewData


GO

CREATE UNIQUE CLUSTERED INDEX Events_UX_ViewBookId_EventId
ON FV.Events(ViewBookId, EventId)
ON FamilyViewIdx

GO

CREATE UNIQUE INDEX Events_UX_ViewBookId_Name
ON FV.Events(ViewBookId, Name)
ON FamilyViewIdx

GO

ALTER TABLE FV.Events
ADD CONSTRAINT Events_FK_ContentIds
FOREIGN KEY (EventId)
REFERENCES FV.ContentIds(ContentId)

GO

ALTER TABLE FV.Events
ADD CONSTRAINT Events_FK_ContentTags
FOREIGN KEY (Tag)
REFERENCES FV.ContentTags(TagId)

GO

ALTER TABLE FV.Events
ADD CONSTRAINT Events_FK_ViewBooks
FOREIGN KEY (ViewBookId)
REFERENCES FV.ViewBooks(ViewBookId)

GO
