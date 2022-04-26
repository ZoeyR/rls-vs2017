// <copyright file="OptionsModel.cs" company="Daniel Griffen">
// Copyright (c) Daniel Griffen. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RustLanguageExtension
{
    using System;
    using System.Composition;
    using Microsoft.VisualStudio.Settings;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.Shell.Settings;
    using IAsyncServiceProvider = Microsoft.VisualStudio.Shell.IAsyncServiceProvider;

    /// <summary>
    /// Data model for Rust extension options.
    /// </summary>
    [Export]
    public class OptionsModel
    {
        private const string SettingsCollection = "RustLanguageExtension";

        private const string ToolchainDefault = "nightly";
        private const string ToolchainProperty = "Toolchain";

        private const string RustupPathDefault = "";
        private const string RustupPathProperty = "RustupPath";

        private static string toolchain = ToolchainDefault;
        private static string rustupPath = RustupPathDefault;

        private readonly IServiceProvider syncServiceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionsModel"/> class.
        /// </summary>
        /// <param name="syncServiceProvider">VS service provider.</param>
        [ImportingConstructor]
        public OptionsModel([Import("Microsoft.VisualStudio.Shell.SVsServiceProvider")] IServiceProvider syncServiceProvider)
        {
            this.syncServiceProvider = syncServiceProvider;
        }

        /// <summary>
        /// Gets or sets the name of the default toolchain to use for the project.
        /// </summary>
        public string Toolchain
        {
            get
            {
                if (toolchain == null)
                {
                    this.LoadData();
                }

                return toolchain;
            }

            set
            {
                toolchain = value;
            }
        }

        /// <summary>
        /// Gets or sets the path to the rustup binary.
        /// </summary>
        public string RustupPath
        {
            get
            {
                if (rustupPath == null)
                {
                    this.LoadData();
                }

                return rustupPath;
            }

            set
            {
                rustupPath = value;
            }
        }

        /// <summary>
        /// Loads data for the model from the VS settings store.
        /// </summary>
        public void LoadData()
        {
            var settingsManager = new ShellSettingsManager(this.syncServiceProvider);
            var userSettingsStore = settingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
            EnsureSettingsStore(userSettingsStore);

            toolchain = userSettingsStore.GetString(SettingsCollection, ToolchainProperty);
            rustupPath = userSettingsStore.GetString(SettingsCollection, RustupPathProperty);
        }

        /// <summary>
        /// Saves model data to the VS settings store.
        /// </summary>
        public void SaveData()
        {
            var settingsManager = new ShellSettingsManager(this.syncServiceProvider);
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
