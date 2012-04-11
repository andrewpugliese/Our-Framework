--
-- Contains the addresses for the family's contacts or places of interest
-- each person gets tagged with a family tag
-- Then add a record for that tag and
CREATE TABLE FV.Addresses
(
	ViewBookId		BIGINT NOT NULL 
	, Tag			BIGINT NOT NULL
	, AddressOf		BIGINT
	, AddressLine1  NVARCHAR(80) NOT NULL
	, AddressLine2  NVARCHAR(80) NOT NULL
	, AddressLine3	NVARCHAR(80)
	, City			NVARCHAR(24)
	, State			NVARCHAR(2) --//?? should it be more for international
	, PostalCode	NVARCHAR(12)
	, CountryCode   NCHAR(2)
	CONSTRAINT Addresses_PK PRIMARY KEY (ViewBookId, Tag) 
) ON FamilyViewData

GO

ALTER TABLE FV.Addresses
ADD CONSTRAINT Addresses_FK_ViewBooks
FOREIGN KEY (ViewBookId)
REFERENCES FV.ViewBooks(ViewBookId)

GO

ALTER TABLE FV.Addresses
ADD CONSTRAINT Addresses_FK_ContentIds
FOREIGN KEY (AddressOf)
REFERENCES FV.ContentIds(ContentId)

GO

ALTER TABLE FV.Addresses
ADD CONSTRAINT Addresses_FK_ContentTags
FOREIGN KEY (Tag)
REFERENCES FV.ContentTags(TagId)

GO