using Azure.Search.Documents.Indexes;
using Azure.Search.Documents;
using Azure;
using AzSearch.Shared;
using Azure.Search.Documents.Indexes.Models;

namespace Mgmt
{
    internal partial class Program
    {


        private static void PullIndex()
        {

            // Create a SearchIndexClient to send create/delete index commands
            Uri serviceEndpoint = new Uri($"https://{searchServiceName}.search.windows.net/");
            AzureKeyCredential credential = new AzureKeyCredential(searchApiKey);
            SearchIndexClient adminClient = new SearchIndexClient(serviceEndpoint, credential);

            // Create a SearchClient to load and query documents
            SearchClient srchclient = new SearchClient(serviceEndpoint, pullIndexName, credential);

            // Delete index if it exists
            Console.WriteLine("{0}", "Deleting pull index...\n");
            DeleteIndexIfExists(pullIndexName, adminClient);

            // Create index
            Console.WriteLine("{0}", "Creating pull index...\n");
            CreateIndexPull(pullIndexName, adminClient);

            CreateIndexer(pullIndexName, adminClient);
        }

        private static void CreateIndexPull(string indexName, SearchIndexClient adminClient)
        {
            FieldBuilder fieldBuilder = new FieldBuilder();
            var searchFields = fieldBuilder.Build(typeof(DocIndexPull));

            var definition = new SearchIndex(indexName, searchFields);

            var suggester = new SearchSuggester("sg", new[] { "argomento" });
            definition.Suggesters.Add(suggester);

            adminClient.CreateOrUpdateIndex(definition);
        }

        private static void CreateIndexer(string indexerName, SearchIndexClient adminClient)
        {

            Uri serviceEndpoint = new Uri($"https://{searchServiceName}.search.windows.net/");
            AzureKeyCredential credential = new AzureKeyCredential(searchApiKey);
            SearchIndexerClient indexerClient = new SearchIndexerClient(serviceEndpoint, credential);

            // datasource
            SearchIndexerDataSourceConnection dataSource = CreateOrUpdateDatasource(indexerClient);

            // indexer 
            IndexingParameters indexingParameters = new IndexingParameters()
            {
                MaxFailedItems = -1,
                MaxFailedItemsPerBatch = -1
            };
            indexingParameters.Configuration.Add("dataToExtract", "contentAndMetadata");

            var functionSkill = CreateEnrichmentSkill();

            // context default "/document"
            //functionSkill.Context = "/document";

            List<SearchIndexerSkill> skills = new List<SearchIndexerSkill>() { functionSkill };

            var skillSet = CreateOrUpdateSkillSet(indexerClient, skills);

            SearchIndexer indexer = new SearchIndexer("pull-indexer", dataSource.Name, pullIndexName)
            {
                Description = "piup indexer",
                SkillsetName = skillSet.Name,
                Parameters = indexingParameters
            };

            FieldMappingFunction mappingFunction = new FieldMappingFunction("base64Encode");
            mappingFunction.Parameters.Add("useHttpServerUtilityUrlTokenEncode", true);

            indexer.FieldMappings.Add(new FieldMapping("metadata_storage_path")
            {
                TargetFieldName = "metadata_storage_path",
                MappingFunction = mappingFunction

            });

            // I campi che arrivano dall'enrichment vanno aggiunti a OUTPUTfieldmapping
            indexer.OutputFieldMappings.Add(new FieldMapping("/document/db_id")
            {
                TargetFieldName = "db_id"
            });
            indexer.OutputFieldMappings.Add(new FieldMapping("/document/guid")
            {
                TargetFieldName = "guid"
            });
            indexer.OutputFieldMappings.Add(new FieldMapping("/document/titolo")
            {
                TargetFieldName = "titolo"
            });
            indexer.OutputFieldMappings.Add(new FieldMapping("/document/argomento")
            {
                TargetFieldName = "argomento"
            });
            indexer.OutputFieldMappings.Add(new FieldMapping("/document/data_creazione")
            {
                TargetFieldName = "data_creazione"
            });
            indexer.OutputFieldMappings.Add(new FieldMapping("/document/anno")
            {
                TargetFieldName = "anno"
            });

            indexer.OutputFieldMappings.Add(new FieldMapping("/document/thumbnail_link")
            {
                TargetFieldName = "thumbnail_link"
            });

            indexer.Schedule = new IndexingSchedule(TimeSpan.FromMinutes(15));

            try
            {
                indexerClient.GetIndexer(indexer.Name);
                indexerClient.DeleteIndexer(indexer.Name);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                //if the specified indexer not exist, 404 will be thrown.
            }

            try
            {
                indexerClient.CreateIndexer(indexer);
            }
            catch (RequestFailedException ex)
            {
                Console.WriteLine("Failed to create the indexer\n Exception message: {0}\n", ex.Message);
            }


        }

