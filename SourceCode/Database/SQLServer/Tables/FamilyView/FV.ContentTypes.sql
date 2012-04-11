-- 
-- Contains system defined content types (e.g. People, Music, Pictures, Books, etc)
--
CREATE TABLE FV.ContentTypes
(
	ContentTypeCode			INT NOT NULL
	, ContentTypeName		NVARCHAR(16) NOT NULL
	, Description			NVARCHAR(256) NOT NULL
	CONSTRAINT ContentTypes_PK PRIMARY KEY (ContentTypeCode) 
) ON FamilyViewData

GO

CREATE UNIQUE INDEX ContentTypes_UX_ContentTypeName
ON FV.ContentTypes(ContentTypeName)
ON FamilyViewIdx

GO