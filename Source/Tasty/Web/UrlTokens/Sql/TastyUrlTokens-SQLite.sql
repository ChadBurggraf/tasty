﻿CREATE TABLE IF NOT EXISTS [TastyUrlToken]
(
	[Key] VARCHAR(64) NOT NULL PRIMARY KEY,
	[Type] VARCHAR(512) NOT NULL,
	[Data] TEXT NOT NULL,
	[Created] TIMESTAMP NOT NULL,
	[Expires] TIMESTAMP NOT NULL
);

CREATE INDEX IF NOT EXISTS [IX_TastyUrlToken_QueuedOn]
ON [TastyUrlToken]([Expires] ASC);