using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace ALTest.Core.Configuration
{
    public class ALTestSection : ConfigurationSection
    {
        [ConfigurationProperty("Assemblies", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(AssemblyConfigCollection),
            AddItemName = "add",
            ClearItemsName = "clear",
            RemoveItemName = "remove")]
        public AssemblyConfigCollection Assemblies
        {
            get
            {
                return (AssemblyConfigCollection)base["Assemblies"];
            }
        }

        public IList<AssemblyConfigElement> AssemblyList
        {
            get { return Assemblies.Cast<AssemblyConfigElement>().ToList(); }
        }

        [ConfigurationProperty("Destination", IsRequired = true)]
        public string Destination
        {
            get { return (string)this["Destination"]; }
            set { this["Destination"] = value; }
        }

        [ConfigurationProperty("TestAssembly", IsRequired = false, DefaultValue = "ALTest.MsTest.dll")]
        public string TestAssembly
        {
            get { return (string)this["TestAssembly"]; }
            set { this["TestAssembly"] = value; }
        }
    }
}
