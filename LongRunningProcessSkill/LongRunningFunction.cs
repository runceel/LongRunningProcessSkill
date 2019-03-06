using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using LongRunningProcessSkill.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;

namespace LongRunningProcessSkill
{
    public static class LongRunningFunction
    {
        [FunctionName("LongRunningFunction")]
        public static async Task<string> RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context, ILogger log)
        {
            var start = context.CurrentUtcDateTime;
            using (var cts = new CancellationTokenSource())
            {
                context.SetCustomStatus($"挨拶文の生成を待っています。");
                // 時間のかかる処理（15秒）を呼び出す
                var greetingMessage = await context.CallActivityAsync<string>("LongRunningFunction_GenerateGreetingMessage", "ちょまど");
                // 途中経過を報告
                context.SetCustomStatus($"挨拶の生成が終わりました。次の処理をしています。");

                // 管理者アプリへ対応してもらうためのデータの追加
                await context.CallActivityAsync("LongRunningFunction_Started", context.GetInput<string>());

                // 5分たつか、管理者からのメッセージを待つ。
                var adminMessage = await context.WaitForExternalEvent<string>("done");

                // 管理者の対応が終わったので後処理
                await context.CallActivityAsync("LongRunningFunction_Finished", context.GetInput<string>());

                context.SetCustomStatus($"最終的なメッセージを生成しています。");
                // 時間のかかる処理（15秒）を呼び出す
                var resultMessage = await context.CallActivityAsync<string>("LongRunningFunction_GenerateResultMessage", new
                {
                    Start = start,
                    GreetingMessage = greetingMessage,
                    AdminMessage = adminMessage,
                });

                // 最終的なメッセージを返す。
                return resultMessage;
            }
        }

        [FunctionName("LongRunningFunction_GenerateGreetingMessage")]
        public static async Task<string> GenerateGreetingMessage([ActivityTrigger] string name, ILogger log)
        {
            await Task.Delay(15 * 1000); // 15秒とめる
            return $"こんにちは、{name}さん。";
        }


        [FunctionName("LongRunningFunction_GenerateResultMessage")]
        public static async Task<string> GenerateResultMessage([ActivityTrigger] DurableActivityContext context, ILogger log)
        {
            await Task.Delay(15 * 1000); // 15秒とめる
            var arg = context.GetInput<dynamic>();
            DateTime start = arg.Start;
            var span = DateTime.UtcNow - start;
            return $"{arg.GreetingMessage} 入力されたメッセージは {arg.AdminMessage} です。この処理は{(int)span.TotalSeconds}秒かかりました。";
        }

        [FunctionName("LongRunningFunction_Started")]
        public static async Task Started([ActivityTrigger] DurableActivityContext context, [Table("EventLog")] IAsyncCollector<EventLog> eventLogTable, ILogger log)
        {
            await eventLogTable.AddAsync(new EventLog
            {
                PartitionKey = context.GetInput<string>(), // Platform
                RowKey = context.InstanceId,
            });
        }

        [FunctionName("LongRunningFunction_Finished")]
        public static async Task Finished([ActivityTrigger] DurableActivityContext context, [Table("EventLog")] CloudTable eventLogTable, ILogger log)
        {
            var deleteOperation = TableOperation.Delete(new EventLog
            {
                PartitionKey = context.GetInput<string>(), // Platform
                RowKey = context.InstanceId,
                ETag = "*",
            });

            await eventLogTable.ExecuteAsync(deleteOperation);
        }
    }
}