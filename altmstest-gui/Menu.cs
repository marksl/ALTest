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
    // Needs to be updated from the test run....

    // When test run starts...

    // When test runs ends..

    public class Menu : ContextMenu
    {
        private readonly AltMstestSection serviceConfigSection;

        public Menu()
        {
            serviceConfigSection = ConfigurationManager.GetSection("AssemblySection") as AltMstestSection;

            foreach (AssemblyConfigElement assemblyConfig in serviceConfigSection.Assemblies)
            {
                var menuItem = new MenuItem
                                   {
                                       Text = string.Format("Run {0} tests", assemblyConfig.FileName)
                                   };
                AssemblyConfigElement config = assemblyConfig;
                menuItem.Click += (a, b) => RunTests(serviceConfigSection.Destination,
                                                     new List<AssemblyConfigElement> { config });
                MenuItems.Add(menuItem);
            }

            var runAllTestsMenuItem = new MenuItem
                                          {
                                              Text = "Run all tests"
                                          };
            runAllTestsMenuItem.Click += (a, b) => RunTests(serviceConfigSection.Destination,
                                                            serviceConfigSection.AssemblyList);

            var dash = new MenuItem
                           {
                               Text = "-"
                           };

            var closeMenuItem = new MenuItem
                                    {
                                        Text = "Exit"
                                    };
            closeMenuItem.Click += (sender, e) => Application.Exit();

            MenuItems.AddRange(new[] {runAllTestsMenuItem, dash, closeMenuItem});

            _notifyIcon = new NotifyIcon
            {
                Icon = Resources.dot,
                ContextMenu = this,
                Text = "AltMsTest",
                Visible = true
            };
        }

        private NotifyIcon _notifyIcon;

        void RunTests(string destination, IList<AssemblyConfigElement> assemblyList)
        {
            var t = new Thread(
                       () =>
                           {
                               _notifyIcon.Icon = Resources.dotorange;

                               foreach (MenuItem m in MenuItems)
                               {
                                   if (m.Text != "Exit")
                                   {
                                       m.Enabled = false;
                                   }
                               }
                           IList<ISyncedDestination> synced = FolderSync.Sync(destination,
                                                                              assemblyList);

                           var assemblies = synced.SelectMany(a => a.AssembliesWithFullPath);
                           TestRunnerLauncher.LoadAssembliesAndRunTests(assemblies);
                           _notifyIcon.Icon = Resources.dot;
                               foreach (MenuItem m in MenuItems)
                               {
                                   m.Enabled = true;
                               }
                           }
                       ) { Name = "My test" };

            t.Start();
        }
    }
}
