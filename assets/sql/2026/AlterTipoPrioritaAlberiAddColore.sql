SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF COL_LENGTH('dbo.TipoPrioritaAlberi', 'colore') IS NULL
BEGIN
    ALTER TABLE [dbo].[TipoPrioritaAlberi]
    ADD [colore] [varchar](7) NOT NULL
        CONSTRAINT [DF_TipoPrioritaAlberi_colore] DEFAULT ('#6c757d');

    UPDATE [dbo].[TipoPrioritaAlberi]
    SET [colore] = '#6c757d'
    WHERE [colore] IS NULL OR [colore] = '';
END
GO
