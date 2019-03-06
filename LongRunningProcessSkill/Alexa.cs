using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Alexa.NET.Request;
using LongRunningProcessSkill.Adapters;
using LongRunningProcessSkill.Models;

namespace LongRunningProcessSkill
{
    public static class Alexa
    {
        [FunctionName("Alexa")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            [OrchestrationClient]DurableOrchestrationClientBase starter,
            ILogger log)
        {
            var skillRequest = JsonConvert.DeserializeObject<SkillRequest>(await req.ReadAsStringAsync());
            var application = new Application(new AlexaAdapter(), starter, log);
            return new OkObjectResult(await application.ExecuteAsync(skillRequest));
        }
    }
}
