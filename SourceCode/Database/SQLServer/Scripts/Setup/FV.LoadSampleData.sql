--------------------------------------------------------------------------------
-- FAMILY VIEW SAMPLE DATA

-- CREATE VIEW BOOK USERS
--
BEGIN TRAN
SET XACT_ABORT ON

DECLARE @uVBgc INT

EXEC B1.usp_UniqueIdsGetNextBlock 'AccessControlGroupCode', 1, @uVBgc out

INSERT INTO B1.AccessControlGroups (AccessControlGroupCode, AccessControlGroupName, DefaultAccessDenied, Remarks)
VALUES
(
@uVBgc, 'ViewBookUsers', 0, 'Users for viewing and updating a ViewBook.'
)

DECLARE @uc INT, @andrew INT, @narayan INT, @rebecca INT, @drue INT, @lili INT, @minal INT

EXEC B1.usp_UniqueIdsGetNextBlock 'UserCode', 1, @uc out

INSERT INTO B1.UserMaster
(
	UserCode
	, UserId
	, UserPassword
	, PasswordSalt
	, DefaultAccessGroupCode
	, SignonRestricted
	, MultipleSignonAllowed
	, ForcePasswordChange
	, FailedSignonAttempts
	, NamePrefix
	, FirstName
	, MiddleName
	, LastName
	, NameSuffix
	, Remarks
)
VALUES
(
@uc
, 'andrew@puglieses.net'
, '4ejxiLVxr/CVUsjPjbks3QGxlamYrtJGKvK2lEbSP6YT5g31B9FXnzv7DV41OnXPbuLk+aahdxCbAh0bqYfp/w=='
, 'fpi8580t7r2FpeouIiPOKRGGfcLeOyIeg+qX03MymqJm0SoDAeMGWOovESyyaPTLAyvU'
, @uVBgc
, 0
, 1
, 0
, 0
, 'Mr'
, 'Andrew'
, null
, 'Pugliese'
, null
, 'A user of Family View and ViewBook Owner.'
)

SET @andrew = @uc

EXEC B1.usp_UniqueIdsGetNextBlock 'UserCode', 1, @uc out

INSERT INTO B1.UserMaster
(
	UserCode
	, UserId
	, UserPassword
	, PasswordSalt
	, DefaultAccessGroupCode
	, SignonRestricted
	, MultipleSignonAllowed
	, ForcePasswordChange
	, FailedSignonAttempts
	, NamePrefix
	, FirstName
	, MiddleName
	, LastName
	, NameSuffix
	, Remarks
)
VALUES
(
@uc
, 'rebecca@puglieses.net'
, '4ejxiLVxr/CVUsjPjbks3QGxlamYrtJGKvK2lEbSP6YT5g31B9FXnzv7DV41OnXPbuLk+aahdxCbAh0bqYfp/w=='
, 'fpi8580t7r2FpeouIiPOKRGGfcLeOyIeg+qX03MymqJm0SoDAeMGWOovESyyaPTLAyvU'
, @uVBgc
, 0
, 1
, 0
, 0
, 'Mrs'
, 'Rebecca'
, 'Claire'
, 'Pugliese'
, null
, 'A user of Family View and ViewBook Contributor.'
)

SET @rebecca = @uc

EXEC B1.usp_UniqueIdsGetNextBlock 'UserCode', 1, @uc out

INSERT INTO B1.UserMaster
(
	UserCode
	, UserId
	, UserPassword
	, PasswordSalt
	, DefaultAccessGroupCode
	, SignonRestricted
	, MultipleSignonAllowed
	, ForcePasswordChange
	, FailedSignonAttempts
	, NamePrefix
	, FirstName
	, MiddleName
	, LastName
	, NameSuffix
	, Remarks
)
VALUES
(
@uc
, 'drue@puglieses.net'
, '4ejxiLVxr/CVUsjPjbks3QGxlamYrtJGKvK2lEbSP6YT5g31B9FXnzv7DV41OnXPbuLk+aahdxCbAh0bqYfp/w=='
, 'fpi8580t7r2FpeouIiPOKRGGfcLeOyIeg+qX03MymqJm0SoDAeMGWOovESyyaPTLAyvU'
, @uVBgc
, 0
, 1
, 0
, 0
, 'Mstr'
, 'Drue'
, 'Anthony'
, 'Pugliese'
, null
, 'A user of Family View and ViewBook contributor.'
)

SET @drue = @uc

EXEC B1.usp_UniqueIdsGetNextBlock 'UserCode', 1, @uc out

