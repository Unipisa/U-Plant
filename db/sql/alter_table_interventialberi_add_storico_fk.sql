-- Aggiunge il riferimento opzionale allo storico creato in chiusura intervento
IF COL_LENGTH('dbo.InterventiAlberi', 'storicoIndividuoId') IS NULL
BEGIN
    ALTER TABLE dbo.InterventiAlberi
    ADD storicoIndividuoId uniqueidentifier NULL;
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_InterventiAlberi_StoricoIndividuo'
)
BEGIN
    ALTER TABLE dbo.InterventiAlberi WITH CHECK
    ADD CONSTRAINT FK_InterventiAlberi_StoricoIndividuo
        FOREIGN KEY (storicoIndividuoId)
        REFERENCES dbo.StoricoIndividuo(id)
        ON DELETE SET NULL;
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_InterventiAlberi_storicoIndividuoId'
      AND object_id = OBJECT_ID('dbo.InterventiAlberi')
)
BEGIN
    CREATE INDEX IX_InterventiAlberi_storicoIndividuoId
        ON dbo.InterventiAlberi(storicoIndividuoId);
END
GO
