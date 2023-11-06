using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzSearch.Shared
{
    public class Documento
    {
        public int Id { get; set; }
        public string Guid { get; set; }
        public string Link { get; set; }
        public string Titolo { get; set; }
        public string Argomento { get; set; }
        public DateTimeOffset DataCreazione { get; set; }
        public string ThumbnailLink { get; set; }

        public string? Content { get; set; }
    }


    public class DocIndexPush
    {
        [SimpleField(IsKey = true, IsFilterable = true)]
        public string db_id { get; set; }

        [SimpleField(IsFilterable = true)]
        public string guid { get; set; }

        [SearchableField(IsFilterable = true, IsSortable = true, IsFacetable = true)]
        public string argomento { get; set; }

        [SearchableField(AnalyzerName = LexicalAnalyzerName.Values.ItLucene)]
        public string titolo { get; set; }

        [SearchableField(AnalyzerName = LexicalAnalyzerName.Values.ItLucene)]
        public string content { get; set; }

        [SimpleField(IsSortable = true)]
        public DateTimeOffset data_creazione { get; set; }

        [SimpleField(IsFilterable = true, IsSortable = true, IsFacetable = true)]
        public int anno { get; set; }
        [SimpleField]
        public string thumbnail_link { get; set; }
    }

    public class DocIndexPull
    {
        [SearchableField(AnalyzerName = LexicalAnalyzerName.Values.ItLucene)]
        public string content { get; set; }
        [SimpleField]
        public string metadata_storage_content_type { get; set; }
        [SimpleField]
        public int metadata_storage_size { get; set; }
        [SimpleField]
        public DateTime metadata_storage_last_modified { get; set; }
        [SimpleField]
        public string metadata_storage_content_md5 { get; set; }
        [SimpleField]
        public string metadata_storage_name { get; set; }
        [SimpleField(IsKey = true)]
        public string metadata_storage_path { get; set; }
        [SimpleField]
        public string metadata_storage_file_extension { get; set; }
        [SimpleField]
        public string metadata_storage_sas_token { get; set; }
        [SimpleField]
        public string metadata_content_type { get; set; }
        [SimpleField]
        public string metadata_author { get; set; }
        [SimpleField]
        public string metadata_character_count { get; set; }
        [SimpleField]
        public DateTime metadata_creation_date { get; set; }
        [SimpleField]
        public DateTime metadata_last_modified { get; set; }
        [SimpleField]
        public string metadata_page_count { get; set; }
        [SimpleField]
        public string metadata_word_count { get; set; }

        [SimpleField]
        public string db_id { get; set; }

        [SimpleField(IsFilterable = true)]
        public string guid { get; set; }

        [SearchableField(IsFilterable = true, IsSortable = true, IsFacetable = true)]
        public string argomento { get; set; }

        [SearchableField(AnalyzerName = LexicalAnalyzerName.Values.ItLucene)]
        public string titolo { get; set; }

        [SimpleField(IsSortable = true)]
        public DateTimeOffset data_creazione { get; set; }

        [SimpleField(IsFilterable = true, IsSortable = true, IsFacetable = true)]
        public int anno { get; set; }

        // enrichment debug
        [SimpleField]
        public string enriched { get; set; }

        [SimpleField]
        public string thumbnail_link { get; set; }
    }
}