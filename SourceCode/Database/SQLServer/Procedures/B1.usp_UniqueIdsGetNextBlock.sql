-- drop existing object if exists
--
if exists (select null from sys.procedures p 
						inner join sys.schemas s
						on p.schema_id = s.schema_id
						and s.name = 'B1'
						where p.name = 'usp_UniqueIdsGetNextBlock')
						drop proc B1.usp_UniqueIdsGetNextBlock
go
--						
create proc B1.usp_UniqueIdsGetNextBlock
(
@UniqueIdKey varchar(64),
@BlockAmount int = 1,
@UniqueIdValue bigint out
)
as
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
	set nocount on
	declare @MaxIdValue bigint, @RolloverIdValue bigint
	
	begin tran
	set xact_abort on

		select @UniqueIdValue = UniqueIdValue
		, @MaxIdValue = MaxIdValue
		, @RolloverIdValue = RolloverIdValue
		from B1.UniqueIds with (xlock, rowlock)
		where UniqueIdKey = @UniqueIdKey

		if @UniqueIdValue is null	-- key not found
		begin
			insert into B1.UniqueIds (UniqueIdKey, UniqueIdValue)
			values (@UniqueIdKey, @BlockAmount)
			set @UniqueIdValue = @BlockAmount
			commit tran
			return
		end
		
		set @UniqueIdValue = @UniqueIdValue + @BlockAmount
		
		-- check to see if we had an overflow
		if @UniqueIdValue > @MaxIdValue
		begin
			if @RolloverIdValue is null -- we have an overflow
			begin
				-- raise error
				raiserror('usp_UniqueIdsGetNextBlock:: Overflow for key: %d; MaxIdValue: %d.  No overflow value assigned.', 16, 1, @UniqueIdKey, @MaxIdValue)
				-- rollback and return
				rollback tran
				return
			end -- we have a rollover value
			else set @UniqueIdValue = @RolloverIdValue + @BlockAmount
		end
			
		update B1.UniqueIds
		set UniqueIdValue = @UniqueIdValue
		where UniqueIdKey = @UniqueIdKey

	commit tran
end
GO
grant execute on B1.usp_UniqueIdsGetNextBlock to public
go