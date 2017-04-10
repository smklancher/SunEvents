using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunEvents
{
    class SunEvent
    {
        public bool IsSunrise=true;     //otherwise sunset
        public bool IsCivil = false;            //otherwise sunrise/set
        public TimeSpan Offset;
        public TimeSpan RetryPeriod;
        public String Command;
        public String CommandArgs;
        public bool EventFired = false; //only relevant between TargetTime and RetryPeriod: After that the target needs to be recalculated for the next day
        public DateTime? TargetTime;
        public String Name;

        public override string ToString()
        {
            String OffsetString = (Offset.TotalSeconds != 0 ? (Offset.TotalSeconds < 0 ? $"{Offset} before " : $"{Offset} before ") : "");

            return (EventFired ? "[EventFired] " : "") + $"\"{Name}\" is set for " + OffsetString +
                (IsCivil ? "civil " : "") + (IsSunrise ? "sunrise " : "sunset ") +
                (TargetTime.HasValue ? $"is scheduled for {TargetTime}." : "is not scheduled yet.") +
                $"  It will retry for {RetryPeriod}" + (!String.IsNullOrEmpty(Command) ? $" to run command: {Command} {CommandArgs}" : "") + ".";
        }
    }
}
