using AzSearch.Shared;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using static AzSearch.Api.Models;

namespace AzSearch.Api
{
    public static class Pull
    {
        [FunctionName("Pull")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Admin, "post", Route = "pull")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<PullInput>(requestBody);

            log.LogInformation(requestBody);

            var listaOutput = new List<ValueOutput>();

            try
            {
                var output = new PullOutput();
                output.values = new List<ValueOutput>();

                foreach (var item in data.values)
                {
                    var oItem = new ValueOutput();
                    oItem.recordId = item.recordId;
                    oItem.errors = new List<string>();
                    oItem.warnings = new List<string>();

                    SqlConnection conn = new SqlConnection(Environment.GetEnvironmentVariable("SqlDb"));

                    var dbList = await conn.QueryAsync<Documento>("SELECT * FROM dbo.Documenti WHERE [Guid] = @id", new { id = Convert.ToString(item.data.metadata_storage_name) });

                    var dbItem = dbList.FirstOrDefault();

                    if (dbItem != null)
                    {
                        oItem.data = new DataOutput()
                        {
                            titolo = dbItem.Titolo,
                            argomento = dbItem.Argomento,
                            db_id = dbItem.Id.ToString(),
                            data_creazione = dbItem.DataCreazione,
                            anno = dbItem.DataCreazione.Year,
                            guid = dbItem.Guid,
                            thumbnail_link = dbItem.ThumbnailLink
                        };
                    }
                    else
                    {
                        oItem.errors.Add($"guid {item.data.metadata_storage_name} not found in db");
                    }

                    output.values.Add(oItem);
                }
                log.LogInformation(JsonConvert.SerializeObject(output));
                
                return new OkObjectResult(output);
            }
            catch (Exception ex) 
            {
                return new NotFoundObjectResult(ex);
            }
        }
    }
}