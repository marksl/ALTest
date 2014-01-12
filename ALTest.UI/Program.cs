using System;
using System.Windows.Forms;
using Menu = ALTest.UI.Menu;

namespace ALTest.UI
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
