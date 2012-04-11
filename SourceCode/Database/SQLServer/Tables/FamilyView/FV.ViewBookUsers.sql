--
-- Contains the people (but UserCodes not personId) that are part of the view book family
-- each User gets tag for that family and they get to add/modify the ViewBook
-- //?? restrict tag to Spouse, Child, Fiance, Single
--
CREATE TABLE FV.ViewBookUsers
(
	ViewBookId	BIGINT NOT NULL
	, UserCode		INT NOT NULL
	, Tag			BIGINT NOT NULL
	CONSTRAINT ViewBookUsers_PK PRIMARY KEY (ViewBookId, UserCode) 
) ON FamilyViewData

GO


ALTER TABLE FV.ViewBookUsers
ADD CONSTRAINT ViewBookUsers_FK_ViewBooks
FOREIGN KEY (ViewBookId)
REFERENCES FV.ViewBooks(ViewBookId)
GO


ALTER TABLE FV.ViewBookUsers
ADD CONSTRAINT ViewBookUsers_FK_ContentTags
FOREIGN KEY (Tag)
REFERENCES FV.ContentTags(TagId)

GO