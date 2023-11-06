using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using AzSearch.Shared;
using Azure.Storage.Blobs;
using System;
using Azure.Search.Documents.Models;
using Azure.Search.Documents;
using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;

namespace AzSearch.Api
{
    public static class Push
    {
        [FunctionName("Push_queueTrigger")]
        public static async Task QueueTrigger(
            [QueueTrigger("push", Connection = "IndexStorage")] string myQueueItem,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            log.LogInformation(myQueueItem);

            string instanceId = await starter.StartNewAsync("Push_orchestrator", input: myQueueItem);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");
        }

        [FunctionName("Push_orchestrator")]
        public static async Task RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var input = context.GetInput<string>();

            var doc = JsonConvert.DeserializeObject<Documento>(input);

            string docContent = await context.CallActivityAsync<string>(nameof(GetText), doc);

            doc.Content = docContent;

            await context.CallActivityAsync<string>(nameof(PushToIndex), doc);
        }

        [FunctionName(nameof(GetText))]
        public static async Task<string> GetText([ActivityTrigger] Documento doc, ILogger log)
        {
            BlobContainerClient container = new BlobContainerClient(Environment.GetEnvironmentVariable("IndexStorage"), "ipush");
            var docBlob = container.GetBlobClient(doc.Guid);

            var blobUri = docBlob.GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddHours(1));

            AzureKeyCredential credential = new AzureKeyCredential(Environment.GetEnvironmentVariable("VisionApiKey"));
            DocumentAnalysisClient client = new DocumentAnalysisClient(new Uri(Environment.GetEnvironmentVariable("VisionApiUrl")), credential);

            AnalyzeDocumentOperation operation = await client.AnalyzeDocumentFromUriAsync(WaitUntil.Completed, "prebuilt-read", blobUri);
            AnalyzeResult result = operation.Value;

            return result.Content;
        }

        [FunctionName(nameof(PushToIndex))]
        public static async Task PushToIndex([ActivityTrigger] Documento doc, ILogger log)
        {
            Uri serviceEndpoint = new Uri(Environment.GetEnvironmentVariable("SearchApiUrl"));
            AzureKeyCredential credential = new AzureKeyCredential(Environment.GetEnvironmentVariable("SearchApiKey"));

            SearchClient searchclient = new SearchClient(serviceEndpoint, "index-push", credential);

            DocIndexPush documentoIndex = new DocIndexPush()
            {
                db_id = doc.Id.ToString(),
                titolo = doc.Titolo,
                argomento = doc.Argomento,
                content = doc.Content,
                data_creazione = doc.DataCreazione,
                anno = doc.DataCreazione.Year,
                guid = doc.Guid.ToString(),
                thumbnail_link = doc.ThumbnailLink,
            };

            IndexDocumentsBatch<DocIndexPush> batch = IndexDocumentsBatch.Create(
               IndexDocumentsAction.MergeOrUpload(documentoIndex));

            try
            {
                IndexDocumentsResult result = await searchclient.IndexDocumentsAsync(batch);
            }
            catch (Exception)
            {
                // If for some reason any documents are dropped during indexing, you can compensate by delaying and
                // retrying. This simple demo just logs the failed document keys and continues.
                Console.WriteLine("Failed to index some of the documents: {0}");
            }
        }

    }
}