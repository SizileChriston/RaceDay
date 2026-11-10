-- Part 3 stores image URLs returned by the Azure Blob Storage API service.
-- Both columns are optional so existing users and events remain unchanged.
IF COL_LENGTH('dbo.Events', 'BannerImageUrl') IS NULL
    ALTER TABLE dbo.Events ADD BannerImageUrl nvarchar(2048) NULL;
GO

IF COL_LENGTH('dbo.Users', 'ProfilePictureUrl') IS NULL
    ALTER TABLE dbo.Users ADD ProfilePictureUrl nvarchar(2048) NULL;
GO

-- If EF Core already tracks this database, mark this migration as applied so
-- a later `dotnet ef database update` will not add these columns a second time.
IF OBJECT_ID(N'dbo.__EFMigrationsHistory', N'U') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1 FROM dbo.__EFMigrationsHistory
       WHERE MigrationId = N'20261111001500_AddRaceDayImageUrls'
   )
    INSERT INTO dbo.__EFMigrationsHistory (MigrationId, ProductVersion)
    VALUES (N'20261111001500_AddRaceDayImageUrls', N'8.0.13');
GO
