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

        public async Task<bool> IsInstalled()
        {
            try
            {
                var (_, exitCode) = await this.RunCommand("toolchain list");
                return exitCode == 0;
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

        public Task Update()
        {
            return RunCommand("update");
        }

        public Task<(string output, int exitCode)> Run(string command, string toolchain)
        {
            return RunCommand($"run {toolchain} {command}");
        }

        public async Task<bool> HasToolchain(string toolchain)
        {
            var (output, _) = await this.RunCommand("toolchain list");
            return output.Contains(toolchain);
        }

        public async Task<bool> HasComponent(string component, string toolchain)
        {
            var (output, _) = await this.RunCommand($"component list --toolchain {toolchain}");
            return Regex.IsMatch(output, $"^{component}.* \\((default|installed)\\)$", RegexOptions.Multiline);
        }

        public async Task<int> InstallToolchain(string toolchain)
        {
            var (_, exitCode) = await this.RunCommand($"toolchain install {toolchain}");
            return exitCode;
        }

        public async Task<int> InstallComponents(string toolchain, params string[] components)
        {
            var componentsString = string.Join(" ", components);
            var (_, exitCode) = await this.RunCommand($"component add {componentsString} --toolchain {toolchain}");
            return exitCode;
        }

        private async Task<(string output, int exitCode)> RunCommand(string command)
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
            return (output, p.ExitCode);
        }
    }
}
