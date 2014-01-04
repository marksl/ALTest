using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using AltMstest.Core;
using AltMstest.Core.Configuration;
using AltMstestGui.Properties;

namespace AltMstestGui
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            new Menu();
            
            Application.Run();        
        }
    }
}
