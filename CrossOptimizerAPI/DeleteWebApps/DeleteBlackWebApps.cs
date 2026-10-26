using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace CrossOptimizerAPI.DeleteWebApps;

// повертаємо назви
public class AppModel
{
    public string DisplayName { get; set; }
    public string UninstallString { get; set; }
}

public class DeleteBlackWebApps
{
    public List<string> RemovedAppsNames { get; } = new List<string>();

    public AppModel AppModel
    {
        get => default;
        set
        {
        }
    }

    public List<AppModel> GetInstalledPrograms()
    {
        List<AppModel> apps = new List<AppModel>();

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return apps;

        string[] locations =
        {
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
            @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
        };

        foreach (string path in locations)
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(path))
            {
                if (key == null) continue;

                foreach (string subkeyName in key.GetSubKeyNames())
                {
                    using (RegistryKey subkey = key.OpenSubKey(subkeyName))
                    {
                        string name = subkey.GetValue("DisplayName") as string;
                        string uninstall = subkey.GetValue("UninstallString") as string;
                        string systemComponent = subkey.GetValue("SystemComponent")?.ToString();

                        if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(uninstall) && systemComponent != "1")
                        {
                            apps.Add(new AppModel
                            {
                                DisplayName = name,
                                UninstallString = uninstall
                            });
                        }
                    }
                }
            }
        }
        return apps;
    }

    public string ExecuteUninstall(AppModel app)
    {
        try
        {
            if (app == null || string.IsNullOrEmpty(app.UninstallString))
                return "Дані відсутні";

            string command = app.UninstallString;
            ProcessStartInfo psi = new ProcessStartInfo();

            if (command.ToLower().Contains("msiexec"))
            {
                psi.FileName = "msiexec.exe";
                psi.Arguments = command.Replace("msiexec.exe", "").Replace("/I", "/X") + " /quiet";
            }
            else
            {
                psi.FileName = "cmd.exe";
                psi.Arguments = $"/c {command}";
            }

            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;

            Process.Start(psi);

            RemovedAppsNames.Add(app.DisplayName);

            return "Процес видалення ініційовано";
        }
        catch (Exception ex)
        {
            return $"Помилка: {ex.Message}";
        }
    }
}