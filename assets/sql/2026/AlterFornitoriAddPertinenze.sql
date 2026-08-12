IF COL_LENGTH('dbo.Fornitori', 'usaPerAccessioni') IS NULL
BEGIN
    ALTER TABLE [dbo].[Fornitori]
    ADD [usaPerAccessioni] [bit] NOT NULL
        CONSTRAINT [DF_Fornitori_usaPerAccessioni] DEFAULT ((1)) WITH VALUES;
END
GO

IF COL_LENGTH('dbo.Fornitori', 'usaPerInterventiAlberi') IS NULL
BEGIN
    ALTER TABLE [dbo].[Fornitori]
    ADD [usaPerInterventiAlberi] [bit] NOT NULL
        CONSTRAINT [DF_Fornitori_usaPerInterventiAlberi] DEFAULT ((1)) WITH VALUES;
END
GO
