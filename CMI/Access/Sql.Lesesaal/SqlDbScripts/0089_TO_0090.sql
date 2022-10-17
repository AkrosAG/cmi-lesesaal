CREATE TABLE [SyncInfo] (
	[LastSequenceNumber] BIGINT NOT NULL,
	CONSTRAINT [PK_SyncInfo] PRIMARY KEY CLUSTERED ([LastSequenceNumber])
) 
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Enthält die letzte verarbeitete Sequenz Nummer des CDWS.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SyncInfo'
GO