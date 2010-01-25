-- 0: Database name
-- 1: Login name
-- 2: Login password

IF NOT EXISTS ( SELECT NULL FROM master.dbo.syslogins WHERE name = '{1}' )
	CREATE LOGIN [{1}] WITH PASSWORD=N'{2}', DEFAULT_DATABASE=[{0}], CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF;