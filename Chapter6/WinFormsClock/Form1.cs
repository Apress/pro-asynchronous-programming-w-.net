using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsClock
{
    public partial class ClockForm : Form
    {
        private Timer timer = new Timer();
        public ClockForm()
        {
            InitializeComponent();

            timer.Interval = 1000; // 1000 ms == 1 second
            timer.Tick += UpdateFormTitleWithCurrentTime;

        
            //timer.Enabled = true; or
            timer.Start();
       
        }

        private void UpdateFormTitleWithCurrentTime(object sender, EventArgs e)
        {
            this.Text = DateTime.Now.ToString();
        }
    }
}
