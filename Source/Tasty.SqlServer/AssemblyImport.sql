IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[StringSetContainsAny]') AND type IN (N'FN', N'IF', N'TF', N'FS', N'FT'))
	DROP FUNCTION [StringSetContainsAny]
GO

DECLARE @Sql nvarchar(max)
SET @Sql = N'ALTER DATABASE [' + DB_NAME() + '] SET TRUSTWORTHY ON'
EXEC sp_executesql @statement = @Sql
GO

IF EXISTS (SELECT * FROM sys.assemblies WHERE [name] = 'Tasty.SqlServer')
	DROP ASSEMBLY [Tasty.SqlServer]
GO

DECLARE 
	@SystemAssembliesPath nvarchar(256),
	@TastyAssemblyPath nvarchar(256)

SELECT @SystemAssembliesPath = '{0}', @TastyAssemblyPath = '{1}'

IF @SystemAssembliesPath NOT LIKE N'%Framework\v3.5%'
	SET @SystemAssembliesPath = N'C:\Program Files\Reference Assemblies\Microsoft\Framework\v3.5'

IF @TastyAssemblyPath NOT LIKE N'%Tasty.SqlServer.dll'
	SET @TastyAssemblyPath = N'C:\Projects\Tasty\Build\Tasty.SqlServer.dll'

IF NOT EXISTS (SELECT * FROM sys.assemblies WHERE [name] = 'System.Core')
BEGIN
	CREATE ASSEMBLY [System.Core]
	AUTHORIZATION [dbo]
	FROM @SystemAssembliesPath + '\System.Core.dll'
	WITH PERMISSION_SET = UNSAFE
END

CREATE ASSEMBLY [Tasty.SqlServer]
AUTHORIZATION [dbo] 
FROM @TastyAssemblyPath
WITH PERMISSION_SET = UNSAFE
GO

CREATE FUNCTION [StringSetContainsAny]
(
	@ReferenceSet nvarchar(max),
	@AskingSet nvarchar(max),
	@Separator nvarchar(24)
)
RETURNS bit
AS
EXTERNAL NAME [Tasty.SqlServer].[Tasty.SqlServer.ClrFunctions].[StringSetContainsAny]
GO