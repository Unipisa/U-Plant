-- UPlant - tabella documenti unificata (Accessioni + Individui)
-- SQL Server

IF OBJECT_ID('dbo.Documenti', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Documenti
    (
        id UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT PK_Documenti PRIMARY KEY
            CONSTRAINT DF_Documenti_id DEFAULT NEWID(),

        -- Contesto del documento: Accessione o Individuo
        tipoEntita NVARCHAR(30) NOT NULL,

        -- FK opzionali (una valorizzata in base al tipoEntita)
        accessioneId UNIQUEIDENTIFIER NULL,
        individuoId UNIQUEIDENTIFIER NULL,

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
            CONSTRAINT DF_Documenti_dataInserimento DEFAULT GETDATE(),

        visibile BIT NOT NULL
            CONSTRAINT DF_Documenti_visibile DEFAULT (0),

        CONSTRAINT FK_Documenti_Accessioni
            FOREIGN KEY (accessioneId) REFERENCES dbo.Accessioni(id),

        CONSTRAINT FK_Documenti_Individui
            FOREIGN KEY (individuoId) REFERENCES dbo.Individui(id),

        CONSTRAINT CK_Documenti_tipoEntita CHECK (tipoEntita IN (N'Accessione', N'Individuo')),
        CONSTRAINT CK_Documenti_dimensioneBytes CHECK (dimensioneBytes >= 0),
        CONSTRAINT CK_Documenti_parent CHECK (
            (tipoEntita = N'Accessione' AND accessioneId IS NOT NULL AND individuoId IS NULL)
            OR
            (tipoEntita = N'Individuo' AND individuoId IS NOT NULL AND accessioneId IS NULL)
        )
    );

    -- Indice principale per le query per contesto + entità (Details Accessione/Individuo)
    CREATE INDEX IX_Documenti_tipoEntita_accessioneId_individuoId_data
        ON dbo.Documenti (tipoEntita, accessioneId, individuoId, dataInserimento DESC);

    -- Evita duplicati fisici per la stessa entità (filtrati)
    CREATE UNIQUE INDEX UX_Documenti_Accessione_nomefileFisico
        ON dbo.Documenti (accessioneId, nomefileFisico)
        WHERE accessioneId IS NOT NULL;

    CREATE UNIQUE INDEX UX_Documenti_Individuo_nomefileFisico
        ON dbo.Documenti (individuoId, nomefileFisico)
        WHERE individuoId IS NOT NULL;
END;
GO
