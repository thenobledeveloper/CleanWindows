using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace CrossOptimizerAPI.Security;

public class AutoRestorePoint
{
    public async Task<string> CreateOnStartupAsync(bool force = false)
    {
        return await Task.Run(() =>
        {
            try
            {
                string today = DateTime.Now.ToString("yyyyMMdd");
                using RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\CrossOptimizer");

                if (!force && key.GetValue("LastRP")?.ToString() == today)
                {
                    return "Точка відновлення на сьогодні вже є";
                }

                var startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = "-NoProfile -ExecutionPolicy Bypass -Command \"Checkpoint-Computer -Description 'CrossOptimizer Restore Point' -RestorePointType 'MODIFY_SETTINGS' -ErrorAction Stop\"",
                    Verb = "runas",
                    UseShellExecute = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                using (var process = Process.Start(startInfo))
                {
                    if (process != null)
                    {
                        process.WaitForExit();
                        if (process.ExitCode == 0)
                        {
                            key.SetValue("LastRP", today);
                            return "Точку створено успішно";
                        }
                        else
                        {
                            return $"Помилка PowerShell (код {process.ExitCode}). Перевірте ліміти системи.";
                        }
                    }
                }
                return "Не вдалося запустити процес PowerShell";
            }
            catch (Exception ex)
            {
                return $"Критична помилка: {ex.Message}";
            }
        });
    }
}