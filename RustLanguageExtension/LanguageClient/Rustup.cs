using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RustLanguageExtension
{
    public static class Rustup
    {
        public static Task Update()
        {
            return RunCommand("update");
        }

        public static Task<(string output, int exitCode)> Run(string command, string toolchain)
        {
            return RunCommand($"run {toolchain} command");
        }

        public static async Task<bool> HasToolchain(string toolchain)
        {
            var (output, _) = await RunCommand("toolchain list");
            return output.Contains(toolchain);
        }

        public static async Task<bool> HasComponent(string component, string toolchain)
        {
            var (output, _) = await RunCommand($"component list --toolchain {toolchain}");
            return Regex.IsMatch(output, $"^{component}.* \\((default|installed)\\)$", RegexOptions.Multiline);
        }

        public static async Task<int> InstallToolchain(string toolchain)
        {
            var (_, exitCode) = await RunCommand($"toolchain install {toolchain}");
            return exitCode;
        }

        public static async Task<int> InstallComponent(string component, string toolchain)
        {
            var (_, exitCode) = await RunCommand($"component add {component} --toolchain {toolchain}");
            return exitCode;
        }

        private static async Task<(string output, int exitCode)> RunCommand(string command)
        {
            var startInfo = new ProcessStartInfo()
            {
                FileName = "rustup",
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
