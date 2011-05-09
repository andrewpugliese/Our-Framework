--
-- This table is used to generate unique numbers for any given key.
--
-- The id's are generated by stored procedure B1.usp_UniqueIdsGetNextBlock
-- All application logic accesing this table is found in the DataAccessMgr class.
-- The class will load this meta data into a local cache
-- and distribute unique ids in a thread safe manner without requiring locking
-- on this table for each number.  Instead the CacheBlockSize is used to set
-- the size of the cache block for each key.  
-- Note: The larger the size, the greater the scalability, but there is a risk
--		wasting more unique ids.  The class will attempt to return those unused
--		cached ids; but only if another class or stored proc call has not already
--		requested a unique id before the ids were returned.
--
CREATE TABLE B1.UniqueIds(
  UniqueIdKey Varchar2(64) NOT NULL, -- unique key which will be associated with unique value
  UniqueIdValue number(19) DEFAULT(0),-- unique value pointer
  CacheBlockSize number(5) default(100) not null, -- configurable size of a block for the key
  MaxIdValue number(19) null,-- maximum value the key can obtain
  RolloverIdValue number(19) null,-- rollover value and maximum is reached
 CONSTRAINT PK_UniqueIds_UniqueIdKey PRIMARY KEY(UniqueIdKey)
) tablespace B1Core
/
-- sample case insensitve index on uniqueidkey
create unique index B1.UX_UniqueIds_UniqueIdKey on
B1.UniqueIds( NLSSORT(UniqueIdKey, 'NLS_SORT=BINARY_CI' )) tablespace B1CoreIdx
/
grant references on B1.UniqueIds to public;
/
grant insert, select, update, delete on B1.UniqueIds to public;
-- NOTE: YOU MUST END THIS FILE WITH THE SLASH OR COMMAND LINE SQLPLUS WILL HANG WAITING FOR COMMAND INPUT
/