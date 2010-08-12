IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[TastyJob]') AND type IN (N'U'))
	DROP TABLE [TastyJob]
GO
CREATE TABLE [TastyJob]
(
	[Id] int NOT NULL IDENTITY(1, 1),
	[Name] varchar(128) NOT NULL,
	[Type] varchar(512) NOT NULL,
	[Data] xml NOT NULL,
	[Status] varchar(12) NOT NULL,
	[Exception] xml NULL,
	[QueueDate] datetime NOT NULL,
	[StartDate] datetime NULL,
	[FinishDate] datetime NULL,
	[ScheduleName] varchar(128) NULL,
	CONSTRAINT [PK_TastyJob] PRIMARY KEY CLUSTERED ([Id] ASC)
	WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO