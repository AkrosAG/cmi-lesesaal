ALTER TABLE ApplicationUser DROP CONSTRAINT CHK_ROLEPUBLICCLIENT_VS_ISINTERNALUSER
GO

EXEC sp_rename 'dbo.ApplicationUser.BarInternalConsultation', 'InternalConsultation', 'COLUMN'
GO

UPDATE ApplicationUser SET RolePublicClient = 'EMA' WHERE RolePublicClient  IN ('BVW')
UPDATE ApplicationUser SET RolePublicClient = 'AMA' WHERE RolePublicClient  IN ('BAR')
GO

ALTER TABLE ApplicationUser ADD CONSTRAINT CHK_ROLEPUBLICCLIENT_VS_ISINTERNALUSER CHECK ((IsInternalUser = 1 AND RolePublicClient IN ('AMA', 'AS', 'EMA')) OR (IsInternalUser = 0 AND RolePublicClient IN ('Ö2', 'Ö3')))																				        
GO

