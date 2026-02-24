-- UPlant - tabella documenti unificata (Accessioni + Individui)
-- SQL Server

IF OBJECT_ID('dbo.documenti', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.documenti
    (
        id UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT PK_documenti PRIMARY KEY
            CONSTRAINT DF_documenti_id DEFAULT NEWID(),

        -- Contesto del documento: Accessione o Individuo
        tipoEntita NVARCHAR(30) NOT NULL,

        -- Guid dell'entità a cui il documento appartiene
        entitaId UNIQUEIDENTIFIER NOT NULL,

        -- Nome originale file lato utente
        nomefile NVARCHAR(255) NOT NULL,

        -- Nome fisico salvato su disco (es: {documentoId}.pdf)
        nomefileFisico NVARCHAR(255) NOT NULL,

        estensione NVARCHAR(10) NULL,
        mimeType NVARCHAR(100) NULL,

        -- Dimensione in byte
        dimensioneBytes BIGINT NOT NULL,

        descrizione NVARCHAR(500) NULL,
        credits NVARCHAR(250) NULL,
        autore NVARCHAR(100) NULL,

        dataInserimento DATETIME NOT NULL
            CONSTRAINT DF_documenti_dataInserimento DEFAULT GETDATE(),

        visibile BIT NOT NULL
            CONSTRAINT DF_documenti_visibile DEFAULT (0),

        CONSTRAINT CK_documenti_tipoEntita CHECK (tipoEntita IN (N'Accessione', N'Individuo')),
        CONSTRAINT CK_documenti_dimensioneBytes CHECK (dimensioneBytes >= 0)
    );

    -- Indice principale per le query per contesto + entità (Details Accessione/Individuo)
    CREATE INDEX IX_documenti_tipoEntita_entitaId_data
        ON dbo.documenti (tipoEntita, entitaId, dataInserimento DESC);

    -- Evita duplicati fisici per la stessa entità
    CREATE UNIQUE INDEX UX_documenti_tipoEntita_entitaId_nomefileFisico
        ON dbo.documenti (tipoEntita, entitaId, nomefileFisico);
END;
GO
