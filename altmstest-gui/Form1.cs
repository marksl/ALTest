using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using AltMstestGui.Configuration;
using System.Linq;

namespace AltMstestGui
{
    public partial class Form1 : Form
    {
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.ContextMenu contextMenu1;
        private System.Windows.Forms.MenuItem menuItem1;

        private AltMstestSection serviceConfigSection;
        
        protected override void OnLoad(EventArgs e)
        {
            this.Hide();
            base.OnActivated(e);
        }

        
        public Form1()
        {
           
            InitializeComponent();

            this.contextMenu1 = new System.Windows.Forms.ContextMenu();
            this.menuItem1 = new System.Windows.Forms.MenuItem();

            // Initialize contextMenu1 
            this.contextMenu1.MenuItems.AddRange(
                        new System.Windows.Forms.MenuItem[] { this.menuItem1 });

            // Initialize menuItem1 
            this.menuItem1.Index = 0;
            this.menuItem1.Text = "Run Tests";
            this.menuItem1.Click += RunTests;



            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon();

            // The Icon property sets the icon that will appear 
            // in the systray for this application.

            serviceConfigSection =
   ConfigurationManager.GetSection("FolderSection") as AltMstestSection;


            notifyIcon1.Icon = AltMstestGui.Properties.Resources.dot;

            // The ContextMenu property sets the menu that will 
            // appear when the systray icon is right clicked.
            notifyIcon1.ContextMenu = this.contextMenu1;

            // The Text property sets the text that will be displayed, 
            // in a tooltip, when the mouse hovers over the systray icon.
            notifyIcon1.Text = "Alt Mstest";
            notifyIcon1.Visible = true;
        }

        void RunTests(object sender, EventArgs e)
        {
            FolderSync.Sync(serviceConfigSection);

            // Maybe only test one DLL at a time... I think that makes sense.
            var assemblies = new List<string>();

            foreach (FolderConfigElement folder in serviceConfigSection.Folders)
            {
                var folderAssemblies = folder.Assemblies.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);

                foreach (var folderAssembly in folderAssemblies)
                {
                    var fullPath = Path.Combine(folder.Folder, folderAssembly);
                    
                    assemblies.Add(fullPath);
                }
            }

            foreach (var assembly in assemblies)
            {
                var name = Path.GetFileNameWithoutExtension(assembly);
                var directoryName = Path.GetDirectoryName(assembly);
                var dirPath = Path.GetFullPath(directoryName);

                var setup = new AppDomainSetup
                                {
                                    ApplicationBase = AppDomain.CurrentDomain.BaseDirectory,
                                    PrivateBinPath = dirPath,
                                    ShadowCopyFiles = "true",
                                    ShadowCopyDirectories = dirPath,
                                };

                AppDomain domain = AppDomain.CreateDomain(name + "Domain", AppDomain.CurrentDomain.Evidence, setup);

                var scannerType = typeof (TestRunner);
                var testRunner = (TestRunner)domain.CreateInstanceAndUnwrap(scannerType.Assembly.FullName, scannerType.FullName);

                testRunner.RunTests(assembly);

                AppDomain.Unload(domain);
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}
