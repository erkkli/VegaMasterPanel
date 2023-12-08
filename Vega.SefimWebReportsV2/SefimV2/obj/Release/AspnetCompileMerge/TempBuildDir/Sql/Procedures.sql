IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'subebazli')
DROP PROCEDURE subebazli
GO
CREATE PROCEDURE subebazli(@myt nvarchar(255), @sqlcmd NVARCHAR(max),@servers NVARCHAR(max) = NULL) AS 
-- Description: ortak sql calistirmak
-- Version: 1.1	 Update Date : 21.09.2018 00:11
BEGIN TRY
exec ('DROP TABLE '+@myt)
END TRY
BEGIN CATCH
END CATCH



DECLARE @tmp nvarchar(max)
SET @tmp = 'SELECT CAST(''local'' AS nvarchar(255)) as ServerName, * INTO '+@myt+' FROM (' +@sqlcmd +') AS V'
PRINT 'LOKALDE CALISACAK:'
PRINT @tmp
EXEC (@tmp)


DECLARE @linkedservers CURSOR
DECLARE	@servername	nvarchar(128)
DECLARE	@defcatalog	nvarchar(128)
SET @linkedservers = CURSOR FOR SELECT name,catalogname FROM msdb..oldactiveservers where catalogname IS NOT NULL

OPEN @linkedservers
FETCH NEXT FROM @linkedservers INTO @servername, @defcatalog
WHILE @@FETCH_STATUS = 0
BEGIN

	SET @tmp = 'INSERT INTO '+@myt+' SELECT '''+@servername+''', * FROM OPENQUERY(['+@servername+'],'''+ REPLACE(@sqlcmd,'''','''''')+''')'
	
	PRINT @tmp


	BEGIN TRY
		IF @servers IS NULL 
		BEGIN 
			exec (@tmp)
		END
		ELSE 
		BEGIN
			IF CHARINDEX(@servername,@servers)>0
			BEGIN
				exec (@tmp)
			END
		END

	END TRY
	BEGIN CATCH
		PRINT ERROR_MESSAGE() 
	END CATCH

	FETCH NEXT FROM @linkedservers INTO @servername, @defcatalog
END

GO

USE msdb
GO
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'updateactiveservers')
DROP PROCEDURE updateactiveservers
GO
CREATE PROCEDURE updateactiveservers AS 

IF object_id('activeservers') is null 
BEGIN
create table activeservers (
  name  nvarchar(128),
  catalogname  nvarchar(128),
  active int
 )
END
DELETE FROM activeservers

IF object_id('oldactiveservers') is null 
BEGIN
create table oldactiveservers (
  name  nvarchar(128),
  catalogname  nvarchar(128),
  active int
 )
END

DECLARE @myt nvarchar(255)
SET @myt = 'activeservers'
DECLARE @sqlcmd NVARCHAR(max) 
SET @sqlcmd='SELECT 1 as active'
DECLARE @servers NVARCHAR(max)
SET @servers  = NULL

BEGIN TRY
exec ('DELETE '+@myt)
END TRY
BEGIN CATCH
END CATCH

create table #linkedservers (
  name  nvarchar(128),
  name2  nvarchar(128),
  name3  nvarchar(128),
  name4  nvarchar(128),
  name5  nvarchar(128),
  name6  nvarchar(128),
  catalogname  nvarchar(128)
 )
insert into #linkedservers execute sp_linkedservers 
DECLARE @linkedservers CURSOR
DECLARE	@servername	nvarchar(128)
DECLARE	@defcatalog	nvarchar(128)
DECLARE	@tmp nvarchar(max)
SET @linkedservers = CURSOR FOR SELECT name,catalogname FROM #linkedservers where catalogname IS NOT NULL

OPEN @linkedservers
FETCH NEXT FROM @linkedservers INTO @servername, @defcatalog
WHILE @@FETCH_STATUS = 0
BEGIN
	SET @tmp = 'INSERT INTO '+@myt+' SELECT '''+@servername+''','''+@defcatalog+''',  * 
	FROM OPENQUERY(['+@servername+'],'''+ REPLACE(@sqlcmd,'''','''''')+''')'
	
	PRINT @tmp

	BEGIN TRY
		IF @servers IS NULL 
		BEGIN 
			exec (@tmp)
		END
		ELSE 
		BEGIN
			IF CHARINDEX(@servername,@servers)>0
			BEGIN
				exec (@tmp)
			END
		END

	END TRY
	BEGIN CATCH
		PRINT ERROR_MESSAGE() 
	END CATCH

	FETCH NEXT FROM @linkedservers INTO @servername, @defcatalog
END

DELETE FROM oldactiveservers
INSERT INTO oldactiveservers (name ,catalogname, active) SELECT DISTINCT name ,catalogname, active FROM activeservers

GO

--exec updateactiveservers

--GO

--GO
--	EXEC msdb.dbo.sp_delete_job @job_name=N'UpdateActiveServersJob', @delete_unused_schedule=1
--GO
--Declare @job_id uniqueidentifier,
--           @schedule_id int

--EXEC dbo.sp_add_job
--    @job_name = N'UpdateActiveServersJob',
--    @job_id = @job_id Output;

--EXEC sp_add_jobstep
--    @job_name = N'UpdateActiveServersJob',
--    @step_name = N'Checking status of servers',
--    @database_name = N'msdb',
--    @command = N'exec updateactiveservers';

--EXEC dbo.sp_add_schedule
--    @schedule_name = N'RunEveryMinute',@freq_type  = 4,
--    @freq_interval = 1,
--    @freq_subday_type  = 4,
--    @freq_subday_interval = 5,
--     @schedule_id = @schedule_id Output;

--EXEC dbo.sp_add_jobserver
--    @job_name = N'UpdateActiveServersJob' ;

--Exec dbo.sp_attach_schedule @job_id = @job_id, @schedule_id = @schedule_id;