INSERT INTO B1.UserMaster
(
	UserCode
	, UserId
	, UserPassword
	, PasswordSalt
	, DefaultAccessGroupCode
	, SignonRestricted
	, MultipleSignonAllowed
	, ForcePasswordChange
	, FailedSignonAttempts
	, NamePrefix
	, FirstName
	, MiddleName
	, LastName
	, NameSuffix
	, Remarks
)
VALUES
(
@uc
, 'lili@puglieses.net'
, '4ejxiLVxr/CVUsjPjbks3QGxlamYrtJGKvK2lEbSP6YT5g31B9FXnzv7DV41OnXPbuLk+aahdxCbAh0bqYfp/w=='
, 'fpi8580t7r2FpeouIiPOKRGGfcLeOyIeg+qX03MymqJm0SoDAeMGWOovESyyaPTLAyvU'
, @uVBgc
, 0
, 1
, 0
, 0
, 'Ms'
, 'Lili'
, 'Marie'
, 'Pugliese'
, null
, 'A user of Family View and ViewBook contibutor.'
)

SET @lili = @uc

EXEC B1.usp_UniqueIdsGetNextBlock 'UserCode', 1, @uc out

INSERT INTO B1.UserMaster
(
	UserCode
	, UserId
	, UserPassword
	, PasswordSalt
	, DefaultAccessGroupCode
	, SignonRestricted
	, MultipleSignonAllowed
	, ForcePasswordChange
	, FailedSignonAttempts
	, NamePrefix
	, FirstName
	, MiddleName
	, LastName
	, NameSuffix
	, Remarks
)
VALUES
(
@uc
, 'snarayans@yahoo.com'
, '4ejxiLVxr/CVUsjPjbks3QGxlamYrtJGKvK2lEbSP6YT5g31B9FXnzv7DV41OnXPbuLk+aahdxCbAh0bqYfp/w=='
, 'fpi8580t7r2FpeouIiPOKRGGfcLeOyIeg+qX03MymqJm0SoDAeMGWOovESyyaPTLAyvU'
, @uVBgc
, 0
, 1
, 0
, 0
, 'Mr'
, 'Narayan'
, null
, 'Singhal'
, null
, 'A user of Family View and ViewBook owner.'
)

SET @narayan = @uc

EXEC B1.usp_UniqueIdsGetNextBlock 'UserCode', 1, @uc out

INSERT INTO B1.UserMaster
(
	UserCode
	, UserId
	, UserPassword
	, PasswordSalt
	, DefaultAccessGroupCode
	, SignonRestricted
	, MultipleSignonAllowed
	, ForcePasswordChange
	, FailedSignonAttempts
	, NamePrefix
	, FirstName
	, MiddleName
	, LastName
	, NameSuffix
	, Remarks
)
VALUES
(
@uc
, 'sminals@yahoo.com'
, '4ejxiLVxr/CVUsjPjbks3QGxlamYrtJGKvK2lEbSP6YT5g31B9FXnzv7DV41OnXPbuLk+aahdxCbAh0bqYfp/w=='
, 'fpi8580t7r2FpeouIiPOKRGGfcLeOyIeg+qX03MymqJm0SoDAeMGWOovESyyaPTLAyvU'
, @uVBgc
, 0
, 1
, 0
, 0
, 'Mrs'
, 'Minal'
, null
, 'Singhal'
, null
, 'A user of Family View and ViewBook contibutor.'
)

SET @minal = @uc


----------------------------------------------------------------------------

-- CREATE VIEW BOOKS


DECLARE @puglieseViewBook BIGINT, @singhalViewBook BIGINT
		, @andrewPerson BIGINT, @rebeccaPerson BIGINT, @druePerson BIGINT, @liliPerson BIGINT
		, @narayanPerson BIGINT, @minalPerson BIGINT
		, @contentId BIGINT, @contentTypePeople INT
		
EXEC B1.usp_UniqueIdsGetNextBlock 'ViewBookId', 1, @puglieseViewBook out

INSERT INTO FV.ViewBooks(ViewBookId, Title, StartedByUserCode)
VALUES (@puglieseViewBook, 'The Pugliese Family ViewBook', @andrew)
		
EXEC B1.usp_UniqueIdsGetNextBlock 'ViewBookId', 1, @singhalViewBook out

INSERT INTO FV.ViewBooks(ViewBookId, Title, StartedByUserCode)
VALUES (@singhalViewBook, 'The Singhal Family ViewBook', @narayan)

-- Create People Records

SELECT @contentTypePeople = ContentTypeCode
FROM FV.ContentTypes
WHERE ContentTypeName = 'People'

EXEC B1.usp_UniqueIdsGetNextBlock 'ContentId', 1, @contentId out

INSERT INTO FV.ContentIds
(
	ViewBookId
	, ContentId
	, ContentTypeCode
	, AddedByUserCode
)
VALUES
(
	@puglieseViewBook
	, @contentId
	, @contentTypePeople
	, @andrew
)

SET @andrewPerson = @contentId

INSERT INTO FV.People
(
	ViewBookId
	, PersonId
	, Name
	, UserCode
	, AddedBy
	, FirstName
	, LastName
	, NamePrefix
	, MiddleName
	, Gender
	, DateOfBirth
)
VALUES
(
	@puglieseViewBook
	, @andrewPerson
	, 'Andrew Pugliese'
	, @andrew
	, @andrewPerson
	, 'Andrew'
	, 'Pugliese'
	, 'Mr.'
	, null
	, 'M'
	, '09/07/1965'
)


