--
--	Script will drop all the Core Objects from the db and creates schema/user.
--	We need to keep track of the schemas that are used by this application so that we can cleanup the appropriate objects and that we can reconcile the schemas
--

set serveroutput on unlimited;
Declare
                    
    v_sql varchar2(8000);
    lv_Exists number(1) := 0;

  cursor c1 is
    SELECT OWNER, OBJECT_NAME, OBJECT_TYPE 
    FROM ALL_OBJECTS WHERE OBJECT_TYPE in ('SEQUENCE', 'TABLE', 'PROCEDURE', 'FUNCTION', 'MATERIALIZED VIEW')
    and OWNER in ('B1');    

Begin
    for c1_rec in c1 loop
        if c1_rec.object_type = 'TABLE'
        then
            begin
                select 1 into lv_Exists from dual
                where exists (select null from all_tables where table_name = c1_rec.object_name and owner = c1_rec.owner)
                and not exists (select null from all_objects where object_name = c1_rec.object_name and owner = c1_rec.owner and object_type = 'MATERIALIZED VIEW');
                Exception when no_data_found then
                lv_Exists := 0;
            end;
            if lv_Exists = 1
            then            
                v_sql := 'drop table ' || c1_rec.owner || '.' || c1_rec.object_name || ' cascade constraints purge';
                DBMS_OUTPUT.PUT_LINE( v_sql) ;
                execute immediate v_sql;
            end if;          
         elsif c1_rec.object_type = 'MATERIALIZED VIEW'
         then       
            v_sql := 'drop materialized view ' || c1_rec.owner || '.' || c1_rec.object_name || ' cascade constraints purge';
            DBMS_OUTPUT.PUT_LINE( v_sql) ;
            execute immediate v_sql;
         elsif c1_rec.object_type = 'PROCEDURE'
         then       
            v_sql := 'drop procedure ' || c1_rec.owner || '.' || c1_rec.object_name;
            DBMS_OUTPUT.PUT_LINE( v_sql) ;
            execute immediate v_sql;
          
         elsif c1_rec.object_type = 'FUNCTION'
         then
            v_sql := 'drop function ' || c1_rec.owner || '.' || c1_rec.object_name;
            DBMS_OUTPUT.PUT_LINE( v_sql) ;
            execute immediate v_sql;
          
         elsif c1_rec.object_type = 'SEQUENCE'
         then
            v_sql := 'drop sequence ' || c1_rec.owner || '.' || c1_rec.object_name;
            DBMS_OUTPUT.PUT_LINE( v_sql) ;
            execute immediate v_sql;
        end if;
    end loop;

End;
/