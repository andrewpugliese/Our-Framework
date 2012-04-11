--------------------------------------------------------------------------------
-- FAMILY VIEW INITIAL DATA

DECLARE @ctCode BIGINT
		
EXEC B1.usp_UniqueIdsGetNextBlock 'ContentTypeCode', 1, @ctCode out

INSERT INTO FV.ContentTypes(ContentTypeCode, ContentTypeName, Description)
VALUES (@ctCode, 'People', 'Content about People')


EXEC B1.usp_UniqueIdsGetNextBlock 'ContentTypeCode', 1, @ctCode out

INSERT INTO FV.ContentTypes(ContentTypeCode, ContentTypeName, Description)
VALUES (@ctCode, 'Families', 'Content about Families.')

EXEC B1.usp_UniqueIdsGetNextBlock 'ContentTypeCode', 1, @ctCode out

INSERT INTO FV.ContentTypes(ContentTypeCode, ContentTypeName, Description)
VALUES (@ctCode, 'Institutions', 'Content about Companies, Organizations, Groups, Churches, etc.')


EXEC B1.usp_UniqueIdsGetNextBlock 'ContentTypeCode', 1, @ctCode out

INSERT INTO FV.ContentTypes(ContentTypeCode, ContentTypeName, Description)
VALUES (@ctCode, 'Events', 'Content about Events (things) that occurred or are about to occur')