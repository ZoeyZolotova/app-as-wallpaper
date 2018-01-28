using BackgroundProgramForm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BackgroundProgramForm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Visible = false;
            ShowInTaskbar = false;
            WindowState = FormWindowState.Minimized;
            Hide();

            RunningWindows.GetOrCreateBackground();
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void toolStripMenuItem2_DropDownOpening(object sender, EventArgs e)
        {
            var windows = RunningWindows.GetOpenedWindows();

            if (windows.Any())
            {
                toolStripMenuItem2.DropDownItems.Clear();

                foreach (var kvp in windows)
                {
                    var moveToBackground = new ToolStripButton(kvp.Value.Title);
                    moveToBackground.Click += (o, args) =>
                    {
                        RunningWindows.SetParent(kvp.Key, RunningWindows.GetOrCreateBackground());
                        //backgroundWindows[kvp.Key] = kvp.Value;
                    };
                    toolStripMenuItem2.DropDownItems.Add(moveToBackground);
                }
            }
        }

        //private Dictionary<IntPtr, InfoWindow> backgroundWindows = new Dictionary<IntPtr, InfoWindow>();

        private void toolStripMenuItem3_DropDownOpening(object sender, EventArgs e)
        {
            var backgroundWindows = RunningWindows.GetBackgroundWindows();

            if (backgroundWindows.Any())
            {
                toolStripMenuItem3.DropDownItems.Clear();

                foreach (var kvp in backgroundWindows)
                {
                    var moveToForeground = new ToolStripButton(kvp.Value.Title);
                    moveToForeground.Click += (o, args) =>
                    {
                        RunningWindows.SetParent(kvp.Key, IntPtr.Zero);
                    };
                    toolStripMenuItem3.DropDownItems.Add(moveToForeground);
                }
            }
        }
    }
}
