--
-- Every person in the system will get a person record
-- FamilyContacts do NOT have to be Users
-- But, to start a View you MUST be a user
--
CREATE TABLE FV.Institutions
(
	ViewBookId				BIGINT NOT NULL 
	, InstitutionId			BIGINT NOT NULL 
	, Name					NVARCHAR(96) NOT NULL
	, Tag					BIGINT NOT NULL 
	, Remarks				NVARCHAR(512)
	, Picture				BINARY
	CONSTRAINT Institutions_PK PRIMARY KEY NONCLUSTERED (InstitutionId) 
) ON FamilyViewData


GO

CREATE UNIQUE CLUSTERED INDEX Institutions_UX_ViewBookId_InstitutionId
ON FV.Institutions(ViewBookId, InstitutionId)
ON FamilyViewIdx

GO

CREATE UNIQUE INDEX Institutions_UX_ViewBookId_Name
ON FV.Institutions(ViewBookId, Name)
ON FamilyViewIdx

GO

ALTER TABLE FV.Institutions
ADD CONSTRAINT Institutions_FK_ContentIds
FOREIGN KEY (InstitutionId)
REFERENCES FV.ContentIds(ContentId)

GO

ALTER TABLE FV.Institutions
ADD CONSTRAINT Institutions_FK_ContentTags
FOREIGN KEY (Tag)
REFERENCES FV.ContentTags(TagId)

GO

ALTER TABLE FV.Institutions
ADD CONSTRAINT Institutions_FK_ViewBooks
FOREIGN KEY (ViewBookId)
REFERENCES FV.ViewBooks(ViewBookId)

GO
