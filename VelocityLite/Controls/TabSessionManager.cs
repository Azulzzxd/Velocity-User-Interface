using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using static VelocityLite.MainWindow;

namespace VelocityLite.Controls
{
    internal static class TabSessionManager
    {
        private static readonly string BaseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Velocity Ui");

        private static readonly string TabsPath = Path.Combine(BaseDir, "Tabs.json");

        public static async Task SaveAsync(IEnumerable<SavedTab> tabs)
        {
            try
            {
                if (!Directory.Exists(BaseDir))
                {
                    Directory.CreateDirectory(BaseDir);
                }

                var json = JsonSerializer.Serialize(tabs, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(TabsPath, json);
            }
            catch
            {
            }
        }

        public static async Task<List<SavedTab>> LoadAsync()
        {
            try
            {
                if (!File.Exists(TabsPath))
                {
                    return new List<SavedTab>();
                }

                var json = await File.ReadAllTextAsync(TabsPath);
                return JsonSerializer.Deserialize<List<SavedTab>>(json) ?? new();
            }
            catch
            {
                return new List<SavedTab>();
            }
        }

        public static void Delete()
        {
            try
            {
                if (File.Exists(TabsPath))
                {
                    File.Delete(TabsPath);
                }
            }
            catch
            {
            }
        }
    }
}
