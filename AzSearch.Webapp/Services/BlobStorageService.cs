using AzSearch.Shared;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Queues;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace AzSearch.App.Services
{
    public class BlobStorageService
    {
        private BlobContainerClient _pushContainer;
        private BlobContainerClient _pullContainer;
        private BlobContainerClient _thumbnailContainer;

        private QueueClient _pushQueue;

        public BlobStorageService() { }

        public BlobStorageService(string connectionString) 
        {
            _pushContainer = new BlobContainerClient(connectionString, "ipush");
            _pullContainer = new BlobContainerClient(connectionString, "ipull");
            _thumbnailContainer = new BlobContainerClient(connectionString, "imgs");

            _pushQueue = new QueueClient(connectionString, "push", new QueueClientOptions()
            {
                MessageEncoding = QueueMessageEncoding.Base64
            });

            _pushContainer.CreateIfNotExists();
            _pullContainer.CreateIfNotExists();
            
            _thumbnailContainer.CreateIfNotExists();
            
            _pushQueue.CreateIfNotExists();
        }

        public async Task UploadPush(string docName, BinaryData content)
        {
            await _pushContainer.UploadBlobAsync(docName, content);
        }

        public async Task UploadPull(string docName, BinaryData content)
        {
            await _pullContainer.UploadBlobAsync(docName, content);
        }

        public async Task UploadThumbnail(string imgName, BinaryData content)
        {
            await _thumbnailContainer.UploadBlobAsync(imgName, content);
        }

        public async Task EnqueuePush(Documento doc)
        {
            var msg = JsonConvert.SerializeObject(doc);
            await _pushQueue.SendMessageAsync(msg);
        }

        public async Task MarkAsEdited(Documento doc)
        {
            var blobClient = _pullContainer.GetBlobBaseClient(doc.Guid);
            Dictionary<string, string> update = new Dictionary<string, string>();
            update.Add("manualUpdate", DateTime.UtcNow.ToString());
            await blobClient.SetMetadataAsync(update);
        }

        public async Task Delete(Documento doc)
        {
            var blobClient = _pullContainer.GetBlobBaseClient(doc.Guid);
            Dictionary<string, string> softDelete = new Dictionary<string, string>();
            softDelete.Add("isSoftDeleted","Yes");
            
            var blobClientPush = _pushContainer.GetBlobBaseClient(doc.Guid);
            
            await Task.WhenAll(new Task[] { blobClient.SetMetadataAsync(softDelete), blobClientPush.DeleteAsync()});
        }

        public string GetAccountName() => _pullContainer.AccountName;

        public string GetThumbnailAccess(string guid)
        {
            var blobClient = _thumbnailContainer.GetBlobBaseClient($"{guid}.png");
            return blobClient.GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read, DateTimeOffset.Now.AddHours(1)).ToString();
        }
    }
}
