using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos.Table;
using Company.Function;

namespace api
{
    public static class PageCounterGet
    {
        const string tableName = "viewcountertable";

        [FunctionName("PageCounterGet")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "PageCounter/{pageViewURL}")] HttpRequest req,
            string pageViewURL,
            ILogger log)
        {
            var storageAccountConnectionString = GetConnectionString("StorageConnectionString");
            var storageAccount = CloudStorageAccount.Parse($"{storageAccountConnectionString}");
            var tableClient = storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference(tableName);

            await table.CreateIfNotExistsAsync(); 

            var retrievedResult = table.Execute(TableOperation.Retrieve<ViewCount>(pageViewURL, "visits"));

            if(retrievedResult.Result == null)
            {
                return new NotFoundResult();
            }
            var pageView = (ViewCount)retrievedResult.Result;

            return new OkObjectResult(pageView.Count.ToString());
        }

        public static string GetConnectionString(string name)
        {
           string conStr = System.Environment.GetEnvironmentVariable($"ConnectionStrings:{name}", 
                                                                   EnvironmentVariableTarget.Process);
           if (string.IsNullOrEmpty(conStr)) 
               conStr = Environment.GetEnvironmentVariable("StorageConnectionString");
            return conStr;
        }
    }
}
