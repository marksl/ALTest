using System.Configuration;

namespace ALTest.Core.Configuration
{
    public class AssemblyConfigCollection : ConfigurationElementCollection
    {
        public AssemblyConfigElement this[int index]
        {
            get { return (AssemblyConfigElement)BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        public void Add(AssemblyConfigElement serviceConfig)
        {
            BaseAdd(serviceConfig);
        }

        public void Clear()
        {
            BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new AssemblyConfigElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((AssemblyConfigElement)element).Folder;
        }

        public void Remove(AssemblyConfigElement serviceConfig)
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