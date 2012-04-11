--
-- Every person in the system will get a person record
-- FamilyContacts do NOT have to be Users
-- But, to start a View you MUST be a user
--
CREATE TABLE FV.People
(
	ViewBookId				BIGINT NOT NULL 
	, PersonId				BIGINT NOT NULL 
	, Name					NVARCHAR(40) NOT NULL
	, Tag					BIGINT 
	, UserCode				INT
	, AddedBy				BIGINT
	, FirstName				NVARCHAR(40)
	, LastName				NVARCHAR(60)
	, NamePrefix			NVARCHAR(5)
	, MiddleName			NVARCHAR(40)
	, NameSuffix			NVARCHAR(5)
	, Remarks				NVARCHAR(512)
	, Gender				CHAR(1) CONSTRAINT People_CC_Gender CHECK (Gender = 'F' OR Gender = 'M')
	, DateOfBirth			DATE
	, Picture				BINARY
	, LastModifiedUserCode	INT
	, LastModifiedDatetime	DATETIME
	CONSTRAINT People_PK PRIMARY KEY NONCLUSTERED (PersonId) 
) ON FamilyViewData


GO

CREATE UNIQUE CLUSTERED INDEX People_UX_ViewBookId_PersonId
ON FV.People(ViewBookId, PersonId)
ON FamilyViewIdx

GO

CREATE UNIQUE INDEX People_UX_ViewBookId_Name
ON FV.People(ViewBookId, Name)
ON FamilyViewIdx

GO

ALTER TABLE FV.People
ADD CONSTRAINT People_FK_ViewBooks
FOREIGN KEY (ViewBookId)
REFERENCES FV.ViewBooks(ViewBookId)
GO

ALTER TABLE FV.People
ADD CONSTRAINT People_FK_ContentIds
FOREIGN KEY (PersonId)
REFERENCES FV.ContentIds(ContentId)

GO

ALTER TABLE FV.People
ADD CONSTRAINT People_FK_ContentTags
FOREIGN KEY (Tag)
REFERENCES FV.ContentTags(TagId)

GO

ALTER TABLE FV.People
ADD CONSTRAINT People_FK_People_AddedBy
FOREIGN KEY (AddedBy)
REFERENCES FV.People(PersonId)

GO
ALTER TABLE FV.People
ADD CONSTRAINT People_FK_UserMaster_UserCode
FOREIGN KEY (UserCode)
REFERENCES B1.UserMaster(UserCode)

GO
ALTER TABLE FV.People
ADD CONSTRAINT People_FK_UserMaster_LastModifedUserCode
FOREIGN KEY (LastModifiedUserCode)
REFERENCES B1.UserMaster(UserCode)

GO