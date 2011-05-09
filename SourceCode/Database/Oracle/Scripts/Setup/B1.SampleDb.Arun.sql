alter session set NLS_SORT = BINARY_CI;
alter session set NLS_COMP = LINGUISTIC;
/
declare lv_exists number(1) := 0;

begin
    select count(*) into lv_exists 
    from all_users where username = 'B1';

    if lv_exists = 1
    then 
        execute immediate 'drop user B1 cascade';
    end if;
end;
/
declare lv_exists number(1) := 0;

begin
    select count(*) into lv_exists 
    FROM DBA_DATA_FILES where TableSpace_Name = 'B1CORE';

    if lv_exists = 1
    then 
        execute immediate 'drop tablespace B1CORE including contents and datafiles';
    end if;
end;
/
declare lv_exists number(1) := 0;

begin
    select count(*) into lv_exists 
    FROM DBA_DATA_FILES where TableSpace_Name = 'B1COREIDX';

    if lv_exists = 1
    then 
        execute immediate 'drop tablespace B1COREIDX including contents and datafiles';
    end if;
end;
/
create tablespace B1Core datafile 'D:\DB\Oracle\db\oradata\orcl\B1Core.DBF'
SIZE 20M AUTOEXTEND ON
/
create tablespace B1CoreIdx
datafile 'D:\DB\Oracle\db\oradata\orcl\B1CoreIdx.DBF'
SIZE 20M AUTOEXTEND ON
/
create user B1 identified by B1
default tablespace B1Core
/
grant create session TO B1
/
grant resource TO B1
/