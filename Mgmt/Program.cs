using Azure.Search.Documents.Indexes;
using Azure.Search.Documents;
using Azure;

namespace Mgmt
{
    internal partial class Program
    {
        static string searchServiceName;
        static string searchApiKey;

        static string pullIndexName;
        static string datasourceConnectionString;
        static string functionsKey;
        static string functionsEndpoint;

        static string pushIndexName;


        static void Main(string[] args)
        {
            if (!ParseConfiguration("development.env") || ParseConfiguration(".env"))
            {
                Console.WriteLine("Please create configuration file");
                return;
            }

            searchServiceName = Environment.GetEnvironmentVariable("searchServiceName");
            searchApiKey = Environment.GetEnvironmentVariable("searchApiKey");
            
            pullIndexName = "index-pull";
            datasourceConnectionString = Environment.GetEnvironmentVariable("datasourceConnectionString");
            functionsEndpoint = Environment.GetEnvironmentVariable("functionsEndpoint");
            functionsKey = Environment.GetEnvironmentVariable("functionsKey");
            
            pushIndexName = "index-push";

            PushIndex();
            PullIndex();
        }
        
        static bool ParseConfiguration(string filePath)
        {
            if (!File.Exists(filePath))
                return false;

            foreach (var line in File.ReadAllLines(filePath))
            {
                int splitPoint = line.IndexOf('=');

                string key = line[0..splitPoint];
                string value = line[(splitPoint+1)..];

                Environment.SetEnvironmentVariable(key, value);
            }

            return true;
        }

        private static void DeleteIndexIfExists(string indexName, SearchIndexClient adminClient)
        {
            adminClient.GetIndexNames();
            {
                adminClient.DeleteIndex(indexName);
            }
        }
    }
}