using System.Configuration;

namespace AltMstest.Core.Configuration
{
    public class AssemblyConfigElement : ConfigurationElement
    {
        public string Folder
        {
            get
            {
                int lastDash = Assembly.LastIndexOf('\\');
                return Assembly.Substring(0, lastDash);
            }
        }

        internal string FileName
        {
            get
            {
                int lastDash = Assembly.LastIndexOf('\\');
                return Assembly.Substring(lastDash + 1, Assembly.Length - lastDash - 1);
            }
        }

        [ConfigurationProperty("Assembly", IsRequired = true, IsKey = false)]
        public string Assembly
        {
            get { return (string)this["Assembly"]; }
            set { this["Assembly"] = value; }
        }
    }
}