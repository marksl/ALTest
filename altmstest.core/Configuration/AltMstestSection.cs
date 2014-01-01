using System.Configuration;

namespace AltMstestGui.Configuration
{
    public class AltMstestSection : ConfigurationSection
    {
        [ConfigurationProperty("Folders", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(FolderConfigCollection),
            AddItemName = "add",
            ClearItemsName = "clear",
            RemoveItemName = "remove")]
        public FolderConfigCollection Folders
        {
            get
            {
                return (FolderConfigCollection)base["Folders"];
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
