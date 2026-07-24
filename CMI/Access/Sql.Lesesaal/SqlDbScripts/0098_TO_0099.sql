/* ---------------------------------------------------------------------- */
/* Alter SyncAction view to use Outer Apply to improve perfomance         */
/* ---------------------------------------------------------------------- */

CREATE OR ALTER View v_SyncAction
AS
SELECT
    s.SyncActionId,
    s.ArchiveRecordId,
    s.ActionType,
    s.ActionStatus,
    s.NumberOfTries,
    s.CreatedOn,
    s.ModifiedOn,
    sal.SyncActionLogId,
    sal.LogDate,
    sal.ErrorReason,
    CASE
        WHEN sal.ErrorReason IS NULL THEN sal.ActionStatusHistory
        ELSE CONCAT(sal.ActionStatusHistory, ' - ', sal.ErrorReason)
        END AS ActionStatusHistory
FROM SyncAction s
    OUTER APPLY (
    SELECT TOP 1 
        l.SyncActionLogId,
        l.LogDate,
        l.ErrorReason,
        l.ActionStatusHistory
    FROM SyncActionLog l
    WHERE l.SyncActionId = s.SyncActionId
    ORDER BY l.SyncActionLogId DESC
) sal     
GO

CREATE OR ALTER VIEW dbo.v_SyncNumberPerHour
AS
SELECT
    ROW_NUMBER() OVER (
        ORDER BY
            CONVERT(DATE, ISNULL(ModifiedOn, CreatedOn)),
            FORMAT(ISNULL(ModifiedOn, CreatedOn), 'yyyy-MM-dd HH'),
            ActionStatus
    ) AS Id,
    FORMAT(ISNULL(ModifiedOn, CreatedOn), 'yyyy-MM-dd HH') AS LastModified,
    CONVERT(DATE, ISNULL(ModifiedOn, CreatedOn)) AS LastModifiedDay,
    COUNT(SyncActionId) AS RecordCount,
    ActionStatus
FROM SyncAction
GROUP BY
    FORMAT(ISNULL(ModifiedOn, CreatedOn), 'yyyy-MM-dd HH'),
    CONVERT(DATE, ISNULL(ModifiedOn, CreatedOn)),
    ActionStatus;
GO