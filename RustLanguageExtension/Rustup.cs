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

        public static Task<Tuple<string, int>> Run(string command, string toolchain)
        {
            return RunCommand($"run {toolchain} command");
        }

        public static async Task<bool> HasToolchain(string toolchain)
        {
            var t = await RunCommand("toolchain list");
            return t.Item1.Contains(toolchain);
        }

        public static async Task<bool> HasComponent(string component, string toolchain)
        {
            var t = await RunCommand($"component list --toolchain {toolchain}");
            return Regex.IsMatch(t.Item1, $"^{component}.* \\((default|installed)\\)$", RegexOptions.Multiline);
        }

        public static async Task<int> InstallToolchain(string toolchain)
        {
            var t = await RunCommand($"toolchain install {toolchain}");
            return t.Item2;
        }

        public static async Task<int> InstallComponent(string component, string toolchain)
        {
            var t = await RunCommand($"component add {component} --toolchain {toolchain}");
            return t.Item2;
        }

        private static async Task<Tuple<string, int>> RunCommand(string command)
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
            return new Tuple<string, int>(output, p.ExitCode);
        }
    }
}
