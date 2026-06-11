IF COL_LENGTH('dbo.Accessioni', 'vouchercode') IS NULL
BEGIN
    ALTER TABLE [dbo].[Accessioni]
    ADD [vouchercode] varchar(100) NULL;
END
GO
