using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using CEK.CSharp;
using LongRunningProcessSkill.Adapters;
using LongRunningProcessSkill.Models;

namespace LongRunningProcessSkill
{
    public static class Clova
    {
        [FunctionName("Clova")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            [OrchestrationClient]DurableOrchestrationClientBase starter,
            ILogger log)
        {
            var client = new ClovaClient();
            var cekRequest = await client.GetRequest(req.Headers["SignatureCEK"], req.Body);
            var application = new Application(new ClovaAdapter(), starter, log);
            return new OkObjectResult(await application.ExecuteAsync(cekRequest));
        }
    }
}
