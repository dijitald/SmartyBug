using System;
using System.IO;
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

namespace SmartyBug
{
    public static class Alexa
    {
        [FunctionName("Alexa")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req, ILogger log)
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
                    //var speechlet = new SmartyBugSpeechlet();
                    //return await speechlet.GetResponseAsync(req);
                }
            }
    
           return new OkObjectResult(response);
        }        
    }
    public class SmartyBugSpeechlet // : SpeechletBase, ISpeechletWithContext
    {
         public SkillResponse FunctionHandler(SkillRequest input)
        {
            // your function logic goes here
            
            var intentRequest = input.Request as IntentRequest;
            // check the name to determine what you should do
            if (intentRequest.Intent.Name.Equals("MyIntentName"))
            {
                if(intentRequest.DialogState.Equals("COMPLETED"))
                {
                    // get the slots
                    var firstValue = intentRequest.Intent.Slots["FirstSlot"].Value;
                }
            } 
            return ResponseBuilder.Empty();
            //return new SkillResponse("OK");
        } 

    }
}
