// <copyright file="OptionsPage.cs" company="Daniel Griffen">
// Copyright (c) Daniel Griffen. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RustLanguageExtension
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft;
    using Microsoft.VisualStudio.ComponentModelHost;
    using Microsoft.VisualStudio.Shell;

    /// <summary>
    /// Dialog page for Rust extension options.
    /// </summary>
    internal class OptionsPage : DialogPage
    {
        private OptionsModel optionsModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionsPage"/> class.
        /// </summary>
        public OptionsPage()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var componentModel = ServiceProvider.GlobalProvider.GetService(typeof(SComponentModel)) as IComponentModel;

            Assumes.Present(componentModel);
            this.optionsModel = componentModel.GetService<OptionsModel>();
        }

        /// <summary>
        /// Gets or sets the name of the default toolchain to use for the project.
        /// </summary>
        [DisplayName("Toolchain to Use")]
        [Description("Toolchain to use when invoking the Rust Language Server")]
        public string Toolchain
        {
            get { return this.optionsModel.Toolchain; }
            set { this.optionsModel.Toolchain = value; }
        }

        /// <summary>
        /// Gets or sets the path to the rustup binary.
        /// </summary>
        [DisplayName("Path to rustup")]
        [Description("Path to the rustup binary, including rustup.exe")]
        public string RustupPath
        {
            get { return this.optionsModel.RustupPath; }
            set { this.optionsModel.RustupPath = value; }
        }

        /// <inheritdoc/>
        public override void LoadSettingsFromStorage()
        {
            this.optionsModel.LoadData();
        }

        /// <inheritdoc/>
        public override void SaveSettingsToStorage()
        {
            this.optionsModel.SaveData();
        }
    }
}
