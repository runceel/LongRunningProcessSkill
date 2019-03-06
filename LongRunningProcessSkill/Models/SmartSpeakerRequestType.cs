using System;
using System.Collections.Generic;
using System.Text;

namespace LongRunningProcessSkill.Models
{
    public enum SmartSpeakerRequestType
    {
        None,
        LaunchRequest,
        IntentRequest,
        EndSessionRequest,
    }
}
