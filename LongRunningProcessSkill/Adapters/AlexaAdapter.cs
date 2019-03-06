using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using LongRunningProcessSkill.Models;

namespace LongRunningProcessSkill.Adapters
{
    public class AlexaAdapter : SmartSpeakerAdapterBase<SkillRequest, SkillResponse>
    {
        public override SkillResponse ConvertResponse(SmartSpeakerResponse response) => new SkillResponse
        {
            Version = "1.0",
            Response = new ResponseBody
            {
                OutputSpeech = new PlainTextOutputSpeech
                {
                    Text = response.ResponseMessage,
                },
                ShouldEndSession = response.ShouldEndSession,
            },
        };

        public override SmartSpeakerRequest ConvertRequest(SkillRequest request)
        {
            var user = new SmartSpeakerUser
            {
                Id = request.Session.User.UserId,
            };

            switch (request.Request)
            {
                case LaunchRequest _:
                    return new SmartSpeakerRequest
                    {
                        Platform = "Alexa",
                        RequestType = SmartSpeakerRequestType.LaunchRequest,
                        User = user,
                    };
                case IntentRequest ir:
                    return new SmartSpeakerRequest
                    {
                        Platform = "Alexa",
                        RequestType = SmartSpeakerRequestType.IntentRequest,
                        Intent = new SmartSpeakerIntent
                        {
                            IntentName = ir.Intent.Name,
                            Entities = ir.Intent.Slots.ToDictionary(x => x.Key, x => x.Value.Value),
                        },
                        User = user,
                    };
                case SessionEndedRequest _:
                    return new SmartSpeakerRequest
                    {
                        Platform = "Alexa",
                        RequestType = SmartSpeakerRequestType.EndSessionRequest,
                        User = user,
                    };
                default:
                    return new SmartSpeakerRequest
                    {
                        Platform = "Alexa",
                        RequestType = SmartSpeakerRequestType.EndSessionRequest,
                        User = user,
                    };
            }
        }
    }
}
