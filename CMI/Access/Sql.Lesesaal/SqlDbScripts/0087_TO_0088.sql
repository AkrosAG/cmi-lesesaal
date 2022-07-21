DROP INDEX IF EXISTS [IX_ForCalcIndividualAccessTokens]
ON [OrderItem]
GO
DROP INDEX IF EXISTS [IX_PrimaerdatenAuftrag_4]
ON [PrimaerdatenAuftrag]
GO
DROP INDEX IF EXISTS [IDX_OrderExecutedWaitList_1]
ON [OrderExecutedWaitList]
GO



USE [Lesesaal]
ALTER TABLE [OrderItem] 
ALTER COLUMN [Ve] nvarchar(255) NOT  NULL;
GO
USE [Lesesaal]
ALTER TABLE [Favorite] 
ALTER COLUMN [Ve] nvarchar(255);
GO
USE [Lesesaal]
ALTER TABLE [PrimaerdatenAuftrag] 
ALTER COLUMN [VeId] nvarchar(255) NOT  NULL;
GO
USE [Lesesaal]
ALTER TABLE [DownloadReasonHistory] 
ALTER COLUMN [VeId] nvarchar(255);
GO
USE [Lesesaal]
ALTER TABLE [OrderExecutedWaitList] 
ALTER COLUMN [VeId] nvarchar(255);

GO
CREATE NONCLUSTERED INDEX [IDX_OrderExecutedWaitList_1] ON [dbo].[OrderExecutedWaitList] ([VeId])
GO

CREATE NONCLUSTERED INDEX [IX_PrimaerdatenAuftrag_4] ON [PrimaerdatenAuftrag] ([Abgeschlossen] ASC, [VeId] ASC, [Status] ASC)
GO

CREATE NONCLUSTERED INDEX [IX_ForCalcIndividualAccessTokens] ON [dbo].[OrderItem]
(
	[Ve] ASC,
	[ApproveStatus] ASC,
	[EntscheidGesuch] ASC
)
