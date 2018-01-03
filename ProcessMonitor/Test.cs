using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
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

        private void Test_Resize(object sender, EventArgs e)
        {
            Hide();
        }

        private void tmrStep_Tick(object sender, EventArgs e)
        {
            Hide();

            var processes = Process.GetProcesses();
            var names = new StringBuilder();

            var windowHandle = GetForegroundWindow();
            var currentProcess = GetProcessByHandle(windowHandle);

            using (var writer = GetStreamWriter())
            {
                if (writer == null)
                {
                    return;
                }

                try
                {
                    foreach (var process in processes)
                    {
                        var isCurrent = currentProcess != null &&
                                        process.ProcessName.Equals(currentProcess.ProcessName);

                        AddProcessData(ref names, process, isCurrent);

                        var line = names.ToString();
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            writer.WriteLine(names.ToString());
                            writer.Flush();
                        }

                        names.Clear();
                    }
                }
                catch (IOException)
                {
                }
                finally
                {
                    writer.Close();
                }
            }
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern Int32 GetWindowThreadProcessId(IntPtr windowHandle, out uint lpdwProcessId);

        private static Process GetProcessByHandle(IntPtr windowHandle)
        {
            try
            {
                GetWindowThreadProcessId(windowHandle, out uint processId);
                return Process.GetProcessById((int)processId);
            }
            catch
            {
                return null;
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SendMessageTimeout(IntPtr windowHandle, uint Msg, IntPtr wParam, IntPtr lParam, uint fuFlags, uint uTimeout, out IntPtr lpdwResult);

        private StreamWriter GetStreamWriter()
        {
            var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var path = Path.Combine(desktop, "processes.csv");

            try
            {
                var fileStream = new FileStream(path, FileMode.Append);
                return new StreamWriter(fileStream);
            }
            catch (IOException)
            {
                return null;
            }
        }

        private static void AddProcessData(ref StringBuilder names, Process process, bool isCurrent)
        {
            var processName = string.Empty;
            var fileDescription = string.Empty;

            try
            {
                fileDescription = process.MainModule.FileVersionInfo.FileDescription;
            }
            catch (Win32Exception)
            {
            }
            catch (InvalidOperationException)
            {
            }

            if (!string.IsNullOrWhiteSpace(process.MainWindowTitle))
            {
                processName = process.MainWindowTitle;
            }
            else if (!string.IsNullOrWhiteSpace(fileDescription))
            {
                processName = fileDescription;
            }
            else
            {
                processName = process.ProcessName;
            }

            var windowTitle = processName.Replace(',', ':');

            names.Append(DateTime.Now.ToShortDateString());
            names.Append(",");

            names.Append(DateTime.Now.ToShortTimeString());
            names.Append(",");

            names.Append(process.ProcessName);
            names.Append(",");

            names.Append(windowTitle);
            names.Append(",");

            names.Append(isCurrent ? "*" : "");
        }
    }
}
