IF EXISTS (SELECT * FROM sys.server_principals WHERE name = N'{0}')
	DROP LOGIN [{0}]