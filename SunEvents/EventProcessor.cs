using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace SunEvents
{
    class EventProcessor
    {
        public List<SunEvent> Events { get; } = new List<SunEvent> { };
        public SunEvent NextEvent { get; set; }

        public void ProcessEvents(double lat, double lon)
        {
            foreach (SunEvent se in Events)
            {
                //Calculate target if none is set
                if (!se.TargetTime.HasValue)
                {
                    SetTargetTime(se, lat, lon);
                    Debug.Print($"Initial target time set for \"{se.Name}\": {se.TargetTime}");
                }
                else
                {
                    //Caludlate new target if we are past the existing target and retry period
                    if (DateTime.Now > se.TargetTime + se.RetryPeriod)
                    {
                        SetTargetTime(se, lat, lon);

                        //Reset flag to enable
                        se.EventFired = false;

                        Debug.Print($"{DateTime.Now} is past {se.TargetTime + se.RetryPeriod}: New target time set for \"{se.Name}\": {se.TargetTime}");
                    }

                    //Fire event if within the retry period and it has not already fired
                    if (!se.EventFired && se.TargetTime < DateTime.Now && (se.TargetTime + se.RetryPeriod) > DateTime.Now)
                    {
                        FireEvent(se);
                    }
                }
                
            }

            //Update next event
            NextEvent = GetNextEvent();
        }

        private SunEvent GetNextEvent()
        {
            SunEvent closestEvent=null;

            foreach (SunEvent se in Events)
            {
                if (closestEvent == null)
                {
                    closestEvent = se;
                }
                else
                {
                    //current event is after now but closer than current closest
                    if(se.TargetTime.HasValue && se.TargetTime.Value>DateTime.Now && se.TargetTime.Value<closestEvent.TargetTime.Value)
                    {
                        closestEvent = se;
                    }
                }
            }
            return closestEvent;
        }

        private void FireEvent(SunEvent se)
        {
            if (se.Disabled)
            {
                Debug.Print($"Event \"{se.Name}\" NOT firing at {DateTime.Now} because it is DISABLED.");
                
                //Still set as fired so that it does not trigger again
                se.EventFired = true;
                return;
            }

            String msg= $"Event \"{se.Name}\" Fired at {DateTime.Now}";
            Debug.Print(msg);

            msg = $"Event \"{se.Name}\" wants to run cmd \"{se.Command} {se.CommandArgs}\"";

            //var x = new Notification();
            //var result=x.SimpleNotification(msg);

            se.EventFired = true;

            if (!String.IsNullOrEmpty(se.Command))
            {
                Process p = new Process();
                p.StartInfo.FileName = se.Command;
                p.StartInfo.Arguments = se.CommandArgs;

                p.Start();
            }

            //System.Windows.Forms.MessageBox.Show(msg);
        }
        

        /// <summary>
        /// Set the target time when the event should occur
        /// </summary>
        /// <param name="se"></param>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        private void SetTargetTime(SunEvent se, double lat, double lon)
        {
            // Create instance of the class
            Twilight c = new Twilight();

            //The offset will change how far forward we need to look for "today"
            //Example, late night already past offset from tommorow's sunrise: 
            //will actually need day after tomorrow to set correct target time
            //Offset is negated because ususally it is time before target, here it is time after now
            DateTime dayToGetData = DateTime.Now + se.Offset.Negate();

            //First look for a target today
            SunAndMoonData dataToday = c.GetData(dayToGetData.Date, lat, lon, (DateTime.Now - DateTime.UtcNow).TotalHours);
            DateTime possibleTarget = SunTimeByType(se, dataToday) + se.Offset;

            Debug.Print($"Possible target for {se.Name} ({se.Offset}): {possibleTarget}");

            //DECIDE: if time is during the retry period could either set it, causing the event to immediately fire, or push until tomorrow
            //Currently immediate firing event if intialized during retry period

            DateTime CutoffForEventToday = possibleTarget + se.RetryPeriod;

            //Target time (or retry period) is still in the future, so set event for today
            if (DateTime.Now<CutoffForEventToday)
            {
                se.TargetTime = possibleTarget;
            }
            else //target time is past the target and retry period, so look for tomorrow
            {
                dayToGetData = dayToGetData.AddDays(1);
                SunAndMoonData dataTomorrow = c.GetData(dayToGetData.Date, lat, lon, (DateTime.Now - DateTime.UtcNow).TotalHours);

                //BUG: Something is wrong here
                //Past Stephen should have been more descriptive.  As far as I know, I believe this is working.

                se.TargetTime= SunTimeByType(se, dataTomorrow) + se.Offset;
                Debug.Print($"Possible target for {se.Name} was past cutoff time of {CutoffForEventToday}, target for tomorrow: {se.TargetTime}");
            }
        }

        private DateTime SunTimeByType(SunEvent se, SunAndMoonData s)
        {
            return (se.IsSunrise ?
                (se.IsCivil ? s.CivalTwilightStart : s.SunRise) :
                (se.IsCivil ? s.CivalTwilightEnd : s.SunSet));
        }

        public override string ToString()
        {
            return "Events:" + Environment.NewLine + String.Join(Environment.NewLine,Events);
        }
    }
}
