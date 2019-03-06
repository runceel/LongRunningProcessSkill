using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CEK.CSharp.Models;
using LongRunningProcessSkill.Models;

namespace LongRunningProcessSkill.Adapters
{
    public class ClovaAdapter : SmartSpeakerAdapterBase<CEKRequest, CEKResponse>
    {
        public override SmartSpeakerRequest ConvertRequest(CEKRequest request)
        {
            var user = new SmartSpeakerUser
            {
                Id = request.Session.User.UserId,
            };

            switch (request.Request.Type)
            {
                case RequestType.LaunchRequest:
                    return new SmartSpeakerRequest
                    {
                        Platform = "Clova",
                        RequestType = SmartSpeakerRequestType.LaunchRequest,
                        User = user,
                    };
                case RequestType.SessionEndedRequest:
                    return new SmartSpeakerRequest
                    {
                        Platform = "Clova",
                        RequestType = SmartSpeakerRequestType.EndSessionRequest,
                        User = user,
                    };
                case RequestType.IntentRequest:
                    return new SmartSpeakerRequest
                    {
                        Platform = "Clova",
                        RequestType = SmartSpeakerRequestType.IntentRequest,
                        Intent = new SmartSpeakerIntent
                        {
                            IntentName = request.Request.Intent.Name,
                            Entities = request.Request.Intent.Slots.ToDictionary(x => x.Key, x => x.Value.Value),
                        },
                        User = user,
                    };
                default:
                    return new SmartSpeakerRequest
                    {
                        Platform = "Clova",
                        RequestType = SmartSpeakerRequestType.None,
                    };
            }
        }

        public override CEKResponse ConvertResponse(SmartSpeakerResponse response)
        {
            var r = new CEKResponse
            {
                ShouldEndSession = response.ShouldEndSession,
            };

            r.AddText(response.ResponseMessage);
            return r;
        }
    }
}
