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

        -- Guid dell'entità padre (Accessioni/Individui)
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
            CONSTRAINT DF_Documenti_dataInserimento DEFAULT GETDATE(),

        visibile BIT NOT NULL
            CONSTRAINT DF_Documenti_visibile DEFAULT (0),

        CONSTRAINT CK_Documenti_tipoEntita CHECK (tipoEntita IN (N'Accessione', N'Individuo')),
        CONSTRAINT CK_Documenti_dimensioneBytes CHECK (dimensioneBytes >= 0)
    );

    CREATE INDEX IX_Documenti_tipoEntita_entitaId_data
        ON dbo.Documenti (tipoEntita, entitaId, dataInserimento DESC);

    CREATE UNIQUE INDEX UX_Documenti_tipoEntita_entitaId_nomefileFisico
        ON dbo.Documenti (tipoEntita, entitaId, nomefileFisico);
END;
GO
