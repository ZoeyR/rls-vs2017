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
        private string toolchain = "nightly";

        [DisplayName("Toolchain to Use")]
        [Description("Toolchain to use when invoking the Rust Language Server")]
        public string Toolchain
        {
            get { return toolchain; }
            set { toolchain = value; }
        }
    }
}
