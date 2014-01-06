using System;
using System.Windows.Forms;

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
