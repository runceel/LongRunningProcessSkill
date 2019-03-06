using System;
using System.Collections.Generic;
using System.Text;

namespace LongRunningProcessSkill.Models
{
    public class SmartSpeakerIntent
    {
        public string IntentName { get; set; }
        public IDictionary<string, string> Entities { get; set; }
    }
}
