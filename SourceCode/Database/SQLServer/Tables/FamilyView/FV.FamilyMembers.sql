--
-- Contains the immediate family or whomever is considered part of the immediate family
-- each person gets tag for that family
-- //?? restrict tag to Spouse, Child, Fiance, Single
--
CREATE TABLE FV.FamilyMembers
(
	ViewBookId	BIGINT NOT NULL 
	, FamilyId		BIGINT NOT NULL 
	, FamilyMember	BIGINT NOT NULL
	, Tag			BIGINT NOT NULL
	CONSTRAINT FamilyMembers_PK PRIMARY KEY (ViewBookId, FamilyId, FamilyMember) 
) ON FamilyViewData

GO

ALTER TABLE FV.FamilyMembers
ADD CONSTRAINT FamilyMembers_FK_ViewBooks
FOREIGN KEY (ViewBookId)
REFERENCES FV.ViewBooks(ViewBookId)
GO

ALTER TABLE FV.FamilyMembers
ADD CONSTRAINT FamilyMembers_FK_Families
FOREIGN KEY (FamilyId)
REFERENCES FV.Families(FamilyId)

GO

ALTER TABLE FV.FamilyMembers
ADD CONSTRAINT FamilyMembers_FK_People
FOREIGN KEY (FamilyMember)
REFERENCES FV.People(PersonId)

GO

ALTER TABLE FV.FamilyMembers
ADD CONSTRAINT FamilyMembers_FK_ContentTags
FOREIGN KEY (Tag)
REFERENCES FV.ContentTags(TagId)

GO