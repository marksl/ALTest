using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Windows.Forms;
using AltMstestGui.Configuration;
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

            var serviceConfigSection = ConfigurationManager.GetSection("FolderSection") as AltMstestSection;
            var menuItem1 = new MenuItem
            {
                Index = 0,
                Text = "Run Tests"
            };
            menuItem1.Click += (sender, e) =>
                                   {
                                       IList<ISyncedDestination> synced = FolderSync.Sync(serviceConfigSection);

                                       var assemblies = synced.SelectMany(a => a.AssembliesWithFullPath);
                                       TestRunnerLauncher.LoadAssembliesAndRunTests(assemblies);   
                                   };

            var contextMenu1 = new ContextMenu();
            contextMenu1.MenuItems.AddRange(new[] { menuItem1 });

            new NotifyIcon
            {
                Icon = Resources.dot,
                ContextMenu = contextMenu1,
                Text = "AltMsTest",
                Visible = true
            };

            Application.Run();        
        }
    }
}
