create or replace procedure B1.usp_UniqueIdsGetNextBlock
(
UniqueIdKey varchar2,
BlockAmount number default 1,
UniqueIdValue out number
)
as


pragma autonomous_transaction;

  lv_UniqueIdValue B1.UniqueIds.UniqueIdValue%type := null;
  lv_UniqueIdKey B1.UniqueIds.UniqueIdKey%type := UniqueIdKey;
  lv_MaxIdValue B1.UniqueIds.MaxIdValue%type := null;
  lv_RolloverIdValue B1.UniqueIds.RolloverIdValue%type := null;
  lv_BlockAmount B1.UniqueIds.CacheBlockSize%type := BlockAmount;

cursor cUniqueIds is
  select UniqueIdValue, MaxIdValue, RolloverIdValue
  from B1.UniqueIds
  where UniqueIdKey = lv_UniqueIdKey
  for update of UniqueIdValue;
  
begin

--
--	Procedure Get Next UniqueId Block
--	This procedure will obtain the next UniqueId block for
--	a given uniqueIdKey and blocksize.  
--	The key is the identifier for which the uniqueIds 
--		will be generated; e.g. tableName).
--	The BlockSize determines what the new value will be
--		the default is sequential, but you could obtain
--		50 at a time for example.  
--	It will return the end of the block and you use all the numbers
--		from the returned value - the block size.
--
-- If MaxValue is not null then that will be the limit for that key
-- If RollOverValue is not null, then that will be the new value,
-- if MaxValue is not null and has been reached.  If RollOverValue
-- is null when MaxValue has been reached, then it will result in 
-- an exception.
--
--

  open  cUniqueIds;
  fetch cUniqueIds into lv_UniqueIdValue
        , lv_MaxIdValue
        , lv_RolloverIdValue;
  close cUniqueIds;

  if lv_UniqueIdValue is null then  -- key not found
        lv_UniqueIdValue := 0;
        lv_UniqueIdValue := lv_UniqueIdValue + lv_BlockAmount;
        insert into B1.UniqueIds(UniqueIdKey, UniqueIdValue)
        values (lv_UniqueIdKey, lv_UniqueIdValue);
  else
  
      lv_UniqueIdValue := lv_UniqueIdValue + lv_BlockAmount;
      
      if lv_UniqueIdValue > lv_MaxIdValue
      then 
        if lv_RolloverIdValue is null -- we have an overflow
        then
            rollback;
            raise_application_error (-20001, 'usp_UniqueIdsGetNextBlock:: Overflow for key: '|| UniqueIdKey || 'MaxValue: ' || lv_MaxIdValue || ' no rollover value found.');
            return;
        else -- set the new value to the rollover value + the block amount
            lv_UniqueIdValue := lv_RolloverIdValue + lv_BlockAmount;
        end if;
      end if;
      
      update B1.UniqueIds
         set UniqueIdValue = lv_UniqueIdValue
       where UniqueIdKey = lv_UniqueIdKey;
  end if;
  
  commit;
  UniqueIdValue := lv_UniqueIdValue;
  
  Exception when others then
    rollback;
    raise;

End;
/
grant execute on B1.usp_UniqueIdsGetNextBlock to public;
/