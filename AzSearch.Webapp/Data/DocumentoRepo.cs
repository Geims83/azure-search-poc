using AzSearch.App.Services;
using AzSearch.Shared;
using Dapper;

namespace AzSearch.App.Data
{
    public class DocumentoRepo
    {
        private readonly IDbFactory _dbf;

        public DocumentoRepo()
        {
        }

        public DocumentoRepo(IDbFactory dbFactory)
        {
            _dbf = dbFactory;
        }

        public async Task<Documento> AddAsync(Documento doc)
        {
            using var db = _dbf.GetConnection();

            var docs = await db.QueryAsync<Documento>(
                @"
                INSERT dbo.Documenti ([Guid], Link, Titolo, Argomento, DataCreazione, ThumbnailLink)
                OUTPUT 
                    inserted.Id,
                    inserted.[Guid],
                    inserted.Link,
                    inserted.Titolo,
                    inserted.Argomento,
                    inserted.DataCreazione,
                    inserted.ThumbNailLink
                VALUES (@Guid, @Link, @Titolo, @Argomento, @DataCreazione, @ThumbnailLink)
                ",
                doc);

            return docs.FirstOrDefault();
        }

        public async Task<IEnumerable<Documento>> GetAllAsync()
        {
            using var db = _dbf.GetConnection();

           return await db.QueryAsync<Documento>(
                @"
                SELECT 
                    Id,
                    [Guid],
                    Link,
                    Titolo,
                    Argomento,
                    DataCreazione,
                    ThumbnailLink
                FROM dbo.Documenti
                ");
        }

        public async Task<Documento> GetByIdAsync(int id)
        {
            using var db = _dbf.GetConnection();

            var items = await db.QueryAsync<Documento>(
                 @"
                SELECT 
                    Id,
                    [Guid],
                    Link,
                    Titolo,
                    Argomento,
                    DataCreazione,
                    ThumbnailLink
                FROM dbo.Documenti
                WHERE Id = @Id
                ",
                 new { Id = id });

            return items.ToList().FirstOrDefault();
        }

        public async Task DeleteAsync(int id)
        {
            using var db = _dbf.GetConnection();

            await db.QueryAsync<Documento>(
                 @"
                DELETE
                FROM dbo.Documenti
                WHERE Id = @id
                ",
                 new { id = id });
        }
    }
}
