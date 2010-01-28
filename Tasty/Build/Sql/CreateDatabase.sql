﻿-- 0: Database name
-- 1: Data files path

IF db_id('{0}') IS NULL
	CREATE DATABASE [{0}] ON PRIMARY 
	(
		FILENAME = '{1}\{0}.mdf', 
		NAME = N'{0}',
		SIZE = 2048KB, 
		FILEGROWTH = 1024KB 
	)
	LOG ON 
	( 
		FILENAME = '{1}\{0}_log.ldf',
		NAME = N'{0}_log', 
		SIZE = 1024KB, 
		FILEGROWTH = 10%
	);

EXEC dbo.sp_dbcmptlevel @dbname=N'{0}', @new_cmptlevel=90;

IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
BEGIN
EXEC [{0}].[dbo].[sp_fulltext_database] @action = 'enable'
END;

ALTER DATABASE [{0}] SET ANSI_NULL_DEFAULT OFF; 

ALTER DATABASE [{0}] SET ANSI_NULLS OFF; 

ALTER DATABASE [{0}] SET ANSI_PADDING OFF; 

ALTER DATABASE [{0}] SET ANSI_WARNINGS OFF; 

ALTER DATABASE [{0}] SET ARITHABORT OFF; 

ALTER DATABASE [{0}] SET AUTO_CLOSE OFF; 

ALTER DATABASE [{0}] SET AUTO_CREATE_STATISTICS ON; 

ALTER DATABASE [{0}] SET AUTO_SHRINK OFF; 

ALTER DATABASE [{0}] SET AUTO_UPDATE_STATISTICS ON; 

ALTER DATABASE [{0}] SET CURSOR_CLOSE_ON_COMMIT OFF; 

ALTER DATABASE [{0}] SET CURSOR_DEFAULT  GLOBAL; 

ALTER DATABASE [{0}] SET CONCAT_NULL_YIELDS_NULL OFF; 

ALTER DATABASE [{0}] SET NUMERIC_ROUNDABORT OFF; 

ALTER DATABASE [{0}] SET QUOTED_IDENTIFIER OFF; 

ALTER DATABASE [{0}] SET RECURSIVE_TRIGGERS OFF; 

ALTER DATABASE [{0}] SET AUTO_UPDATE_STATISTICS_ASYNC OFF; 

ALTER DATABASE [{0}] SET DATE_CORRELATION_OPTIMIZATION OFF; 

ALTER DATABASE [{0}] SET PARAMETERIZATION SIMPLE; 

ALTER DATABASE [{0}] SET  READ_WRITE; 

ALTER DATABASE [{0}] SET RECOVERY FULL; 

ALTER DATABASE [{0}] SET  MULTI_USER; 

ALTER DATABASE [{0}] SET PAGE_VERIFY CHECKSUM;  

IF NOT EXISTS (SELECT name FROM [{0}].sys.filegroups WHERE is_default=1 AND name = N'PRIMARY') 
	ALTER DATABASE [{0}] MODIFY FILEGROUP [PRIMARY] DEFAULT;