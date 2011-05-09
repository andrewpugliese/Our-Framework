truncate table B1.TestSequence
/
alter table B1.TestSequence
add  AppSequenceName varchar2(32) not null -- sample name field
/
create unique index B1.UX_TestSequence_Name_Id on
B1.TestSequence( NLSSORT(AppSequenceName, 'NLS_SORT=BINARY_CI' ), AppSequenceId ) tablespace B1CoreIdx
/
