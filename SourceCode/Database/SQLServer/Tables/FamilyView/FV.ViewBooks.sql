--
-- Main table for application functionality
-- Contains the unique ViewBook definitions and their creators
--
CREATE TABLE FV.ViewBooks
(
	ViewBookId			BIGINT NOT NULL
	, Title				NVARCHAR(64) NOT NULL
	, StartedByUserCode INT NOT NULL
	, StartedOn			DATETIME NOT NULL DEFAULT(GETUTCDATE())
	, EndedOn			DATETIME -- can only be ended by StartedBy person && if all members agree ??
	CONSTRAINT ViewBooks_PK PRIMARY KEY (ViewBookId) 
) ON FamilyViewData

GO

CREATE UNIQUE INDEX ViewBooks_UX_Title_ViewBookId
ON FV.ViewBooks(Title, ViewBookId)
ON FamilyViewIdx

GO

ALTER TABLE FV.ViewBooks
ADD CONSTRAINT ViewBooks_FK_UserMaster_StartedByUserCode
FOREIGN KEY (StartedByUserCode)
REFERENCES B1.UserMaster(UserCode)

GO
	