using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ALTest.Core;
using ALTest.Core.Configuration;
using ALTest.UI.Properties;

namespace ALTest.UI
{
    public class Menu : ContextMenu
    {
        private const string AppName = "ALTest";

        private readonly MenuItem _cancel;
        private readonly MenuItem _lastRun;
        private readonly NotifyIcon _notifyIcon;
        private readonly ALTestSection serviceConfigSection;

        public Menu()
        {
            serviceConfigSection = ConfigurationManager.GetSection("AssemblySection") as ALTestSection;

            // Run a tests from a specific assembly
            {
                var parentRun = new MenuItem {Text = "Run"};

                foreach (AssemblyConfigElement assemblyConfig in serviceConfigSection.AssemblyList)
                {
                    var runAssemblyTests = new MenuItem {Text = string.Format("Run {0} tests", assemblyConfig.FileName)};

                    AssemblyConfigElement config = assemblyConfig;
                    runAssemblyTests.Click += (a, b) => RunTests(serviceConfigSection.Destination,
                                                                 new List<AssemblyConfigElement> {config});
                    parentRun.MenuItems.Add(runAssemblyTests);
                }
                MenuItems.Add(parentRun);
            }

            // Run a category of tests
            {
                var categories = serviceConfigSection.AssemblyList.Select(c => c.Category).Distinct().ToList();
                foreach (var cat in categories)
                {
                    var runCategoryTests = new MenuItem {Text = string.Format("Run {0}", cat)};

                    List<AssemblyConfigElement> categoryAssemblies = serviceConfigSection.AssemblyList.Where(c => c.Category == cat).ToList();
                    runCategoryTests.Click += (a, b) => RunTests(serviceConfigSection.Destination,
                                                                 categoryAssemblies);
                    MenuItems.Add(runCategoryTests);
                }
            }

            // Run all or cancel tests
            {
                var runAllTestsMenuItem = new MenuItem {Text = "Run all tests"};
                runAllTestsMenuItem.Click += (a, b) => RunTests(serviceConfigSection.Destination,
                                                                serviceConfigSection.AssemblyList);

                var dash0 = new MenuItem {Text = "-"};
                var dash1 = new MenuItem {Text = "-"};
                var closeMenuItem = new MenuItem {Text = "Exit"};
                closeMenuItem.Click += (sender, e) => Application.Exit();

                _lastRun = new MenuItem {Visible = false};
                _cancel = new MenuItem { Text = "Cancel" };
                _cancel.Click += (sender, e) => Cancel();
                _lastRun.MenuItems.Add(_cancel);

                MenuItems.AddRange(new[] {dash0, runAllTestsMenuItem, _lastRun, dash1, closeMenuItem});
            }

            _notifyIcon = new NotifyIcon
                              {
                                  Icon = Resources.dotgreen,
                                  ContextMenu = this,
                                  Text = AppName,
                                  Visible = true,
                              };
        }

        private void Cancel()
        {
            if (_launcher != null)
            {
                _launcher.Cancel();
                _launcher = null;
            }
        }

        private TestRunner _launcher;

        private void RunTests(string destination, IList<AssemblyConfigElement> assemblyList)
        {
            var startTime = DateTime.Now;
            var text = "Current Run - Started " + startTime.ToString("hh:mmtt");

            _lastRun.Text = text;
            _lastRun.Visible = true;
            _lastRun.MenuItems.Clear();
            _lastRun.MenuItems.Add(_cancel);

            _notifyIcon.BalloonTipTitle = "Test run started";
            _notifyIcon.BalloonTipText = text;
            _notifyIcon.ShowBalloonTip(2);
            _notifyIcon.Text = AppName + " - " + text;
            _notifyIcon.Icon = Resources.dotorange;

            DisableAllMenuItems();

            var configuration = new RuntimeConfiguration(destination, assemblyList, serviceConfigSection.TestAssembly, true, null);

            _launcher = new TestRunner();
            _launcher.Finished += Finished;
            _launcher.Start(startTime, configuration);
        }

        private bool firstTime = true;

        void Finished(object sender, TestRunnerFinishedEventArgs e)
        {
            EnableAllMenuItems();

            var text = "Last run - " + e.StartTime.ToString("hh:mmtt") + " - " + e.ElapsedDisplay + " elapsed";

            _lastRun.Text = text; 
            _lastRun.MenuItems.Clear();

            if (e.Failures.Count > 0)
            {
                // red dot
                _notifyIcon.Icon = Resources.dotred;
                _notifyIcon.BalloonTipTitle = "Test run failed";

                var errorDetails = new StringBuilder();
                int count = 0;
                foreach (var f in e.Failures)
                {
                    if (count++ > 20)
                    {
                        var moreErrors = string.Format("There are {0} failures total.", e.Failures.Count);
                        var onlyShowFirst20 = new MenuItem { Text = moreErrors };
                        onlyShowFirst20.Click += (a, b) => Clipboard.SetText(((MenuItem)a).Text);

                        _lastRun.MenuItems.Add(onlyShowFirst20);

                        errorDetails.Append(moreErrors).AppendLine();

                        break;
                    }

                    var menuItem = new MenuItem { Text = f.TestName };
                    menuItem.Click += (a, b) =>
                                          {
                                              var clipboardText = ((MenuItem) a).Text;
                                              Clipboard.SetText(clipboardText);

                                              if (firstTime)
                                              {
                                                  firstTime = false;
                                                  _notifyIcon.BalloonTipTitle = "Clipboard";
                                                  _notifyIcon.BalloonTipText = string.Format("[{0}] was copied to the clip board.", clipboardText);
                                                  _notifyIcon.ShowBalloonTip(2);
                                              }
                                          };

                    _lastRun.MenuItems.Add(menuItem);

                    errorDetails
                        .Append("Class: ").Append(f.ClassName).AppendLine()
                        .Append("Test: ").Append(f.TestName).AppendLine()
                        .Append("Exception:").AppendLine()
                        .Append(f.ExceptionString).AppendLine().Append("============").AppendLine().AppendLine();

                }

                var detailsMenu = new MenuItem { Text = "View Details" };
                detailsMenu.Click += (a, b) => new FailureDetails { ErrorDetails = errorDetails }.Show();
                _lastRun.MenuItems.Add(detailsMenu);
            }
            else
            {
                // green dot
                _notifyIcon.Icon = Resources.dotgreen;
                _notifyIcon.BalloonTipTitle = "Test run success";
            }

            _notifyIcon.BalloonTipText = text;
            _notifyIcon.Text = AppName + " - " + text;
            _notifyIcon.ShowBalloonTip(2);

            _launcher = null;
        }

        private void EnableAllMenuItems()
        {
            foreach (MenuItem m in MenuItems)
            {
                m.Enabled = true;
            }
        }

        private void DisableAllMenuItems()
        {
            foreach (MenuItem m in MenuItems)
            {
                if (m.Text != "Exit" && m.Text != "Cancel" && m != _lastRun)
                {
                    m.Enabled = false;
                }
            }
        }
    }
}