        private static SearchIndexerDataSourceConnection CreateOrUpdateDatasource(SearchIndexerClient indexerClient)
        {
            SearchIndexerDataSourceConnection dataSource = new SearchIndexerDataSourceConnection(
                                name: "pull-datasource",
                                type: SearchIndexerDataSourceType.AzureBlob,
                                connectionString: datasourceConnectionString,
                                container: new SearchIndexerDataContainer("ipull"))
            {
                Description = "Indexer per indice pull",
                DataDeletionDetectionPolicy = new SoftDeleteColumnDeletionDetectionPolicy()
                {
                    SoftDeleteColumnName = "isSoftDeleted",
                    SoftDeleteMarkerValue = "Yes"
                }
            };

            // The data source does not need to be deleted if it was already created
            // since we are using the CreateOrUpdate method
            try
            {
                indexerClient.CreateOrUpdateDataSourceConnection(dataSource);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to create or update the data source\n Exception message: {0}\n", ex.Message);
            }

            return dataSource;
        }

        private static WebApiSkill CreateEnrichmentSkill()
        {
            List<InputFieldMappingEntry> inputMappings = new List<InputFieldMappingEntry>();
            inputMappings.Add(new InputFieldMappingEntry("metadata_storage_name")
            {
                Source = "/document/metadata_storage_name"
            });

            // definisci i campi in output della skill
            List<OutputFieldMappingEntry> outputMappings = new List<OutputFieldMappingEntry>();
            outputMappings.Add(new OutputFieldMappingEntry("db_id"));
            outputMappings.Add(new OutputFieldMappingEntry("titolo"));
            outputMappings.Add(new OutputFieldMappingEntry("argomento"));
            outputMappings.Add(new OutputFieldMappingEntry("data_creazione"));
            outputMappings.Add(new OutputFieldMappingEntry("anno"));
            outputMappings.Add(new OutputFieldMappingEntry("guid"));
            outputMappings.Add(new OutputFieldMappingEntry("thumbnail_link"));

            var enrich = new WebApiSkill(inputMappings, outputMappings, functionsEndpoint);
            enrich.HttpHeaders.Add("x-functions-key", functionsKey);
            enrich.HttpMethod = "POST";

            return enrich;
        }

        private static SearchIndexerSkillset CreateOrUpdateSkillSet(SearchIndexerClient indexerClient, IList<SearchIndexerSkill> skills)
        {
            SearchIndexerSkillset skillset = new SearchIndexerSkillset("pull-skillset", skills)
            {
                Description = "Pull skillset"
            };

            // Create the skillset in your search service.
            // The skillset does not need to be deleted if it was already created
            // since we are using the CreateOrUpdate method
            try
            {
                indexerClient.CreateOrUpdateSkillset(skillset);
            }
            catch (RequestFailedException ex)
            {
                Console.WriteLine("Failed to create the skillset\n Exception message: {0}\n", ex.Message);
            }

            return skillset;
        }
    }
}

