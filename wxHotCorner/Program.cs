using System;
using System.Threading;
using System.Windows.Forms;

namespace HCWX
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static Mutex mutex = new Mutex(true, "{8F6F0AC4-B9A1-45fd-A8CF-72F04E6BDE8F}");
        [STAThread]
        static void Main()
        {
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                //Set working directory for normal autorun from registry key
                System.IO.Directory.SetCurrentDirectory(System.IO.Path.GetDirectoryName(Application.ExecutablePath));
                Application.Run(new SettngsForm());
                mutex.ReleaseMutex();
            }
            else
            {
                SendHotKey.KeyDown(Keys.LWin);
                SendHotKey.KeyDown(Keys.F5);
                SendHotKey.KeyUp(Keys.LWin);
                SendHotKey.KeyUp(Keys.F5);
            }
        }
    }
}
