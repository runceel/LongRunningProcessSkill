using LongRunningProcessSkill.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LongRunningProcessSkill.Adapters
{
    public interface ISmartSpeakerAdapter
    {
        SmartSpeakerRequest ConvertRequest(object originalRequest);
        object ConvertResponse(SmartSpeakerResponse response);
    }
}
