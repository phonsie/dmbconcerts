using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace getMyTracksMD5sWin
{
    static class getMyTracksMD5sWin
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new getMyMD5sWin());
        }
    }
}
