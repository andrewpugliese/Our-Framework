CREATE TABLE FV.EventAttendees
(
	ViewBookId		BIGINT NOT NULL 
	, EventId		BIGINT NOT NULL 
	, EventAttendee	BIGINT NOT NULL
	, Tag				BIGINT NOT NULL
	CONSTRAINT EventAttendees_PK PRIMARY KEY (ViewBookId, EventId, EventAttendee) 
) ON FamilyViewData

GO

ALTER TABLE FV.EventAttendees
ADD CONSTRAINT EventAttendees_FK_ViewBooks
FOREIGN KEY (ViewBookId)
REFERENCES FV.ViewBooks(ViewBookId)
GO

ALTER TABLE FV.EventAttendees
ADD CONSTRAINT EventAttendees_FK_Institutions
FOREIGN KEY (EventId)
REFERENCES FV.Events(EventId)

GO

ALTER TABLE FV.EventAttendees
ADD CONSTRAINT EventAttendees_FK_People
FOREIGN KEY (EventAttendee)
REFERENCES FV.People(PersonId)

GO

ALTER TABLE FV.EventAttendees
ADD CONSTRAINT EventAttendees_FK_ContentTags
FOREIGN KEY (Tag)
REFERENCES FV.ContentTags(TagId)

