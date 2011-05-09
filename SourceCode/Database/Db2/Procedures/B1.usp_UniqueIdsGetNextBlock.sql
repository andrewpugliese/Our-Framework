--#SET TERMINATOR /

CREATE OR REPLACE PROCEDURE B1.usp_UniqueIdsGetNextBlock 
  (IN uniqueIdKey varchar(24), IN blockAmount int default 1, out uniqueIdValue bigint)
  LANGUAGE SQL
  AUTONOMOUS
  BEGIN

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
  DECLARE SQLSTATE CHAR(5) DEFAULT '00000';
  DECLARE lv_errLabel VARCHAR(10) DEFAULT 'stmt 0';
  DECLARE lv_errMsg VARCHAR(1024);
  DECLARE lv_errState CHAR(5);
  DECLARE lv_UniqueIdKey varchar(24);
  DECLARE lv_BlockAmount bigint;
  DECLARE lv_UniqueIdValue bigint;
  DECLARE lv_RolloverIdValue bigint;
  DECLARE lv_MaxIdValue bigint;
  DEClARE cUniqueIds CURSOR WITH HOLD FOR
  	select UniqueIdValue, MaxIdValue, RolloverIdValue
	from B1.UniqueIds
  	where UniqueIdKey = lv_UniqueIdKey
	FOR UPDATE OF UniqueIdValue;


  DECLARE EXIT HANDLER FOR SQLEXCEPTION
  BEGIN
	SET lv_errState = SQLSTATE;
	ROLLBACK;
	CASE lv_errState
	WHEN '99091' THEN RESIGNAL;
	ELSE SET lv_errMsg = 'B1.usp_UniqueIdsGetNextBlock: SQL error at location: ' 
	CONCAT lv_errLabel 
	CONCAT ' SQLSTATE: ' 
	CONCAT lv_errState;
	SIGNAL SQLSTATE '99090' SET MESSAGE_TEXT = lv_errMsg; 
	END CASE;
  END; 
  
  SET lv_UniqueIdKey = UniqueIdKey;
  OPEN  cUniqueIds;
  FETCH cUniqueIds into lv_UniqueIdValue
        , lv_MaxIdValue
        , lv_RolloverIdValue;
  CLOSE cUniqueIds;
  
  SET lv_BlockAmount = blockAmount;

  IF lv_UniqueIdValue is null then  -- key not found
        SET lv_UniqueIdValue = 0;
        SET lv_UniqueIdValue = lv_UniqueIdValue + lv_BlockAmount;
        INSERT INTO B1.UniqueIds(UniqueIdKey, UniqueIdValue)
        VALUES (lv_UniqueIdKey, lv_UniqueIdValue);
  ELSE
  
      SET lv_UniqueIdValue = lv_UniqueIdValue + lv_BlockAmount;
      
      IF lv_UniqueIdValue > lv_MaxIdValue
      THEN 
        IF lv_RolloverIdValue is null -- we have an overflow
        THEN
	    SET  lv_errmsg = 'B1.usp_UniqueIdsGetNextBlock:: Overflow for key: '|| UniqueIdKey || 'MaxValue: ' || lv_MaxIdValue || ' no rollover value found.';
	    SIGNAL SQLSTATE '99091'
	    SET MESSAGE_TEXT = lv_errmsg;
        ELSE -- set the new value to the rollover value + the block amount
            SET lv_UniqueIdValue = lv_RolloverIdValue + lv_BlockAmount;
        END IF;
      END IF;
      
      UPDATE B1.UniqueIds
         SET UniqueIdValue = lv_UniqueIdValue
       WHERE UniqueIdKey = lv_UniqueIdKey;
  END IF;
  
  COMMIT;

  SET uniqueIdValue = lv_UniqueIdValue;
  
END
/
GRANT EXECUTE ON PROCEDURE B1.usp_UniqueIdsGetNextBlock TO PUBLIC
/