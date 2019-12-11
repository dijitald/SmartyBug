using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Alexa.NET;
using Alexa.NET.Response;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Microsoft.Azure.Cosmos.Table;

namespace SmartyBug
{
    public static class Alexa
    {
        private static String DeviceId = System.Environment.GetEnvironmentVariable("DeviceId", EnvironmentVariableTarget.Process);
        private static String SmartyBugStorageConnectionString = System.Environment.GetEnvironmentVariable("SmartyBugStorageConnectionString", EnvironmentVariableTarget.Process);

        [FunctionName("Alexa")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req, 
            ILogger log)
        {
            SkillResponse response = null;

            log.LogInformation("Alexa function processing...");
            string json = await req.ReadAsStringAsync();
            var skillRequest = JsonConvert.DeserializeObject<SkillRequest>(json);
            if (skillRequest.Context is null) return new BadRequestResult();

            var requestType = skillRequest.GetRequestType();
            log.LogInformation("request type: " + requestType.ToString());

            if (requestType == typeof(LaunchRequest))
            {
                response = ResponseBuilder.Tell("Welcome to Smarty Bug! Smarty Pants.");
                response.Response.ShouldEndSession = false;
            }
            else if (requestType == typeof(SessionEndedRequest))
            {
                response = ResponseBuilder.Empty();
                response.Response.ShouldEndSession = true;
            }
            else if (requestType == typeof(IntentRequest))
            {
                var intentRequest = skillRequest.Request as IntentRequest;
                if (intentRequest.Intent.Name == "Status")
                {
                    response = GetSensorData(log);
                }
            }
           return new OkObjectResult(response);
        }        
        private static SkillResponse GetSensorData(ILogger log)
        {
            String reply = "";
            log.LogInformation("Getting Sensor Data");
            log.LogInformation("Device Id: {0}", DeviceId);

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(SmartyBugStorageConnectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("SmartyBugTelemetry");
            TableQuery<SmartyBugEntity> query = new TableQuery<SmartyBugEntity>().Where(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, DeviceId)).Take(1);

            foreach (SmartyBugEntity entity in table.ExecuteQuery(query))
            {                
                reply = String.Format("The last update I got was on {0} at {1} when the Humidity was {2}, the Temperature was {3}, the Light was {4}, and the Soil Moisture was {5}",
                     entity.Timestamp.ToString("M"),
                     entity.Timestamp.ToString("t"),
                     entity.Humidity,
                     entity.Temperature,
                     entity.Light,
                     entity.SoilMoisture);
                log.LogInformation(reply);
            }
            return ResponseBuilder.Tell(reply);
        }
        public class SmartyBugEntity : TableEntity
        {
            public SmartyBugEntity() { }
            public SmartyBugEntity(string partitionKey, string rowKey)
            {
                this.PartitionKey = partitionKey;
                this.RowKey = rowKey;
            }
            public int Humidity { get; set; }
            public int Light { get; set; }
            public int SoilMoisture { get; set; }
            public int Temperature { get; set; }
            }
    }
}
