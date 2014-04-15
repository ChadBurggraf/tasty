IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Levenshtein]') AND type IN (N'FN', N'IF', N'TF', N'FS', N'FT'))
	DROP FUNCTION [Levenshtein]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[RegexIsMatch]') AND type IN (N'FN', N'IF', N'TF', N'FS', N'FT'))
	DROP FUNCTION [RegexIsMatch]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[RegexReplace]') AND type IN (N'FN', N'IF', N'TF', N'FS', N'FT'))
	DROP FUNCTION [RegexReplace]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[RegexSplit]') AND type IN (N'FN', N'IF', N'TF', N'FS', N'FT'))
	DROP FUNCTION [RegexSplit]
GO
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
	@TastyAssemblyPath nvarchar(256), 
	@Success bit,
	@ErrorMessage nvarchar(4000),
	@ErrorSeverity int
	
SELECT 
	@TastyAssemblyPath = '{0}',
	@Success = 0

IF @TastyAssemblyPath NOT LIKE N'%Tasty.SqlServer.dll'
	SET @TastyAssemblyPath = N'C:\Program Files\Tasty\SqlServer\Tasty.SqlServer.dll'

BEGIN TRY
	CREATE ASSEMBLY [Tasty.SqlServer]
	AUTHORIZATION [dbo] 
	FROM @TastyAssemblyPath
	WITH PERMISSION_SET = UNSAFE
	
	SET @Success = 1
END TRY
BEGIN CATCH
	SELECT
		@ErrorMessage = ERROR_MESSAGE(),
		@ErrorSeverity = ERROR_SEVERITY()
END CATCH

IF @Success = 0
BEGIN
	BEGIN TRY
		SET @TastyAssemblyPath = N'C:\Program Files (x86)\Tasty\SqlServer\Tasty.SqlServer.dll'
		
		CREATE ASSEMBLY [Tasty.SqlServer]
		AUTHORIZATION [dbo] 
		FROM @TastyAssemblyPath
		WITH PERMISSION_SET = UNSAFE
		
		SET @Success = 1
	END TRY
	BEGIN CATCH
		SELECT
			@ErrorMessage = ERROR_MESSAGE(),
			@ErrorSeverity = ERROR_SEVERITY()
	END CATCH
END

IF @Success = 0
	RAISERROR(@ErrorMessage, @ErrorSeverity, 18)
GO

CREATE FUNCTION [Levenshtein]
(
	@First nvarchar(max),
	@Second nvarchar(max)
)
RETURNS float
AS
EXTERNAL NAME [Tasty.SqlServer].[Tasty.SqlServer.ClrFunctions].[Levenshtein]
GO

CREATE FUNCTION [RegexIsMatch]
(
	@Input nvarchar(max),
	@Pattern nvarchar(max),
	@IgnoreCase bit,
	@Multiline bit
)
RETURNS bit
AS
EXTERNAL NAME [Tasty.SqlServer].[Tasty.SqlServer.ClrFunctions].[RegexIsMatch]
GO

CREATE FUNCTION [RegexReplace]
(
	@Input nvarchar(max),
	@Pattern nvarchar(max),
	@Replacement nvarchar(max),
	@IgnoreCase bit,
	@Multiline bit
)
RETURNS nvarchar(max)
EXTERNAL NAME [Tasty.SqlServer].[Tasty.SqlServer.ClrFunctions].[RegexReplace]
GO

CREATE FUNCTION [RegexSplit]
(
	@Input nvarchar(max),
	@Pattern nvarchar(max),
	@IgnoreCase bit,
	@Multiline bit
)
RETURNS TABLE
(
	[Value] nvarchar(max)
)
AS
EXTERNAL NAME [Tasty.SqlServer].[Tasty.SqlServer.ClrFunctions].[RegexSplit]
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

EXEC sp_configure 'clr enabled', 1
GO
RECONFIGURE
GO