IF COL_LENGTH('dbo.Accessioni', 'visualizzaLocalitaWeb') IS NULL
BEGIN
    ALTER TABLE [dbo].[Accessioni]
    ADD [visualizzaLocalitaWeb] bit NOT NULL
        CONSTRAINT [DF_Accessioni_visualizzaLocalitaWeb] DEFAULT (1);
END
GO

UPDATE [dbo].[Accessioni]
SET [visualizzaLocalitaWeb] = 1
WHERE [visualizzaLocalitaWeb] IS NULL;
GO
