using System;
using System.Collections.Generic;
using System.Text;

namespace LongRunningProcessSkill.Models
{
    public class SmartSpeakerRequest
    {
        public string Platform { get; set; }
        public SmartSpeakerRequestType RequestType { get; set; }
        public SmartSpeakerUser User { get; set; }
        public SmartSpeakerIntent Intent { get; set; }
    }
}
