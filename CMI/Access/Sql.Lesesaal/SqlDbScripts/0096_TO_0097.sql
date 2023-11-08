ALTER TABLE [DownloadLog] 
ADD [Zeitraum] nvarchar(200) NULL;
GO

ALTER TABLE [DownloadToken] 
ALTER COLUMN [RecordId] nvarchar(255);
GO