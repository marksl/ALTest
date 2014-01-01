using System.Configuration;

namespace AltMstestGui.Configuration
{
    public class FolderConfigCollection : ConfigurationElementCollection
    {
        public FolderConfigElement this[int index]
        {
            get { return (FolderConfigElement)BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        public void Add(FolderConfigElement serviceConfig)
        {
            BaseAdd(serviceConfig);
        }

        public void Clear()
        {
            BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new FolderConfigElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((FolderConfigElement)element).Folder;
        }

        public void Remove(FolderConfigElement serviceConfig)
        {
            BaseRemove(serviceConfig.Folder);
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        public void Remove(string name)
        {
            BaseRemove(name);
        }
    }
}