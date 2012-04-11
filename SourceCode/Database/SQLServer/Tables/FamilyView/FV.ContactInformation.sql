--
-- Contains the ContactInformation for the family's contacts or places of interest
-- each person gets tagged with a family tag
-- Then add a record for that tag and
CREATE TABLE FV.ContactInformation
(
	ViewBookId		BIGINT NOT NULL 
	, Tag			BIGINT NOT NULL
	, ContactFor	BIGINT NOT NULL
	, ContactInfo	NVARCHAR(128) NOT NULL
	, ContactOrder	INT
	CONSTRAINT ContactInformation_PK PRIMARY KEY (ViewBookId, Tag) 
) ON FamilyViewData

GO


CREATE UNIQUE INDEX ContactInformation_UX_ContactInfo_ContactFor_ViewBookId
ON FV.ContactInformation(ContactInfo, ContactFor, ViewBookId)
ON FamilyViewIdx

GO

ALTER TABLE FV.ContactInformation
ADD CONSTRAINT ContactInformation_FK_ViewBooks
FOREIGN KEY (ViewBookId)
REFERENCES FV.ViewBooks(ViewBookId)

GO

ALTER TABLE FV.ContactInformation
ADD CONSTRAINT ContactInformation_FK_ContentIds
FOREIGN KEY (ContactFor)
REFERENCES FV.ContentIds(ContentId)

GO

ALTER TABLE FV.ContactInformation
ADD CONSTRAINT ContactInformation_FK_ContentTags
FOREIGN KEY (Tag)
REFERENCES FV.ContentTags(TagId)

GO