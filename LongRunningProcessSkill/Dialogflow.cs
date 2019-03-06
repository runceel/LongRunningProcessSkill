using Google.Cloud.Dialogflow.V2;
using Google.Protobuf;
using LongRunningProcessSkill.Adapters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace LongRunningProcessSkill
{
    public static class Dialogflow
    {
        [FunctionName("Dialogflow")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            [OrchestrationClient]DurableOrchestrationClientBase starter,
            ILogger log)
        {
            var parser = new JsonParser(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));
            var webhookRequest = parser.Parse<WebhookRequest>(await req.ReadAsStringAsync());
            var application = new Application(new DialogflowAdapter(log), starter, log);
            return new ProtcolBufJsonResult(await application.ExecuteAsync(webhookRequest), JsonFormatter.Default);
        }
    }

    class ProtcolBufJsonResult : IActionResult
    {
        private readonly object _obj;
        private readonly JsonFormatter _formatter;

        public ProtcolBufJsonResult(object obj, JsonFormatter formatter)
        {
            _obj = obj;
            _formatter = formatter;
        }

        public async Task ExecuteResultAsync(ActionContext context)
        {
            context.HttpContext.Response.Headers.Add("Content-Type", new Microsoft.Extensions.Primitives.StringValues("application/json; charset=utf-8"));
            var stringWriter = new StringWriter();
            _formatter.WriteValue(stringWriter, _obj);
            await context.HttpContext.Response.WriteAsync(stringWriter.ToString());
        }
    }

}
