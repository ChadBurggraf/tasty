IF db_id('{0}') IS NOT NULL
	SELECT 1 AS [Exists]
ELSE
	SELECT 0 AS [Exists]