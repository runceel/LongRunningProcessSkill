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
                context.SetCustomStatus($"���A���̐�����҂��Ă��܂��B");
                // ���Ԃ̂����鏈���i15�b�j���Ăяo��
                var greetingMessage = await context.CallActivityAsync<string>("LongRunningFunction_GenerateGreetingMessage", "����܂�");
                // �r���o�߂��
                context.SetCustomStatus($"���A�̐������I���܂����B���̏��������Ă��܂��B");

                // �Ǘ��҃A�v���֑Ή����Ă��炤���߂̃f�[�^�̒ǉ�
                await context.CallActivityAsync("LongRunningFunction_Started", context.GetInput<string>());

                // 5�������A�Ǘ��҂���̃��b�Z�[�W��҂B
                var adminMessage = await context.WaitForExternalEvent<string>("done");

                // �Ǘ��҂̑Ή����I������̂Ō㏈��
                await context.CallActivityAsync("LongRunningFunction_Finished", context.GetInput<string>());

                context.SetCustomStatus($"�ŏI�I�ȃ��b�Z�[�W�𐶐����Ă��܂��B");
                // ���Ԃ̂����鏈���i15�b�j���Ăяo��
                var resultMessage = await context.CallActivityAsync<string>("LongRunningFunction_GenerateResultMessage", new
                {
                    Start = start,
                    GreetingMessage = greetingMessage,
                    AdminMessage = adminMessage,
                });

                // �ŏI�I�ȃ��b�Z�[�W��Ԃ��B
                return resultMessage;
            }
        }

        [FunctionName("LongRunningFunction_GenerateGreetingMessage")]
        public static async Task<string> GenerateGreetingMessage([ActivityTrigger] string name, ILogger log)
        {
            await Task.Delay(15 * 1000); // 15�b�Ƃ߂�
            return $"����ɂ��́A{name}����B";
        }


        [FunctionName("LongRunningFunction_GenerateResultMessage")]
        public static async Task<string> GenerateResultMessage([ActivityTrigger] DurableActivityContext context, ILogger log)
        {
            await Task.Delay(15 * 1000); // 15�b�Ƃ߂�
            var arg = context.GetInput<dynamic>();
            DateTime start = arg.Start;
            var span = DateTime.UtcNow - start;
            return $"{arg.GreetingMessage} ���͂��ꂽ���b�Z�[�W�� {arg.AdminMessage} �ł��B���̏�����{(int)span.TotalSeconds}�b������܂����B";
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