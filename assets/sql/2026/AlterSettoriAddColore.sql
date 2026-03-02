ALTER TABLE [dbo].[Settori]
ADD [colore] varchar(20) NULL;
GO

UPDATE [dbo].[Settori]
SET [colore] = '#6c757d'
WHERE [colore] IS NULL;
GO
