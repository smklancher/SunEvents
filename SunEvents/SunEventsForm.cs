using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SunEvents
{
    public partial class SunEventsForm : Form
    {
        public SunEventsForm()
        {
            //form icon: http://www.iconsplace.com/orange-icons/sun-icon
            InitializeComponent();
        }

        public void SetInitialData()
        {
            // Determine the # of hours that the local time is different from UTC/GMT
            TimeSpan ts = DateTime.Now - DateTime.UtcNow;

            // Get the number of hours difference
            double TimeZoneDifferenceFromUTC = ts.TotalHours;

            // Get the Latitude
            double lat = 33.589952;

            // Get the Longtitude
            double lon = -117.586788;

            // Creae instance of the class
            Twilight c = new Twilight();

            // Get back the data
            SunAndMoonData dataToday = c.GetData(DateTime.Now.Date, lat, lon, TimeZoneDifferenceFromUTC);


            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Sunrise and Sunset Data\r\n");
            sb.AppendFormat("Today\r\n");

            c.RenderData(0, dataToday, sb);

            MainTextBox.Text = sb.ToString();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            SetInitialData();
        }

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
            WindowState = FormWindowState.Normal;
            Application.Exit();
        }

#endregion
    }
}
