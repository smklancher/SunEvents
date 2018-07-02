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
        public bool Disabled;
        private readonly double EPSILON=0.0001;

        public override string ToString()
        {
            String OffsetString = (Math.Abs(Offset.TotalSeconds) > EPSILON ? (Offset.TotalSeconds < 0 ? $"{Offset} before " : $"{Offset} after ") : "");

            return (EventFired ? "[EventFired] " : "") + 
                (Disabled ? "[Disabled]" : "") + 
                $"\"{Name}\" is set for " + OffsetString +
                (IsCivil ? "civil " : "") + 
                (IsSunrise ? "sunrise " : "sunset ") +
                (TargetTime.HasValue ? $"is scheduled for {TargetTime}." : "is not scheduled yet.") +
                $"  It will retry for {RetryPeriod}" + 
                (!String.IsNullOrEmpty(Command) ? $" to run command: {Command} {CommandArgs}" : "") + ".";
        }

        public string Time()
        {
            return (TargetTime.HasValue ? 
                TargetTime.Value.ToShortTimeString() + " " + TargetTime.Value.ToShortDateString()
                : "No Target Time");
        }
    }
}
