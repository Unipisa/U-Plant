IF OBJECT_ID(N'dbo.StatusNomenclaturale', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.ValidazioneTassonomica', N'U') IS NULL
BEGIN
    EXEC sp_rename 'dbo.StatusNomenclaturale', 'ValidazioneTassonomica';
END;

IF OBJECT_ID(N'dbo.ValidazioneTassonomica', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ValidazioneTassonomica
    (
        id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ValidazioneTassonomica PRIMARY KEY DEFAULT NEWID(),
        descrizione VARCHAR(100) NOT NULL,
        descrizione_en VARCHAR(100) NULL,
        ordinamento INT NOT NULL CONSTRAINT DF_ValidazioneTassonomica_Ordinamento DEFAULT (0),
        organizzazione UNIQUEIDENTIFIER NOT NULL
    );

    ALTER TABLE dbo.ValidazioneTassonomica
        ADD CONSTRAINT FK_ValidazioneTassonomica_Organizzazioni
        FOREIGN KEY (organizzazione) REFERENCES dbo.Organizzazioni(id);
END;

IF COL_LENGTH('dbo.Specie', 'status_nomenclaturale') IS NOT NULL
   AND COL_LENGTH('dbo.Specie', 'validazione_tassonomica') IS NULL
BEGIN
    EXEC sp_rename 'dbo.Specie.status_nomenclaturale', 'validazione_tassonomica', 'COLUMN';
END;

IF COL_LENGTH('dbo.Specie', 'validazione_tassonomica') IS NULL
BEGIN
    ALTER TABLE dbo.Specie ADD validazione_tassonomica UNIQUEIDENTIFIER NULL;
END;

IF COL_LENGTH('dbo.Specie', 'data_inserimento') IS NULL
   AND COL_LENGTH('dbo.Specie', 'data') IS NOT NULL
BEGIN
    EXEC sp_rename 'dbo.Specie.data', 'data_inserimento', 'COLUMN';
END;

IF COL_LENGTH('dbo.Specie', 'data_inserimento') IS NULL
BEGIN
    ALTER TABLE dbo.Specie ADD data_inserimento DATETIME NULL;
END;

IF COL_LENGTH('dbo.Specie', 'lsid') IS NULL
BEGIN
    ALTER TABLE dbo.Specie ADD lsid VARCHAR(255) NULL;
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_Specie_ValidazioneTassonomica'
)
BEGIN
    IF EXISTS (
        SELECT 1
        FROM sys.foreign_keys
        WHERE name = 'FK_Specie_StatusNomenclaturale'
    )
    BEGIN
        ALTER TABLE dbo.Specie DROP CONSTRAINT FK_Specie_StatusNomenclaturale;
    END;

    ALTER TABLE dbo.Specie
        ADD CONSTRAINT FK_Specie_ValidazioneTassonomica
        FOREIGN KEY (validazione_tassonomica) REFERENCES dbo.ValidazioneTassonomica(id);
END;

INSERT INTO dbo.ValidazioneTassonomica (id, descrizione, descrizione_en, ordinamento, organizzazione)
SELECT NEWID(), seed.descrizione, seed.descrizione, seed.ordinamento, o.id
FROM dbo.Organizzazioni o
CROSS JOIN (
    SELECT 'N.D.' AS descrizione, 10 AS ordinamento
    UNION ALL
    SELECT 'WFO', 20
    UNION ALL
    SELECT 'A.A.', 30
) seed
WHERE NOT EXISTS (
    SELECT 1
    FROM dbo.ValidazioneTassonomica s
    WHERE s.organizzazione = o.id
      AND s.descrizione = seed.descrizione
);

EXEC(N'
UPDATE sp
SET sp.data_inserimento = ISNULL(sp.data_inserimento, GETDATE())
FROM dbo.Specie sp
WHERE sp.data_inserimento IS NULL;
');

UPDATE sp
SET sp.validazione_tassonomica = s.id
FROM dbo.Specie sp
INNER JOIN dbo.Accessioni a ON a.specie = sp.id
INNER JOIN dbo.ValidazioneTassonomica s ON s.organizzazione = a.organizzazione AND s.descrizione = 'N.D.'
WHERE sp.validazione_tassonomica IS NULL;

UPDATE sp
SET sp.validazione_tassonomica = s.id
FROM dbo.Specie sp
CROSS APPLY (
    SELECT TOP 1 sn.id
    FROM dbo.ValidazioneTassonomica sn
    WHERE sn.descrizione = 'N.D.'
    ORDER BY sn.ordinamento, sn.descrizione
) s
WHERE sp.validazione_tassonomica IS NULL;

IF EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.Specie')
      AND name = 'data_inserimento'
      AND is_nullable = 1
)
BEGIN
    EXEC(N'ALTER TABLE dbo.Specie ALTER COLUMN data_inserimento DATETIME NOT NULL;');
END;

IF EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.Specie')
      AND name = 'validazione_tassonomica'
      AND is_nullable = 1
)
BEGIN
    ALTER TABLE dbo.Specie ALTER COLUMN validazione_tassonomica UNIQUEIDENTIFIER NOT NULL;
END;
