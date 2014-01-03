using System.Configuration;

namespace AltMstest.Core.Configuration
{
    public class AltMstestSection : ConfigurationSection
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


        [ConfigurationProperty("Destination", IsRequired = true)]
        public string Destination
        {
            get { return (string)this["Destination"]; }
            set { this["Destination"] = value; }
        }
    }
}
