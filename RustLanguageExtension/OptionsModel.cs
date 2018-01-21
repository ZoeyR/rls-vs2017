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

        private const string ToolchainDefault = "beta";
        private const string ToolchainProperty = "Toolchain";

        private const string RustupPathDefault = "";
        private const string RustupPathProperty = "RustupPath";

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

        private static string rustupPath;
        public static string RustupPath
        {
            get
            {
                if (rustupPath == null)
                {
                    LoadData();
                }

                return rustupPath;
            }
            set
            {
                rustupPath = value;
            }
        }

        public static void LoadData()
        {
            var settingsManager = new ShellSettingsManager(ServiceProvider.GlobalProvider);
            var userSettingsStore = settingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
            EnsureSettingsStore(userSettingsStore);

            toolchain = userSettingsStore.GetString(SettingsCollection, ToolchainProperty);
            rustupPath = userSettingsStore.GetString(SettingsCollection, RustupPathProperty);
        }

        public static void SaveData()
        {
            var settingsManager = new ShellSettingsManager(ServiceProvider.GlobalProvider);
            var userSettingsStore = settingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
            EnsureSettingsStore(userSettingsStore);

            userSettingsStore.SetString(SettingsCollection, ToolchainProperty, toolchain);
            userSettingsStore.SetString(SettingsCollection, RustupPathProperty, rustupPath);
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

            if (!settingsStore.PropertyExists(SettingsCollection, RustupPathProperty))
            {
                settingsStore.SetString(SettingsCollection, RustupPathProperty, RustupPathDefault);
            }
        }
    }
}
