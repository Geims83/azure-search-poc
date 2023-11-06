using AzSearch.Shared;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents;
using Azure;

namespace Mgmt
{
    internal partial class Program
    {

        private static void CreateIndexPush(string indexName, SearchIndexClient adminClient)
        {
            FieldBuilder fieldBuilder = new FieldBuilder();
            var searchFields = fieldBuilder.Build(typeof(DocIndexPush));

            var definition = new SearchIndex(indexName, searchFields);

            var suggester = new SearchSuggester("sg", new[] { "argomento" });
            definition.Suggesters.Add(suggester);

            adminClient.CreateOrUpdateIndex(definition);
        }

        private static void PushIndex()
        {

            // Create a SearchIndexClient to send create/delete index commands
            Uri serviceEndpoint = new Uri($"https://{searchServiceName}.search.windows.net/");
            AzureKeyCredential credential = new AzureKeyCredential(searchApiKey);

            SearchIndexClient adminClient = new SearchIndexClient(serviceEndpoint, credential);

            // Create a SearchClient to load and query documents
            SearchClient srchclient = new SearchClient(serviceEndpoint, pushIndexName, credential);

            // Delete index if it exists
            Console.WriteLine("{0}", "Deleting push index...\n");
            DeleteIndexIfExists(pushIndexName, adminClient);

            // Create index
            Console.WriteLine("{0}", "Creating push index...\n");
            CreateIndexPush(pushIndexName, adminClient);
        }
    }
}
