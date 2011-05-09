--
-- This is a script to remove all the objects associated with the given Schema
-- This script will not create a database and it will NOT drop the schema
--

declare @sql nvarchar(1024), @name varchar(64), @type varchar(4)
		, @schema varchar(12), @tableName varchar(64)
set @schema = 'B1'

declare c cursor for
select name, type, object_name(parent_object_id) as tableName from sys.objects 
where schema_id = schema_id(@schema)
and type in ('U', 'P', 'FN', 'F')
order by type

open c
fetch c into  @name, @type, @tableName
while @@fetch_status = 0
begin

	if @type = 'F'
		set @sql = 'alter table ' + @schema + '.' + @tableName + ' drop constraint ' + @name
	else if @type = 'U'
		set @sql = 'drop table ' + @schema + '.' + @name
	else if @type = 'P'
		set @sql = 'drop procedure ' + @schema + '.' + @name
	else set @sql = 'drop function ' + @schema + '.' + @name

	raiserror('Command: %s', 5, 1, @sql)
	exec sp_executesql @sql
	fetch c into  @name, @type, @tableName

end

close c
deallocate c
