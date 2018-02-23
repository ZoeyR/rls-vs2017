using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Shell;
using System.IO;
using Microsoft.VisualStudio.Workspace.VSIntegration.Contracts;
using Task = System.Threading.Tasks.Task;

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

        private readonly IVsFolderWorkspaceService workspaceService;

        [ImportingConstructor]
        public RustLanguageExtension([Import] IVsFolderWorkspaceService workspaceService)
        {
            this.workspaceService = workspaceService;
        }

        public async Task<Connection> ActivateAsync(CancellationToken token)
        {
            var path = OptionsModel.RustupPath == string.Empty ? "rustup" : OptionsModel.RustupPath;
            var rustup = new Rustup(path);
            var toolchain = OptionsModel.Toolchain;
            var env = await MakeEnvironment(rustup, toolchain);

            var directoryPath = this.workspaceService.CurrentWorkspace?.Location ?? string.Empty;
            var startInfo = new ProcessStartInfo()
            {
                FileName = path,
                Arguments = $"run {toolchain} rls",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = directoryPath,
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
            var rustup = new Rustup(OptionsModel.RustupPath);
            if(!await rustup.IsInstalled())
            {
                var infoBar = new VsUtilities.InfoBar("could not start the rls: rustup is not installed or not on the path");
                await VsUtilities.ShowInfoBar(infoBar);
                return;
            }

            var toolchain = OptionsModel.Toolchain;
            if (!await rustup.HasToolchain(toolchain))
            {
                var infoBar = new VsUtilities.InfoBar($"configured toolchain {toolchain} is not installed", new VsUtilities.InfoBarButton("Install"));
                if (await Utilities.WaitForSingleButtonInfoBarAsync(infoBar))
                {
                    var task = rustup.InstallToolchain(toolchain).ContinueWith(t => t.Result == 0);
                    await VsUtilities.CreateTask($"Installing {toolchain}", task);
                    if (!await task)
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
            }

            // Check for necessary rls components
            if (!await rustup.HasComponent("rls-preview", toolchain)
                || !await rustup.HasComponent("rust-analysis", toolchain)
                || !await rustup.HasComponent("rust-src", toolchain))
            {
                if (!await InstallComponents(rustup, toolchain, "rls-preview", "rust-analysis", "rust-src"))
                {
                    var infoBar = new VsUtilities.InfoBar("could not install one of the required rls components");
                    await VsUtilities.ShowInfoBar(infoBar);
                    return;
                }
            }

            if (StartAsync != null)
            {
                await StartAsync.InvokeAsync(this, EventArgs.Empty);
            }
        }

        private async Task<bool> InstallComponents(Rustup rustup, string toolchain, params string[] components)
        {
            VsUtilities.InfoBar infoBar;
            if (components.Length == 1)
            {
                infoBar = new VsUtilities.InfoBar($"component '{components[0]}' is not installed", new VsUtilities.InfoBarButton("Install"));
            } else
            {
                infoBar = new VsUtilities.InfoBar("required components are not installed", new VsUtilities.InfoBarButton("Install"));
            }

            if (await Utilities.WaitForSingleButtonInfoBarAsync(infoBar))
            {
                var task = rustup.InstallComponents(toolchain, components).ContinueWith(t => t.Result == 0);
                await VsUtilities.CreateTask($"Installing components", task);
                return await task;
            }
            else
            {
                return false;
            }
        }

        private async Task<IDictionary<string, string>> MakeEnvironment(Rustup rustup, string toolchain)
        {
            var newEnv = new Dictionary<string, string>();
            if (Environment.GetEnvironmentVariable("RUST_SRC_PATH") == null)
            {
                var sysRoot = await GetSysRoot(rustup, toolchain);
                var srcPath = Path.Combine(sysRoot, "lib\\rustlib\\src\\rust\\src");
                newEnv["RUST_SRC_PATH"] = srcPath;
            }

            return newEnv;
        }

        private async Task<string> GetSysRoot(Rustup rustup, string toolchain)
        {
            var (sysRoot, _) = await rustup.Run("rustc --print sysroot", toolchain);
            return sysRoot.Replace("\n", "").Replace("\r", "");
        }

        public Task OnServerInitializedAsync()
        {
            return Task.CompletedTask;
        }

        public Task OnServerInitializeFailedAsync(Exception e)
        {
            return Task.CompletedTask;
        }
    }
}
