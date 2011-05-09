--#SET TERMINATOR /
--
-- This table is used for testing and demoing the functionality
-- of the DataAccessMgr class and the UniqueIds table and stored procedure.
--  
CREATE TABLE B1.TestSequence(
  AppSequenceId bigint NOT NULL,	-- unique id generated by DataAccessMgr
  DbSequenceId bigint GENERATED BY DEFAULT AS IDENTITY
                         (START WITH 1, INCREMENT BY 1),-- unique id generated by database 
  AppSynchTime timestamp not null,-- application-database synchronized time kept by app
  AppLocalTime timestamp not null,-- local time of application
  DbServerTime timestamp not null with DEFAULT current timestamp,-- database server time (universal time)
  AppSequenceName varchar(32) not null, -- sample name field
  AppSequenceNameUPPER generated always as (Upper(AppSequenceName)), 
  Remarks varchar(100),-- sample comments
  ExtraData clob,-- sample large data field
 CONSTRAINT PK_TestSequence_AppSequenceId PRIMARY KEY(AppSequenceId)
) in B1Core index in B1CoreIdx
/
create unique index B1.UX_TestSequence_AppSequenceName_AppSequenceId on
B1.TestSequence( AppSequenceName, AppSequenceId )
/
create unique index B1.UX_TestSequence_AppSequenceName_AppSequenceId2 on
B1.TestSequence( AppSequenceNameUPPER, AppSequenceId )
/
GRANT REFERENCES ON B1.TestSequence TO PUBLIC
/
GRANT ALL ON B1.TestSequence TO PUBLIC 
/