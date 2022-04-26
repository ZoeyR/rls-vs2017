// <copyright file="Rustup.cs" company="Daniel Griffen">
// Copyright (c) Daniel Griffen. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RustLanguageExtension
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    /// <summary>
    /// Class for interacting with rustup.exe.
    /// </summary>
    internal class Rustup
    {
        private string path;

        /// <summary>
        /// Initializes a new instance of the <see cref="Rustup"/> class.
        /// </summary>
        /// <param name="path">Path to the rustup binary.</param>
        public Rustup(string path)
        {
            if (path == string.Empty)
            {
                this.path = "rustup";
            }
            else
            {
                this.path = path;
            }
        }

        /// <summary>
        /// Checks if rustup is installed on the machine.
        /// </summary>
        /// <returns>A task that completes with true if rustup is installed, false otherwise.</returns>
        public async Task<bool> IsInstalledAsync()
        {
            try
            {
                var result = await this.RunCommandAsync("toolchain list");
                return result.ExitCode == 0;
            }
            catch (Win32Exception)
            {
                return false;
            }
            catch (FileNotFoundException)
            {
                return false;
            }
        }

        /// <summary>
        /// Updates installed rust toolchains.
        /// </summary>
        /// <returns>A task that completes when rustup has finished updating the rust toolchains.</returns>
        public Task UpdateAsync()
        {
            return this.RunCommandAsync("update");
        }

        /// <summary>
        /// Runs a rustup command on the specified toolchain.
        /// </summary>
        /// <param name="command">The command to run.</param>
        /// <param name="toolchain">The toolchain to run the command on.</param>
        /// <returns>A task that completes with the result of the command.</returns>
        public Task<CommandResult> RunAsync(string command, string toolchain)
        {
            return this.RunCommandAsync($"run {toolchain} {command}");
        }

        /// <summary>
        /// Checks if the specified toolchain is installed.
        /// </summary>
        /// <param name="toolchain">The toolchain to check.</param>
        /// <returns>A task that completes with true if the toolchain is installed, false otherwise.</returns>
        public async Task<bool> HasToolchainAsync(string toolchain)
        {
            var result = await this.RunCommandAsync("toolchain list");
            return result.Output.Contains(toolchain);
        }

        /// <summary>
        /// Checks if the specified component is installed.
        /// </summary>
        /// <param name="component">The component to check for.</param>
        /// <param name="toolchain">The toolchain to check for the component on.</param>
        /// <returns>A task that completes with true if the component is installed, false otherwise.</returns>
        public async Task<bool> HasComponentAsync(string component, string toolchain)
        {
            var result = await this.RunCommandAsync($"component list --toolchain {toolchain}");
            return Regex.IsMatch(result.Output, $"^{component}.* \\((default|installed)\\)$", RegexOptions.Multiline);
        }

        /// <summary>
        /// Installs the specified toolchain.
        /// </summary>
        /// <param name="toolchain">The name of the toolchain to install.</param>
        /// <returns>A task that completes with the exit code of the command.</returns>
        public async Task<int> InstallToolchainAsync(string toolchain)
        {
            var result = await this.RunCommandAsync($"toolchain install {toolchain}");
            return result.ExitCode;
        }

        /// <summary>
        /// Installs the specified component.
        /// </summary>
        /// <param name="toolchain">The toolchain to install the component on.</param>
        /// <param name="components">The component to install.</param>
        /// <returns>A task that completes with the exit code of the command.</returns>
        public async Task<int> InstallComponentsAsync(string toolchain, params string[] components)
        {
            var componentsString = string.Join(" ", components);
            var result = await this.RunCommandAsync($"component add {componentsString} --toolchain {toolchain}");
            return result.ExitCode;
        }

        private async Task<CommandResult> RunCommandAsync(string command)
        {
            var startInfo = new ProcessStartInfo()
            {
                FileName = this.path,
                Arguments = command,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
            };

            var p = Process.Start(startInfo);
            var output = await p.StandardOutput.ReadToEndAsync();
            return new CommandResult
            {
                Output = output,
                ExitCode = p.ExitCode,
            };
        }

        /// <summary>
        /// Result of executing a rustup command.
        /// </summary>
        public class CommandResult
        {
            /// <summary>
            /// Gets or sets the exit code of rustup for the command.
            /// </summary>
            public int ExitCode { get; set; }

            /// <summary>
            /// Gets or sets the stdout from the command.
            /// </summary>
            public string Output { get; set; }
        }
    }
}
