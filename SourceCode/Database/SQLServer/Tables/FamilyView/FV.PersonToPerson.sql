--
-- Contains records outlining requests for two persons to 'Connect'
-- A request must be accepted before connection is made.
--
-- NOTE:
-- A person may receive a request from a person (who they know) but do not have an
-- existing person record for.  So the application ByFamily and ByPerson columns
-- refer to the requester's data.  Once the request is accepted that accepting party
-- will have that person's record added as a Person record under the accepting party's family.
-- 
CREATE TABLE FV.PersonToPerson
(
	P2PId				BIGINT NOT NULL
	, ByFamily			BIGINT NOT NULL 
	, ToFamily			BIGINT NOT NULL 
	, RequestedOn		DATETIME NOT NULL
	, ByPerson			BIGINT NOT NULL
	, ToPerson			BIGINT NOT NULL 
	, ConnectedOn		DATETIME 
	, DeniedOn			DATETIME 
	, EndedOn			DATETIME 
	, EndedByFamily		BIGINT 
	, EndedByPerson		BIGINT 
	, Remarks			NVARCHAR(512) 
	CONSTRAINT PersonToPerson_PK PRIMARY KEY NONCLUSTERED (P2PId) 
) ON FamilyViewData

GO

--
-- cluster data on the by families
--
CREATE CLUSTERED INDEX PersonToPerson_IX_ByFamily_ToFamily
ON FV.PersonToPerson(ByFamily, ToFamily)
ON FamilyViewIdx

GO

CREATE INDEX PersonToPerson_IX_ToFamily_ByFamily_RequestedOn
ON FV.PersonToPerson(ToFamily, ConnectedOn, RequestedOn, ByFamily, ByPerson)
ON FamilyViewIdx

GO

CREATE INDEX PersonToPerson_IX_ToFamily_ByFamily_ConnectedOn
ON FV.PersonToPerson(ToFamily, ConnectedOn, ByFamily, ByPerson)
ON FamilyViewIdx

GO

ALTER TABLE FV.PersonToPerson
ADD CONSTRAINT PersonToPerson_FK_ContentIds
FOREIGN KEY (P2PId)
REFERENCES FV.ContentIds(ContentId)
GO

ALTER TABLE FV.PersonToPerson
ADD CONSTRAINT PersonToPerson_FK_Families_ByFamily
FOREIGN KEY (ByFamily)
REFERENCES FV.Families(FamilyId)
GO

ALTER TABLE FV.PersonToPerson
ADD CONSTRAINT PersonToPerson_FK_Families_ToFamily
FOREIGN KEY (ToFamily)
REFERENCES FV.Families(FamilyId)
GO


ALTER TABLE FV.PersonToPerson
ADD CONSTRAINT PersonToPerson_FK_People_ByPerson
FOREIGN KEY (ByPerson)
REFERENCES FV.People(PersonId)

GO

ALTER TABLE FV.PersonToPerson
ADD CONSTRAINT PersonToPerson_FK_People_ToPerson
FOREIGN KEY (ToPerson)
REFERENCES FV.People(PersonId)

GO
