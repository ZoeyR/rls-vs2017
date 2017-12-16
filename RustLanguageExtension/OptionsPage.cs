using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RustLanguageExtension
{
    class OptionsPage: DialogPage
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
