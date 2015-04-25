using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace ProcessMonitor
{
    public partial class Test : Form
    {
        StreamWriter output;

        public Test()
        {
            InitializeComponent();
            output = new StreamWriter(new FileStream(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "processes.csv"), FileMode.Append));
            tmrStep_Tick(this, EventArgs.Empty);
        }

        private void tmrStep_Tick(object sender, EventArgs e)
        {
            Hide();

            var processes = Process.GetProcesses();
            var names = "";

            foreach (var process in processes)
            {
                if (process.MainWindowTitle != "")
                names += DateTime.Now.ToShortDateString() + "," + DateTime.Now.ToShortTimeString() + "," + process.ProcessName + ",\"" + process.MainWindowTitle + "\"" + Environment.NewLine;
            }

            output.Write(names);
            output.Flush();
        }

        private void Test_Resize(object sender, EventArgs e)
        {
            Hide();
        }
    }
}
