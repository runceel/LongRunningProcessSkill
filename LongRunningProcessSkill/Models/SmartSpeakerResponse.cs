using System;
using System.Collections.Generic;
using System.Text;

namespace LongRunningProcessSkill.Models
{
    public class SmartSpeakerResponse
    {
        public string ResponseMessage { get; set; }
        public bool ShouldEndSession { get; set; }
    }
}
