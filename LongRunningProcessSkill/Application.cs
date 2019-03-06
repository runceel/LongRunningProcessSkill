using LongRunningProcessSkill.Adapters;
using LongRunningProcessSkill.Models;
using Microsoft.Azure.WebJobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace LongRunningProcessSkill
{
    public class Application
    {
        private readonly ISmartSpeakerAdapter _adapter;
        private readonly DurableOrchestrationClientBase _starter;
        private readonly ILogger _log;

        public Application(ISmartSpeakerAdapter adapter, DurableOrchestrationClientBase starter, ILogger log)
        {
            _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
            _starter = starter ?? throw new ArgumentNullException(nameof(starter));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public async Task<object> ExecuteAsync(object originalRequest)
        {
            var request = _adapter.ConvertRequest(originalRequest);
            var response = new SmartSpeakerResponse();

            switch (request.RequestType)
            {
                case SmartSpeakerRequestType.LaunchRequest:
                    await HandleLauncheRequestAsync(request, response);
                    break;
                case SmartSpeakerRequestType.IntentRequest:
                    await HandleIntentAsync(request, response);
                    break;
                case SmartSpeakerRequestType.EndSessionRequest:
                    // noop
                    break;
                default:
                    // noop
                    break;
            }

            return _adapter.ConvertResponse(response);
        }

        private async Task HandleLauncheRequestAsync(SmartSpeakerRequest request, SmartSpeakerResponse response)
        {
            response.ShouldEndSession = false;
            var status = await _starter.GetStatusAsync(request.User.Id);

            switch (status?.RuntimeStatus ?? OrchestrationRuntimeStatus.Canceled)
            {
                case OrchestrationRuntimeStatus.Completed:
                    {
                        response.ResponseMessage = $"こんにちは。前回開始していた処理が完了しています。結果は {status.Output.ToObject<string>()} 新しい時間のかかる処理を起動します。「進捗どうですか」と尋ねると進捗を答えます。";
                        await _starter.StartNewAsync("LongRunningFunction", request.User.Id, request.Platform);
                        break;
                    }
                case OrchestrationRuntimeStatus.Running:
                    {
                        response.ResponseMessage = $"前回の処理が続いています。現在の状態は {status.CustomStatus.ToObject<string>()}「進捗どうですか」と尋ねると進捗を答えます。";
                        break;
                    }
                default:
                    {
                        response.ResponseMessage = "こんにちは。時間のかかる処理を起動します「進捗どうですか」と尋ねると進捗を答えます。";
                        await _starter.StartNewAsync("LongRunningFunction", request.User.Id, request.Platform);
                        break;
                    }
            }

        }

        private async Task HandleIntentAsync(SmartSpeakerRequest request, SmartSpeakerResponse response)
        {
            var status = await _starter.GetStatusAsync(request.User.Id);
            _log.LogInformation($"現在のタスクの状態: {JsonConvert.SerializeObject(status)}");
            if (status == null)
            {
                response.ResponseMessage = "ごめんなさい。何か変なことが起きたみたい。";
                response.ShouldEndSession = true;
            }
            else if (status.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
            {
                response.ResponseMessage = $"処理が終わりました。メッセージを再生します。{status.Output.ToObject<string>()}";
                response.ShouldEndSession = true;
            }
            else if (status.RuntimeStatus == OrchestrationRuntimeStatus.Terminated || status.RuntimeStatus == OrchestrationRuntimeStatus.Failed || status.RuntimeStatus == OrchestrationRuntimeStatus.Canceled)
            {
                response.ResponseMessage = "失敗したみたい。";
                response.ShouldEndSession = true;
            }
            else
            {
                response.ResponseMessage = $"もうちょっと待ってね。現在のステータスは {status.CustomStatus.ToObject<string>()}";
            }
        }
    }
}
