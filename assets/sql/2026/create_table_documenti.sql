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
        AccessioneId UNIQUEIDENTIFIER NULL,
        IndividuoId UNIQUEIDENTIFIER NULL,

        -- Nome originale file lato utente
        nomefile NVARCHAR(255) NOT NULL,

        -- Nome fisico salvato su disco (es: {documentoId}.pdf)
        nomefileFisico NVARCHAR(255) NOT NULL,

        estensione NVARCHAR(10) NULL,
        mimeType NVARCHAR(100) NULL,

        -- Dimensione in byte
        dimensioneBytes BIGINT NOT NULL,

        descrizione NVARCHAR(500) NULL,
        utenteAcquisizione UNIQUEIDENTIFIER NOT NULL,

        dataInserimento DATETIME NOT NULL
            CONSTRAINT DF_Documenti_dataInserimento DEFAULT GETDATE(),

        CONSTRAINT FK_Documenti_Accessioni
            FOREIGN KEY (AccessioneId) REFERENCES dbo.Accessioni(id),

        CONSTRAINT FK_Documenti_Individui
            FOREIGN KEY (IndividuoId) REFERENCES dbo.Individui(id),

        CONSTRAINT FK_Documenti_Users
            FOREIGN KEY (utenteAcquisizione) REFERENCES dbo.Users(Id),

        CONSTRAINT CK_Documenti_tipoEntita CHECK (tipoEntita IN (N'Accessione', N'Individuo')),
        CONSTRAINT CK_Documenti_dimensioneBytes CHECK (dimensioneBytes >= 0),
        CONSTRAINT CK_Documenti_parent CHECK (
            (tipoEntita = N'Accessione' AND AccessioneId IS NOT NULL AND IndividuoId IS NULL)
            OR
            (tipoEntita = N'Individuo' AND IndividuoId IS NOT NULL AND AccessioneId IS NULL)
        )
    );

    -- Indice principale per le query per contesto + entità (Details Accessione/Individuo)
    CREATE INDEX IX_Documenti_tipoEntita_AccessioneId_IndividuoId_data
        ON dbo.Documenti (tipoEntita, AccessioneId, IndividuoId, dataInserimento DESC);

    -- Evita duplicati fisici per la stessa entità (filtrati)
    CREATE UNIQUE INDEX UX_Documenti_Accessione_nomefileFisico
        ON dbo.Documenti (AccessioneId, nomefileFisico)
        WHERE AccessioneId IS NOT NULL;

    CREATE UNIQUE INDEX UX_Documenti_Individuo_nomefileFisico
        ON dbo.Documenti (IndividuoId, nomefileFisico)
        WHERE IndividuoId IS NOT NULL;
END;
GO
