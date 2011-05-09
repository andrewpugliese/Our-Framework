--
truncate table B1.TestSequence
go
alter table B1.TestSequence
add  AppSequenceName varchar(32) not null -- sample name field
go
create unique index UX_TestSequence_Name_Id on
B1.TestSequence( AppSequenceName, AppSequenceId )
