using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RustLanguageExtension
{
    internal class OptionsModel
    {
        private const string SettingsCollection = "RustLanguageExtension";
        private const string ToolchainDefault = "nightly";
        private const string ToolchainProperty = "Toolchain";

        private static string toolchain;
        public static string Toolchain
        {
            get
            {
                if (toolchain == null)
                {
                    LoadData();
                }

                return toolchain;
            }
            set
            {
                toolchain = value;
            }
        }

        public static void LoadData()
        {
            var settingsManager = new ShellSettingsManager(ServiceProvider.GlobalProvider);
            var userSettingsStore = settingsManager.GetWritableSettingsStore(Microsoft.VisualStudio.Settings.SettingsScope.UserSettings);
            EnsureSettingsStore(userSettingsStore);

            toolchain = userSettingsStore.GetString(SettingsCollection, ToolchainProperty);
        }

        public static void SaveData()
        {
            var settingsManager = new ShellSettingsManager(ServiceProvider.GlobalProvider);
            var userSettingsStore = settingsManager.GetWritableSettingsStore(Microsoft.VisualStudio.Settings.SettingsScope.UserSettings);
            EnsureSettingsStore(userSettingsStore);

            userSettingsStore.SetString(SettingsCollection, ToolchainProperty, toolchain);
        }

        private static void EnsureSettingsStore(WritableSettingsStore settingsStore)
        {
            if (!settingsStore.CollectionExists(SettingsCollection))
            {
                settingsStore.CreateCollection(SettingsCollection);
            }

            if (!settingsStore.PropertyExists(SettingsCollection, ToolchainProperty))
            {
                settingsStore.SetString(SettingsCollection, ToolchainProperty, ToolchainDefault);
            }
        }
    }
}
