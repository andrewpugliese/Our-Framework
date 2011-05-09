--#SET TERMINATOR /
-- Drop database 
DROP DATABASE B1SAMPLE
/
-- Create a sample database
CREATE DATABASE B1SAMPLE AUTOMATIC STORAGE YES ON 'G:'
/
UPDATE DB CFG FOR B1SAMPLE USING AUTO_MAINT ON
/
UPDATE DB CFG FOR B1SAMPLE USING AUTO_TBL_MAINT ON
/
UPDATE DB CFG FOR B1SAMPLE USING AUTO_RUNSTATS ON
/
UPDATE ALERT CFG FOR DATABASE ON B1SAMPLE USING db.db_backup_req SET THRESHOLDSCHECKED YES
/
UPDATE ALERT CFG FOR DATABASE ON B1SAMPLE USING db.tb_reorg_req SET THRESHOLDSCHECKED YES
/
UPDATE ALERT CFG FOR DATABASE ON B1SAMPLE USING db.tb_runstats_req SET THRESHOLDSCHECKED YES
/
-- connect to the database using the default userid used when db2 was installed
CONNECT TO B1SAMPLE
/
-- create sample tablespaces on database (for tables)
CREATE  REGULAR  TABLESPACE B1CORE PAGESIZE 4 K  MANAGED BY AUTOMATIC STORAGE 
EXTENTSIZE 16 OVERHEAD 10.5 PREFETCHSIZE 16 TRANSFERRATE 0.14 BUFFERPOOL  IBMDEFAULTBP  DROPPED TABLE RECOVERY ON
/
-- create sample tablespaces on database (for indexes)
CREATE  REGULAR  TABLESPACE B1COREIDX PAGESIZE 4 K  MANAGED BY AUTOMATIC STORAGE 
EXTENTSIZE 16 OVERHEAD 10.5 PREFETCHSIZE 16 TRANSFERRATE 0.14 BUFFERPOOL  IBMDEFAULTBP  DROPPED TABLE RECOVERY ON
/
-- create the B1 schema for the database objects
CREATE SCHEMA B1 AUTHORIZATION ADMINISTRATOR
/
