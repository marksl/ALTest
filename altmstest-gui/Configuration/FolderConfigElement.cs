using System;
using System.Collections.Generic;
using System.Configuration;

namespace AltMstestGui.Configuration
{
    public class FolderConfigElement : ConfigurationElement
    {
        public FolderConfigElement() { }

        public FolderConfigElement(string folder, string assemblies)
            :this()
        {
            Folder = folder;
            Assemblies = assemblies;
        }

        [ConfigurationProperty("Folder", IsRequired = true, IsKey = true)]
        public string Folder
        {
            get { return (string)this["Folder"]; }
            set { this["Folder"] = value; }
        }

        internal IList<string> AssemblyNames
        {
            get { return Assemblies.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries); }
        }

        [ConfigurationProperty("Assemblies", IsRequired = true, IsKey = false)]
        public string Assemblies
        {
            get { return (string)this["Assemblies"]; }
            set { this["Assemblies"] = value; }
        }
    }
}