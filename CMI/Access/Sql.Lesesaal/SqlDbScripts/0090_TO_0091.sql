ALTER TABLE SyncInfo DROP CONSTRAINT [PK_SyncInfo]
GO

exec sp_rename 'SyncInfo', 'SyncInfoOld'
GO

CREATE TABLE [SyncInfo] (
    [SyncInfoId] BIGINT IDENTITY(1,1) NOT NULL,
	[LastSequenceNumber] BIGINT,
	CONSTRAINT [PK_SyncInfo] PRIMARY KEY CLUSTERED ([SyncInfoId])
) 
GO

insert into SyncInfo(LastSequenceNumber) select LastSequenceNumber from SyncInfoOld
GO

DROP TABLE SyncInfoOld
GO