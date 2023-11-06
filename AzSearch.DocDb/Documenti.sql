CREATE TABLE [dbo].[Documenti]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY,
	[Guid] UNIQUEIDENTIFIER NOT NULL, 
    [Link] NVARCHAR(MAX) NULL, 
    [Titolo] NVARCHAR(50) NULL,
    [Argomento] NVARCHAR(50) NULL, 
    [DataCreazione] DATETIMEOFFSET NULL,
    [ThumbnailLink] NVARCHAR(MAX) NULL
)

GO

CREATE INDEX [IX_Documenti_Guid] ON [dbo].[Documenti] ([Guid])
