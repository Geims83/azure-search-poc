using AzSearch.Shared;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace AzSearch.App.Services
{
    public class SearchService
    {

        private SearchClient _pushClient;
        private SearchClient _pullClient;

        public SearchService() { }

        public SearchService(string endpoint, string key)
        {
            // Create a SearchIndexClient to send create/delete index commands
            Uri serviceEndpoint = new Uri($"https://{endpoint}.search.windows.net/");
            AzureKeyCredential credential = new AzureKeyCredential(key);
            SearchIndexClient adminClient = new SearchIndexClient(serviceEndpoint, credential);

            // Create a SearchClient to load and query documents
            _pullClient= new SearchClient(serviceEndpoint, "index-pull", credential);
            _pushClient = new SearchClient(serviceEndpoint, "index-push", credential);
        }

        public async Task<Response<Azure.Search.Documents.Models.SearchResults<DocIndexPull>>> SearchPull(
            string term, string titolo, string argomento, bool sortByDate, bool sortDirection, int pagina, int numeroElementi)
        {
            var options = CreateOptions(term, titolo, argomento, sortByDate, sortDirection, numeroElementi, pagina * numeroElementi);

            string searchQuery = CreateSearchTerm(term, titolo);

            var response = await _pullClient.SearchAsync<DocIndexPull>(searchQuery, options);

            return response;
        }

        public async Task<Response<Azure.Search.Documents.Models.SearchResults<DocIndexPush>>> SearchPush(
            string term, string titolo, string argomento, bool sortByDate, bool sortDirection, int pagina, int numeroElementi)
        {
            var options = CreateOptions(term, titolo, argomento, sortByDate, sortDirection, numeroElementi, pagina * numeroElementi);

            string searchQuery = CreateSearchTerm(term, titolo);

            var response = await _pushClient.SearchAsync<DocIndexPush>(searchQuery, options);

            return response;
        }

        public async Task<List<string>> GetArgomentoSuggest(string start)
        {
            var suggestList = new List<string>();

            var result = await _pushClient.AutocompleteAsync(start, "sg");
            var autocompleteResult = result.Value;
            var list = autocompleteResult.Results.ToList();
            foreach ( var item in list )
            {
                suggestList.Add(item.Text);
            }

            return suggestList;
        }

        private SearchOptions CreateOptions(string term, string titolo, string argomento, bool sorting, bool sortDirection, int size, int skip)
        {
            string orderby = "";
            if (sorting)
            {
                orderby += "data_creazione";
                if (sortDirection)
                    orderby += " asc";
                else
                    orderby += " desc";
            }

            var options = new SearchOptions()
            {
                IncludeTotalCount = true,
                Filter = (!argomento.IsNullOrEmpty()? $"argomento eq '{argomento}'" : ""),
                OrderBy = { orderby },
                HighlightFields = { "content" },
                Facets = { "anno", "argomento" },
                QueryType = Azure.Search.Documents.Models.SearchQueryType.Full,
                Size = size,
                Skip = skip
            };

            options.Select.Add("titolo");
            options.Select.Add("argomento");
            options.Select.Add("content");
            options.Select.Add("anno");
            options.Select.Add("data_creazione");
            options.Select.Add("thumbnail_link");
            options.Select.Add("guid");

            return options;
        }

        private string CreateSearchTerm(string term, string titolo)
        {
            string search = "";
            
            if (term.IsNullOrEmpty())
                search = "*";
            else
                search = $"content:{term}";

            if (!titolo.IsNullOrEmpty())
            {
                if (search.Length > 1)
                    search += $" AND titolo:{titolo}";
                else
                    search = $"titolo:{titolo}";
            }

            return search;
        }

        public async Task PushDeleteDocument(Documento doc)
        {
            var result = await _pushClient.DeleteDocumentsAsync("db_id", new string[] { doc.Id.ToString() });

        }
    }
}
