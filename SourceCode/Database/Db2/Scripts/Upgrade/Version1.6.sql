--#SET TERMINATOR /
--
ALTER TABLE B1.TestSequence ACTIVATE NOT LOGGED INITIALLY WITH EMPTY TABLE 
/
delete from B1.TestSequence 
/
COMMIT
/
alter table B1.TestSequence
add  AppSequenceName varchar(32) null  -- sample name field
/
alter table B1.TestSequence
alter column AppSequenceName set not null  
/
set integrity for B1.TestSequence off
/
alter table B1.TestSequence
add  AppSequenceNameUPPER generated always as (upper(AppSequenceName))  -- sample name field
/
set integrity for B1.TestSequence immediate checked
/
create unique index B1.UX_TestSequence_Name_Id on
B1.TestSequence( AppSequenceName, AppSequenceId )
/
create unique index B1.UX_TestSequence_Name_Id2 on
B1.TestSequence( AppSequenceNameUPPER, AppSequenceId )
/
