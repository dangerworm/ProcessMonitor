using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace ProcessMonitor
{
    public partial class Test : Form
    {
        public Test()
        {
            InitializeComponent();
            tmrStep_Tick(this, EventArgs.Empty);
        }

        private void tmrStep_Tick(object sender, EventArgs e)
        {
            Hide();

            var processes = Process.GetProcesses();
            var names = "";

            try
            {
                StreamWriter output = new StreamWriter(new FileStream(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "processes.csv"), FileMode.Append));

                IntPtr hwnd = GetForegroundWindow();
                Process currentProcess = hwnd != null ? GetProcessByHandle(hwnd) : null;

                foreach (var process in processes)
                {
                    if (process.MainWindowTitle != "")
                    {
                        names += string.Format("{0},{1},{2},{3},{4}{5}", DateTime.Now.ToShortDateString(),
                                                                         DateTime.Now.ToShortTimeString(),
                                                                         process.ProcessName,
                                                                         process.MainWindowTitle.Replace(',', ':'),
                                                                         currentProcess != null && process.ProcessName == currentProcess.ProcessName ? "*" : "",
                                                                         Environment.NewLine);
                    }
                }

                output.Write(names);
                output.Close();
            }
            catch (IOException)
            {
            }
        }

        private void Test_Resize(object sender, EventArgs e)
        {
            Hide();
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern Int32 GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        private static Process GetProcessByHandle(IntPtr hwnd)
        {
            try
            {
                uint processID;
                GetWindowThreadProcessId(hwnd, out processID);
                return Process.GetProcessById((int)processID);
            }
            catch
            {
                return null;
            }
        }
    }
}
