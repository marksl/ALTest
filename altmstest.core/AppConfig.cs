using System;
using System.Configuration;
using System.Linq;
using System.Reflection;

namespace AltMstest.Core
{
    // From: http://stackoverflow.com/questions/6150644/change-default-app-config-at-runtime
    public abstract class AppConfig : IDisposable
    {
        public static AppConfig Change(string path)
        {
            return new ChangeAppConfig(path);
        }

        public abstract void Dispose();

        private class ChangeAppConfig : AppConfig
        {
            private readonly string oldConfig =
                AppDomain.CurrentDomain.GetData("APP_CONFIG_FILE").ToString();

            private bool disposedValue;

            private readonly string _path;

            public ChangeAppConfig(string path)
            {
                _path = path;
                if (path != null)
                {
                    AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", path);
                    ResetConfigMechanism();
                }
            }

            public override void Dispose()
            {
                if (!disposedValue)
                {
                    if (_path != null)
                    {
                        AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", oldConfig);
                        ResetConfigMechanism();
                    }

                    disposedValue = true;
                }
                GC.SuppressFinalize(this);
            }

            private static void ResetConfigMechanism()
            {
// ReSharper disable PossibleNullReferenceException
                typeof (ConfigurationManager)
                    .GetField("s_initState", BindingFlags.NonPublic |
                                             BindingFlags.Static)
                    .SetValue(null, 0);

                typeof (ConfigurationManager)
                    .GetField("s_configSystem", BindingFlags.NonPublic |
                                                BindingFlags.Static)
                    .SetValue(null, null);

                typeof (ConfigurationManager)
                    .Assembly.GetTypes().First(x => x.FullName ==
                                                    "System.Configuration.ClientConfigPaths")
                    .GetField("s_current", BindingFlags.NonPublic |
                                           BindingFlags.Static)
                    .SetValue(null, null);
                // ReSharper restore PossibleNullReferenceException
            }
        }
    }
}