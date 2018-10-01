using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RustLanguageExtension
{
    internal class Rustup
    {
        private string path;
        public Rustup(string path)
        {
            if (path == String.Empty)
            {
                this.path = "rustup";
            } else
            {
                this.path = path;
            }
        }

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

        public Task UpdateAsync()
        {
            return RunCommandAsync("update");
        }

        public Task<CommandResult> RunAsync(string command, string toolchain)
        {
            return RunCommandAsync($"run {toolchain} {command}");
        }

        public async Task<bool> HasToolchainAsync(string toolchain)
        {
            var result = await this.RunCommandAsync("toolchain list");
            return result.Output.Contains(toolchain);
        }

        public async Task<bool> HasComponentAsync(string component, string toolchain)
        {
            var result = await this.RunCommandAsync($"component list --toolchain {toolchain}");
            return Regex.IsMatch(result.Output, $"^{component}.* \\((default|installed)\\)$", RegexOptions.Multiline);
        }

        public async Task<int> InstallToolchainAsync(string toolchain)
        {
            var result = await this.RunCommandAsync($"toolchain install {toolchain}");
            return result.ExitCode;
        }

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
                ExitCode = p.ExitCode
            };
        }

        public class CommandResult
        {
            public int ExitCode { get; set; }
            public string Output { get; set; }
        }
    }
}
