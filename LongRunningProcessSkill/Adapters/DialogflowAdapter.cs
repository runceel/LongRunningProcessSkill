using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Cloud.Dialogflow.V2;
using LongRunningProcessSkill.Models;
using static Google.Cloud.Dialogflow.V2.Intent.Types;
using Microsoft.Extensions.Logging;

namespace LongRunningProcessSkill.Adapters
{
    public class DialogflowAdapter : SmartSpeakerAdapterBase<WebhookRequest, WebhookResponse>
    {
        private const string DefaultWelcomeIntent = "Default Welcome Intent";
        private const string EndIntent = "End Intent"; // スキルの終了を拾う方法は自分でインテントを作らないと無い？
        private readonly ILogger _log;

        public DialogflowAdapter(ILogger log)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public override SmartSpeakerRequest ConvertRequest(WebhookRequest request)
        {
            var user = GetUser(request);

            switch (request.QueryResult.Intent.DisplayName)
            {
                case DefaultWelcomeIntent:
                    return new SmartSpeakerRequest
                    {
                        Platform = "Dialogflow",
                        RequestType = SmartSpeakerRequestType.LaunchRequest,
                        User = user,
                    };
                case EndIntent:
                    return new SmartSpeakerRequest
                    {
                        Platform = "Dialogflow",
                        RequestType = SmartSpeakerRequestType.EndSessionRequest,
                        User = user,
                    };
                default:
                    return new SmartSpeakerRequest
                    {
                        Platform = "Dialogflow",
                        RequestType = SmartSpeakerRequestType.IntentRequest,
                        User = user,
                        Intent = new SmartSpeakerIntent
                        {
                            IntentName = request.QueryResult.Intent.DisplayName,
                            Entities = request.QueryResult.Intent.Parameters.ToDictionary(x => x.Name, x => x.Value),
                        },
                    };
            }
        }

        private static SmartSpeakerUser GetUser(WebhookRequest request)
        {
            var user = new SmartSpeakerUser
            {
                Id = request.OriginalDetectIntentRequest
                .Payload.Fields["user"]
                .StructValue
                .Fields["userId"]
                .StringValue
            };
            return user;
        }

        public override WebhookResponse ConvertResponse(SmartSpeakerResponse response)
        {
            var r = new WebhookResponse();
            r.FulfillmentText = response.ResponseMessage;
            if (response.ShouldEndSession)
            {
                var endSessionMessage = new Message
                {
                    Platform = Message.Types.Platform.ActionsOnGoogle,
                    Payload = new Google.Protobuf.WellKnownTypes.Struct(),
                };
                endSessionMessage.Payload.Fields.Add("expectUserResponse", Google.Protobuf.WellKnownTypes.Value.ForBool(false));
                r.FulfillmentMessages.Add(endSessionMessage);
            }

            return r;
        }
    }
}
