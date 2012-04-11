CREATE TABLE FV.Referrals
(
	ViewBookId				BIGINT NOT NULL 
	, ContentId				BIGINT NOT NULL 
	, ReferredBy			BIGINT NOT NULL
	, ReferredOn			DATE   NOT NULL
	, Remarks				NVARCHAR(256)
	CONSTRAINT Referrals_PK PRIMARY KEY (ViewBookId, ContentId, ReferredBy) 
) ON FamilyViewData

GO

CREATE UNIQUE INDEX Referrals_UX_ViewBookId_ReferredBy
ON FV.Referrals(ViewBookId, ReferredBy, ContentId)
ON FamilyViewIdx

GO

ALTER TABLE FV.Referrals
ADD CONSTRAINT Referrals_FK_ViewBooks
FOREIGN KEY (ViewBookId)
REFERENCES FV.ViewBooks(ViewBookId)

GO
ALTER TABLE FV.Referrals
ADD CONSTRAINT Referrals_FK_ContentIds
FOREIGN KEY (ContentId)
REFERENCES FV.ContentIds(ContentId)

GO

ALTER TABLE FV.Referrals
ADD CONSTRAINT Referrals_FK_People_ReferredBy
FOREIGN KEY (ReferredBy)
REFERENCES FV.People(PersonId)

GO

