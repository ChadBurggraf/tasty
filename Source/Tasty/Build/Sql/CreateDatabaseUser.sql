-- 0: Login name

IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = N'{0}')
	CREATE USER [{0}] FOR LOGIN [{0}];

EXEC sp_addrolemember N'db_owner', N'{0}';