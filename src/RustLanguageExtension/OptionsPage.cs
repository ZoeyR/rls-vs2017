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
    using Microsoft.VisualStudio.Shell;

    internal class OptionsPage : DialogPage
    {
        [DisplayName("Toolchain to Use")]
        [Description("Toolchain to use when invoking the Rust Language Server")]
        public string Toolchain
        {
            get { return OptionsModel.Toolchain; }
            set { OptionsModel.Toolchain = value; }
        }

        [DisplayName("Path to rustup")]
        [Description("Path to the rustup binary, including rustup.exe")]
        public string RustupPath
        {
            get { return OptionsModel.RustupPath; }
            set { OptionsModel.RustupPath = value; }
        }

        public override void LoadSettingsFromStorage()
        {
            OptionsModel.LoadData();
        }

        public override void SaveSettingsToStorage()
        {
            OptionsModel.SaveData();
        }
    }
}