EXEC B1.usp_UniqueIdsGetNextBlock 'ContentId', 1, @contentId out

INSERT INTO FV.ContentIds
(
	ViewBookId
	, ContentId
	, ContentTypeCode
	, AddedByUserCode
)
VALUES
(
	@puglieseViewBook
	, @contentId
	, @contentTypePeople
	, @rebecca
)

SET @rebeccaPerson = @contentId

INSERT INTO FV.People
(
	ViewBookId
	, PersonId
	, Name
	, UserCode
	, AddedBy
	, FirstName
	, LastName
	, NamePrefix
	, MiddleName
	, Gender
	, DateOfBirth
)
VALUES
(
	@puglieseViewBook
	, @rebeccaPerson
	, 'Rebecca Pugliese'
	, @rebecca
	, @andrewPerson
	, 'Rebecca'
	, 'Pugliese'
	, 'Mrs.'
	, 'Clare'
	, 'F'
	, '06/24/1965'
)


EXEC B1.usp_UniqueIdsGetNextBlock 'ContentId', 1, @contentId out

INSERT INTO FV.ContentIds
(
	ViewBookId
	, ContentId
	, ContentTypeCode
	, AddedByUserCode
)
VALUES
(
	@puglieseViewBook
	, @contentId
	, @contentTypePeople
	, @drue
)

SET @druePerson = @contentId

INSERT INTO FV.People
(
	ViewBookId
	, PersonId
	, Name
	, UserCode
	, AddedBy
	, FirstName
	, LastName
	, NamePrefix
	, MiddleName
	, Gender
	, DateOfBirth
)
VALUES
(
	@puglieseViewBook
	, @druePerson
	, 'Drue Pugliese'
	, @drue
	, @andrewPerson
	, 'Drue'
	, 'Pugliese'
	, 'Mstr.'
	, 'Anthony'
	, 'M'
	, '09/15/1998'
)


EXEC B1.usp_UniqueIdsGetNextBlock 'ContentId', 1, @contentId out

INSERT INTO FV.ContentIds
(
	ViewBookId
	, ContentId
	, ContentTypeCode
	, AddedByUserCode
)
VALUES
(
	@puglieseViewBook
	, @contentId
	, @contentTypePeople
	, @lili
)

SET @liliPerson = @contentId

INSERT INTO FV.People
(
	ViewBookId
	, PersonId
	, Name
	, UserCode
	, AddedBy
	, FirstName
	, LastName
	, NamePrefix
	, MiddleName
	, Gender
	, DateOfBirth
)
VALUES
(
	@puglieseViewBook
	, @liliPerson
	, 'Lili Pugliese'
	, @lili
	, @andrewPerson
	, 'Lili'
	, 'Pugliese'
	, 'Ms.'
	, 'Marie'
	, 'F'
	, '09/12/2001'
)




EXEC B1.usp_UniqueIdsGetNextBlock 'ContentId', 1, @contentId out

INSERT INTO FV.ContentIds
(
	ViewBookId
	, ContentId
	, ContentTypeCode
	, AddedByUserCode
)
VALUES
(
	@singhalViewBook
	, @contentId
	, @contentTypePeople
	, @narayan
)

SET @narayanPerson = @contentId

INSERT INTO FV.People
(
	ViewBookId
	, PersonId
	, Name
	, UserCode
	, AddedBy
	, FirstName
	, LastName
	, NamePrefix
	, MiddleName
	, Gender
	, DateOfBirth
)
VALUES
(
	@singhalViewBook
	, @narayanPerson
	, 'Narayan Singhal'
	, @narayan
	, @narayanPerson
	, 'Narayan'
	, 'Singhal'
	, 'Mr.'
	, null
	, 'M'
	, '10/01/1974'
)


EXEC B1.usp_UniqueIdsGetNextBlock 'ContentId', 1, @contentId out

INSERT INTO FV.ContentIds
(
	ViewBookId
	, ContentId
	, ContentTypeCode
	, AddedByUserCode
)
VALUES
(
	@singhalViewBook
	, @contentId
	, @contentTypePeople
	, @minal
)

SET @minalPerson = @contentId

INSERT INTO FV.People
(
	ViewBookId
	, PersonId
	, Name
	, UserCode
	, AddedBy
	, FirstName
	, LastName
	, NamePrefix
	, MiddleName
	, Gender
	, DateOfBirth
)
VALUES
(
	@singhalViewBook
	, @minalPerson
	, 'Minal Singhal'
	, @minal
	, @narayanPerson
	, 'Minal'
	, 'Singhal'
	, 'Mrs.'
	, null
	, 'F'
	, '01/01/2000'
)

COMMIT TRAN