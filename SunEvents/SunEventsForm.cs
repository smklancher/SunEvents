using DesktopToast;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SunEvents
{
    public partial class SunEventsForm : Form
    {
        private EventProcessor ep = new EventProcessor();
        private double lat = 33.589952;
        private double lon = -117.586788;

        #region "Start up"

        public SunEventsForm()
        {
            //form icon: http://www.iconsplace.com/orange-icons/sun-icon
            InitializeComponent();
        }

        public void AddDefaultEvents()
        {
            //Could add interface to edit and persist in the future
            ep.Events.Add(new SunEvent
            {
                Name = "Sleep 8 hours before sunrise (civil)",
                IsSunrise = true,
                Offset = new TimeSpan(-8, 0, 0),
                Command = "psshutdown",
                CommandArgs = "-d -t 0",
                RetryPeriod = new TimeSpan(1, 0, 0),
                IsCivil = true
            });

            //Test event to fire 5 seconds after start
            Twilight c = new Twilight();
            SunAndMoonData Tomorrow = c.GetData(DateTime.Now.Date, lat, lon, (DateTime.Now - DateTime.UtcNow).TotalHours);
            TimeSpan OffsetForImmediateEvent = Tomorrow.CivalTwilightStart - DateTime.Now.AddSeconds(5);

            ep.Events.Add(new SunEvent
            {
                Name = "Immediate Test event",
                IsSunrise = true,
                Offset = OffsetForImmediateEvent.Negate(),
                RetryPeriod = new TimeSpan(0, 1, 0),
                IsCivil = true,
                Disabled = false
            });
        }

        private async Task<string> ShowToastAsync(string msg)
        {
            var request = new ToastRequest
            {
                ToastTitle = msg,
                ToastBodyList = new[] { msg },
                //ToastAudio = DesktopToast.ToastAudio.LoopingCall,
                ShortcutFileName = "SunEvents.lnk",
                ShortcutTargetFilePath = Assembly.GetExecutingAssembly().Location,
                AppId = "SunEvents",
            };

            request.ToastXml = @"<toast launch='action=viewFriendRequest&amp;userId=49183'>

  <visual>
    <binding template='ToastGeneric'>
      <text>Matt sent you a friend request</text>
      <text>Hey, wanna dress up as wizards and ride around on our hoverboards together?</text>
      <image placement='appLogoOverride' hint-crop='circle' src='https://unsplash.it/64?image=1005'/>
    </binding>
  </visual>

  <actions>
    <action content='Accept' activationType='background' arguments='action=acceptFriendRequest&amp;userId=49183'/>
    <action content='Decline' activationType='background' arguments='action=declineFriendRequest&amp;userId=49183'/>
  </actions>

</toast>";

            var result = await ToastManager.ShowAsync(request);

            return result.ToString();
        }


        protected override void OnShown(EventArgs e)
        {
            //Form is never actually closed, just minimized with no taskbar entry, so this fires only once
            base.OnShown(e);
            AddDefaultEvents();
            UpdateSummary();
            EventTimer.Interval = 100; //Timer tick resets to longer interval
            EventTimer.Enabled = true;


            //testing notifications

            // For Action Center of Windows 10
            //NotificationActivator.RegisterComType(typeof(NotificationActivator), OnActivated);

            //NotificationHelper.RegisterComServer(typeof(NotificationActivator), Assembly.GetExecutingAssembly().Location);
            //NotificationHelper.UnregisterComServer(typeof(NotificationActivator));

            //var x = new Notification();
            //x.SimpleNotification("Test Message");

            //var task = await ShowToastAsync("Test Message");
            //MessageBox.Show(task);
        }


        #endregion

        #region "Tray Icon"

        private void TrayIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            RestoreWindow();
        }

        private void RestoreWindow()
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Show();
                WindowState = FormWindowState.Normal;
            }
            
            ShowInTaskbar = true;

            Activate();
            Focus();
        }

        private void HideWindow()
        {

        }

        private void SunEventsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (WindowState!=FormWindowState.Minimized)
            {
                e.Cancel = true;
                WindowState = FormWindowState.Minimized;
                ShowInTaskbar = false;
            }
        }

        //protected override void OnFormClosing(FormClosingEventArgs e)
        //{
        //    base.OnFormClosing(e);
        //}

        private void showToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            RestoreWindow();
        }

        private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
            Application.Exit();
        }

        #endregion

        private void EventTimer_Tick(object sender, EventArgs e)
        {
            EventTimer.Interval = 15000;
            ep.ProcessEvents(lat, lon);

            if (WindowState != FormWindowState.Minimized)
            {
                UpdateSummary();
            }
        }


        public void UpdateSummary()
        {
            Twilight c = new Twilight();

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Sunrise and Sunset Data\r\n");

            // Get back the data
            SunAndMoonData dataToday = c.GetData(DateTime.Now.Date, lat, lon, (DateTime.Now - DateTime.UtcNow).TotalHours);
            SunAndMoonData dataTomorrow = c.GetData(DateTime.Now.AddDays(1).Date, lat, lon, (DateTime.Now.AddDays(1) - DateTime.UtcNow.AddDays(1)).TotalHours);

            sb.AppendFormat("Today\r\n");
            c.RenderData(0, dataToday, sb);

            sb.AppendFormat("Tomorrow\r\n");
            c.RenderData(0, dataTomorrow, sb);

            sb.Append(Environment.NewLine + "Next Event: " + (ep.NextEvent != null ? ep.NextEvent.ToString() : "None"));

            sb.Append(Environment.NewLine + Environment.NewLine + ep.ToString());

            MainTextBox.Text = sb.ToString();

            if(ep.NextEvent != null)
            {
                TrayIcon.Text = "Next Event: " + ep.NextEvent.Time();
            }
        }
    }
}
