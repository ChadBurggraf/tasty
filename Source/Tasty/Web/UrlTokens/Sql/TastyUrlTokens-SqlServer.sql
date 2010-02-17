IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[TastyUrlToken]') AND type IN (N'U'))
	DROP TABLE [TastyUrlToken]
GO
CREATE TABLE [TastyUrlToken]
(
	[Key] varchar(64) NOT NULL,
	[Type] varchar(512) NOT NULL,
	[Data] xml NOT NULL,
	[Created] datetime NOT NULL,
	[Expires] datetime NOT NULL,
	CONSTRAINT [PK_TastyUrlToken] PRIMARY KEY CLUSTERED ([Key] ASC)
	WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO