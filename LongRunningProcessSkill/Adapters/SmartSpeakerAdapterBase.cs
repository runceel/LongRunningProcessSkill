using System;
using System.Collections.Generic;
using System.Text;
using LongRunningProcessSkill.Models;

namespace LongRunningProcessSkill.Adapters
{
    public abstract class SmartSpeakerAdapterBase<TRequest, TResponse> : ISmartSpeakerAdapter
        where TRequest : class
        where TResponse : class
    {
        public abstract SmartSpeakerRequest ConvertRequest(TRequest request);
        SmartSpeakerRequest ISmartSpeakerAdapter.ConvertRequest(object originalRequest) => ConvertRequest((TRequest)originalRequest);

        public abstract TResponse ConvertResponse(SmartSpeakerResponse response);

        object ISmartSpeakerAdapter.ConvertResponse(SmartSpeakerResponse response) => ConvertResponse(response);
    }
}
