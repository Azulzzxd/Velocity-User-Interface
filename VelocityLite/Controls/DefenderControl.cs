using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using VelocityLite;

namespace VelocityLite.Controls
{
    internal static class DefenderControl
    {
        #region Fields

        private static readonly string TargetFolder = AppDomain.CurrentDomain.BaseDirectory;

        #endregion

        #region Public API

        public static async Task CheckAndWarnAsync()
        {
            var status = await GetDefenderStatusExternalAsync();
            if (status == null)
                return;

            if (status.RealTimeProtectionEnabled != true)
                return;

            if (IsFolderExcluded(status, TargetFolder))
                return;

            ShowWarning();
        }

        #endregion

        #region Defender Query

        private static async Task<DefenderStatus?> GetDefenderStatusExternalAsync()
        {
            return await Task.Run(() =>
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments =
                        "-NoProfile -NonInteractive -WindowStyle Hidden -Command " +
                        "\"Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass -Force; " +
                        "Import-Module ConfigDefender; " +
                        "$status = Get-MpComputerStatus; " +
                        "$prefs = Get-MpPreference; " +
                        "[PSCustomObject]@{ " +
                        "AMServiceEnabled = $status.AMServiceEnabled; " +
                        "AntispywareEnabled = $status.AntispywareEnabled; " +
                        "RealTimeProtectionEnabled = $status.RealTimeProtectionEnabled; " +
                        "NISEnabled = $status.NISEnabled; " +
                        "ExclusionPath = $prefs.ExclusionPath " +
                        "} | ConvertTo-Json -Depth 3\"",

                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(startInfo);
                if (process == null)
                    return null;

                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                if (string.IsNullOrWhiteSpace(output))
                    return null;

                return JsonSerializer.Deserialize<DefenderStatus>(output);
            });
        }

        #endregion

        #region Warning UI

        private static void ShowWarning()
        {
            var mainWindow = Application.Current.Windows
                .OfType<MainWindow>()
                .FirstOrDefault();

            var parentGrid = mainWindow?.MessageBoxSpawnParent;

            const string message =
                "Due to Velocity behavior that may resemble malware patterns, your antivirus may flag or block it.\n\n" +
                "It is recommended to add an exclusion for the Velocity folder or temporarily disable antivirus protection while using it.";

            if (parentGrid == null)
            {
                MessageBox.Show(
                    message,
                    "Antivirus Warning",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            var dialog = new MessageBoxStandard();

            parentGrid.Children.Add(dialog);
            parentGrid.UpdateLayout();

            Panel.SetZIndex(dialog, 999);

            _ = dialog.ShowDialogAsync(
                "Antivirus Warning",
                message,
                "Ok",
                null,
                "Resource/Msg/BoxType_AntiVirus.png"
            ).ContinueWith(_ =>
            {
                if (parentGrid.Children.Contains(dialog))
                    parentGrid.Children.Remove(dialog);

            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        #endregion

        #region Helpers

        private static bool IsFolderExcluded(DefenderStatus status, string folderPath)
        {
            if (status.ExclusionPath == null || status.ExclusionPath.Length == 0)
                return false;

            string normalizedTarget = Path.GetFullPath(folderPath)
                .TrimEnd(Path.DirectorySeparatorChar)
                .ToUpperInvariant();

            return status.ExclusionPath.Any(excluded =>
            {
                if (string.IsNullOrWhiteSpace(excluded))
                    return false;

                string normalizedExcluded = Path.GetFullPath(excluded)
                    .TrimEnd(Path.DirectorySeparatorChar)
                    .ToUpperInvariant();

                return normalizedTarget.StartsWith(normalizedExcluded);
            });
        }

        #endregion

        #region Models

        private class DefenderStatus
        {
            public bool? AMServiceEnabled { get; set; }
            public bool? AntispywareEnabled { get; set; }
            public bool? RealTimeProtectionEnabled { get; set; }
            public bool? NISEnabled { get; set; }
            public string[]? ExclusionPath { get; set; }
        }

        #endregion
    }
}