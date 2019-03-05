// <copyright file="RustLanguageExtension.cs" company="Daniel Griffen">
// Copyright (c) Daniel Griffen. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RustLanguageExtension
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.LanguageServer.Client;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.Threading;
    using Microsoft.VisualStudio.Utilities;
    using Microsoft.VisualStudio.Workspace.VSIntegration.Contracts;
    using IAsyncServiceProvider = Microsoft.VisualStudio.Shell.IAsyncServiceProvider;
    using Task = System.Threading.Tasks.Task;

    [Export(typeof(ILanguageClient))]
    [ContentType("rust")]
    public class RustLanguageExtension : ILanguageClient
    {
        private readonly IVsFolderWorkspaceService workspaceService;
        private readonly IAsyncServiceProvider serviceProvider;

        [ImportingConstructor]
        public RustLanguageExtension([Import] IVsFolderWorkspaceService workspaceService, [Import(typeof(SAsyncServiceProvider))] IAsyncServiceProvider serviceProvider)
        {
            this.workspaceService = workspaceService;
            this.serviceProvider = serviceProvider;
        }

        public event AsyncEventHandler<EventArgs> StartAsync;

        public event AsyncEventHandler<EventArgs> StopAsync
        {
            add { }
            remove { }
        }

        public string Name => "Rust Language Extension";

        public IEnumerable<string> ConfigurationSections => null;

        public object InitializationOptions => null;

        public IEnumerable<string> FilesToWatch => null;

        public async Task<Connection> ActivateAsync(CancellationToken token)
        {
            var path = OptionsModel.RustupPath == string.Empty ? "rustup" : OptionsModel.RustupPath;
            var rustup = new Rustup(path);
            var toolchain = OptionsModel.Toolchain;
            var env = await this.MakeEnvironmentAsync(rustup, toolchain);

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
            if (!await rustup.IsInstalledAsync())
            {
                var infoBar = new VsUtilities.InfoBar("could not start the rls: rustup is not installed or not on the path");
                await VsUtilities.ShowInfoBarAsync(infoBar);
                return;
            }

            var toolchain = OptionsModel.Toolchain;
            if (!await rustup.HasToolchainAsync(toolchain))
            {
                var infoBar = new VsUtilities.InfoBar($"configured toolchain {toolchain} is not installed", new VsUtilities.InfoBarButton("Install"));
                if (await Utilities.WaitForSingleButtonInfoBarAsync(infoBar))
                {
                    var task = rustup.InstallToolchainAsync(toolchain);
                    await VsUtilities.CreateTaskAsync($"Installing {toolchain}", this.serviceProvider, task);
                    if (await task != 0)
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
            if (!await rustup.HasComponentAsync("rls", toolchain)
                || !await rustup.HasComponentAsync("rust-analysis", toolchain)
                || !await rustup.HasComponentAsync("rust-src", toolchain))
            {
                if (!await this.InstallComponentsAsync(rustup, toolchain, "rls-preview", "rust-analysis", "rust-src"))
                {
                    var infoBar = new VsUtilities.InfoBar("could not install one of the required rls components");
                    await VsUtilities.ShowInfoBarAsync(infoBar);
                    return;
                }
            }

            if (this.StartAsync != null)
            {
                await this.StartAsync.InvokeAsync(this, EventArgs.Empty);
            }
        }

        public Task OnServerInitializedAsync()
        {
            return Task.CompletedTask;
        }

        public Task OnServerInitializeFailedAsync(Exception e)
        {
            return Task.CompletedTask;
        }

        private async Task<bool> InstallComponentsAsync(Rustup rustup, string toolchain, params string[] components)
        {
            VsUtilities.InfoBar infoBar;
            if (components.Length == 1)
            {
                infoBar = new VsUtilities.InfoBar($"component '{components[0]}' is not installed", new VsUtilities.InfoBarButton("Install"));
            }
            else
            {
                infoBar = new VsUtilities.InfoBar("required components are not installed", new VsUtilities.InfoBarButton("Install"));
            }

            if (await Utilities.WaitForSingleButtonInfoBarAsync(infoBar))
            {
                var task = rustup.InstallComponentsAsync(toolchain, components);
                await VsUtilities.CreateTaskAsync($"Installing components", this.serviceProvider, task);
                return await task == 0;
            }
            else
            {
                return false;
            }
        }

        private async Task<IDictionary<string, string>> MakeEnvironmentAsync(Rustup rustup, string toolchain)
        {
            var newEnv = new Dictionary<string, string>();
            if (Environment.GetEnvironmentVariable("RUST_SRC_PATH") == null)
            {
                var sysRoot = await this.GetSysRootAsync(rustup, toolchain);
                var srcPath = Path.Combine(sysRoot, "lib\\rustlib\\src\\rust\\src");
                newEnv["RUST_SRC_PATH"] = srcPath;
            }

            return newEnv;
        }

        private async Task<string> GetSysRootAsync(Rustup rustup, string toolchain)
        {
            var result = await rustup.RunAsync("rustc --print sysroot", toolchain);
            return result.Output.Replace("\n", string.Empty).Replace("\r", string.Empty);
        }
    }
}
