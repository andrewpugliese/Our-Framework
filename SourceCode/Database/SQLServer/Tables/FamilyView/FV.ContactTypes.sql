-- 
-- Contains system defined contact types (e.g. Family, Friends, Aquaintence, ServiceProvider)
--
CREATE TABLE FV.ContactTypes
(
	ContactTypeCode			INT NOT NULL
	, ContactTypeName		NVARCHAR(16) NOT NULL
	, Description			NVARCHAR(256) NOT NULL
	CONSTRAINT ContactTypes_PK PRIMARY KEY (ContactTypeCode) 
) ON FamilyViewData

GO

CREATE UNIQUE INDEX ContactTypes_UX_ContactTypeName
ON FV.ContactTypes(ContactTypeName)
ON FamilyViewIdx

GO