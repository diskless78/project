using System;
using System.Threading;
using System.Windows.Forms;

namespace mtv
{
    internal static class Program
    {
        private static Mutex? mutex;

        [STAThread]
        static void Main()
        {
            bool createdNew;
            string mutexName = "FuckTheLifeToFindTheLuck_FindTheLuckThenFuckTheLife";

            // Try to create mutex
            mutex = new Mutex(true, mutexName, out createdNew);

            if (!createdNew)
            {
                // Another instance is running
                MessageBox.Show("⚠️ The application is already running.\nPlease double-click the icon in the system tray.",
                                "Application Running",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                return; // Exit this instance
            }

            // Normal startup
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());

            // Release when app exits
            mutex.ReleaseMutex();
        }
    }
}
