using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;

namespace RustLanguageExtension
{
    [Export(typeof(ILanguageClient))]
    [ContentType("rust")]
    public class RustLanguageExtension : ILanguageClient
    {
        public string Name => "Rust Language Extension";

        public IEnumerable<string> ConfigurationSections => null;

        public object InitializationOptions => null;

        public IEnumerable<string> FilesToWatch => null;

        public event AsyncEventHandler<EventArgs> StartAsync;
        public event AsyncEventHandler<EventArgs> StopAsync;

        public async Task<Connection> ActivateAsync(CancellationToken token)
        {
            var toolchain = RustLanguageExtensionOptionsPackage.Instance.OptionToolchain;
            var env = await MakeEnvironment(toolchain);
            var startInfo = new ProcessStartInfo()
            {
                FileName = "rustup",
                Arguments = $"run {toolchain} rls",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            foreach (var pair in env)
            {
                startInfo.Environment[pair.Key] = pair.Value;
            }

            var p = Process.Start(startInfo);
            return new Connection(p.StandardOutput.BaseStream, p.StandardInput.BaseStream);
        }

        public async System.Threading.Tasks.Task OnLoadedAsync()
        {
            // Since the extension is loaded via MEF the package with options is not guarenteed to be loaded yet.
            // Load the package here so that options can be accessed.
            IVsShell shell = await ServiceProvider.GetGlobalServiceAsync(typeof(SVsShell)) as IVsShell;
            Guid PackageToBeLoadedGuid = new Guid(RustLanguageExtensionOptionsPackage.PackageGuidString);
            shell.LoadPackage(ref PackageToBeLoadedGuid, out var package);

            var toolchain = RustLanguageExtensionOptionsPackage.Instance.OptionToolchain;
            if (!await Rustup.HasToolchain(toolchain))
            {
                await VsUtilities.ShowInfoBar($"configured toolchain {toolchain} is not installed, please install and relaunch VS");
                return;
            }

            if (!await Rustup.HasComponent("rls-preview", toolchain))
            {
                await VsUtilities.ShowInfoBar($"component 'rls-preview' is not installed, please install and relaunch VS");
                return;
            }

            if (!await Rustup.HasComponent("rust-analysis", toolchain))
            {
                await VsUtilities.ShowInfoBar($"component 'rust-analysis' is not installed, please install and relaunch VS");
                return;
            }

            if (!await Rustup.HasComponent("rust-src", toolchain))
            {
                await VsUtilities.ShowInfoBar($"component 'rust-src' is not installed, please install and relaunch VS");
                return;
            }

            if (StartAsync != null)
            {
                await StartAsync.InvokeAsync(this, EventArgs.Empty);
            }
        }

        private async Task<IDictionary<string, string>> MakeEnvironment(string toolchain)
        {
            var newEnv = new Dictionary<string, string>();
            if (Environment.GetEnvironmentVariable("RUST_SRC_PATH") == null)
            {
                newEnv["RUST_SRC_PATH"] = await GetSysRoot(toolchain);
            }

            return newEnv;
        }

        private async Task<string> GetSysRoot(string toolchain)
        {
            var (sysRoot, _) = await Rustup.Run("rustc --print sysroot", toolchain);
            return sysRoot.Replace("\n", "").Replace("\r", "");
        }
    }
}
