--
-- Contains the immediate family or whomever is considered part of the immediate family
-- each person gets tag for that family
-- //?? restrict tag to Spouse, Child, Fiance, Single
--
CREATE TABLE FV.InstitutionMembers
(
	ViewBookId		BIGINT NOT NULL 
	, InstitutionId		BIGINT NOT NULL 
	, InstitutionMember	BIGINT NOT NULL
	, Tag				BIGINT NOT NULL
	CONSTRAINT InstitutionMembers_PK PRIMARY KEY (ViewBookId, InstitutionId, InstitutionMember) 
) ON FamilyViewData

GO

ALTER TABLE FV.InstitutionMembers
ADD CONSTRAINT InstitutionMembers_FK_ViewBooks
FOREIGN KEY (ViewBookId)
REFERENCES FV.ViewBooks(ViewBookId)
GO

ALTER TABLE FV.InstitutionMembers
ADD CONSTRAINT InstitutionMembers_FK_Institutions
FOREIGN KEY (InstitutionId)
REFERENCES FV.Institutions(InstitutionId)

GO

ALTER TABLE FV.InstitutionMembers
ADD CONSTRAINT InstitutionMembers_FK_People
FOREIGN KEY (InstitutionMember)
REFERENCES FV.People(PersonId)

GO

ALTER TABLE FV.InstitutionMembers
ADD CONSTRAINT InstitutionMembers_FK_ContentTags
FOREIGN KEY (Tag)
REFERENCES FV.ContentTags(TagId)

GO