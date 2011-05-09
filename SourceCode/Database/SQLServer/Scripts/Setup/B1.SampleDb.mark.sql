use Master
go
declare @dbName varchar(24)
set @dbName = 'B1Sample'

-- if database exists, kill processes and drop database
if exists (select null from sys.databases where name = @dbName)
	begin
		declare @SPID int, @LOGIN nvarchar(50), @DB nvarchar(50), @CMD nvarchar(256)

		declare cUsers Cursor for
		select spid, loginame, d.[name] 
		from sysprocesses p 
		inner join sysdatabases d  on d.dbid = p.dbid 
		and upper(d.name) = upper(@dbName)

		OPEN cUsers
		FETCH NEXT FROM cUsers
		INTO @SPID, @LOGIN, @DB
		WHILE @@FETCH_STATUS = 0
		BEGIN
			PRINT 'Killing SPID: ' + cast(@SPID as nvarchar(50)) + ' LOGIN: ' 
				+ @LOGIN + ' IN DB: ' + @DB
			SET @CMD = 'KILL ' + cast(@SPID as nvarchar(20))
			EXEC sp_executesql @CMD
			FETCH NEXT FROM cUsers
			INTO @SPID, @LOGIN, @DB
		END

		CLOSE cUsers
		DEALLOCATE cUsers
		drop database B1Sample

	end

go
-- B1Sample database 
-- primary filegroup
-- should only contain the system objects
-- no user objects should be defined here
-- All B1Core objects will be created in a seperate filegroup
create database B1Sample on  primary 
( name = N'B1Sample'
, filename = N'C:\Program Files\Microsoft SQL Server\MSSQL10.MSSQLSERVER\MSSQL\DATA\B1Sample.mdf' 
, SIZE = 3048KB 
, MAXSIZE = UNLIMITED
, FILEGROWTH = 1024KB )

-- The filegroup for the B1Core (non index) objects
, filegroup B1Core
( name = N'B1Core'
, filename = N'C:\Program Files\Microsoft SQL Server\MSSQL10.MSSQLSERVER\MSSQL\DATA\B1Sample_B1Core.ndf' 
, SIZE = 2048KB 
, MAXSIZE = UNLIMITED
, FILEGROWTH = 1024KB )

-- The filegroup for the B1 Core Indexes
, filegroup B1CoreIdx 
( name = N'B1CoreIdx'
, filename = N'C:\Program Files\Microsoft SQL Server\MSSQL10.MSSQLSERVER\MSSQL\DATA\B1Sample_B1CoreIdx.ndf' 
, SIZE = 2048KB 
, MAXSIZE = UNLIMITED
, FILEGROWTH = 1024KB )

-- define a Log file
 LOG on 
( name = N'B1Sample_log'
, filename = N'C:\Program Files\Microsoft SQL Server\MSSQL10.MSSQLSERVER\MSSQL\DATA\B1Sample_log.ldf' 
, SIZE = 1024KB 
, MAXSIZE = 2048GB 
, FILEGROWTH = 10%)
go
--
-- create a B1 Schema
USE B1Sample
go
create schema B1 authorization dbo

-- Logins may already exist on the server, so we will check
--		if they are not, create them.
--
go
if not exists (select * from sys.server_principals where name = N'owner')
	create login owner with password = N'owner!'
	, default_database = B1Sample
	, default_language = us_english
	, check_expiration = off
	, check_policy = off

go
-- Create Database Users and match them to their Server logins and default Schemas
--
use  B1Sample
create user owner for login owner with default_schema = B1

--
-- Add server roles
--
go
sp_addrolemember 'db_ddladmin','owner'
go
sp_addrolemember 'db_datareader','owner'
go
sp_addrolemember 'db_datawriter','owner'
go
use master
go