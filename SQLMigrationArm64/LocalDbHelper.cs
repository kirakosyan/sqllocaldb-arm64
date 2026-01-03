using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace SQLMigrationArm64
{
    internal class LocalDbHelper
    {
        /// <summary>
        /// Use LocalDB on ARM64 for local development
        /// Adjust for ARM64 local development if necessary
        /// </summary>
        /// <param name="instanceName"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static string GetLocalDbPipe(string instanceName)
        {
            // Ensure instance is started (optional but helps)
            Process.Start(new ProcessStartInfo("sqllocaldb", $"start {instanceName}")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            })?.WaitForExit();

            var psi = new ProcessStartInfo("sqllocaldb", $"info {instanceName}")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using var p = Process.Start(psi)!;
            var output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            // Works across localized output too (looks for the np:\\.\pipe\LOCALDB#... pattern)
            var m = Regex.Match(output, @"(np:\\\\\.\\pipe\\LOCALDB#[^\s]+\\tsql\\query)", RegexOptions.IgnoreCase);
            if (!m.Success) throw new InvalidOperationException($"Could not find LocalDB pipe in output:\n{output}");

            return m.Groups[1].Value;
        }
    }
}
