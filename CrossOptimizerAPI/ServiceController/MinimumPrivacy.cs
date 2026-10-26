using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace CrossOptimizerAPI.ServiceController;

public class MinimumPrivacy
{
    public List<string> AppliedTweaks { get; } = new List<string>();

    public virtual string Apply()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return "Ця функція працює, але виконується тільки на Windows";

        try
        {
            SetRegistryValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowTelemetry", 0, "Вимкнено збір телеметрії");
            SetRegistryValue(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Privacy", "TailoredExperiencesWithDiagnosticDataEnabled", 0, "Вимкнено персоналізацію");
            SetRegistryValue(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", 0, "Вимкнено рекламний ID");
            SetRegistryValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\CloudContent", "DisableWindowsConsumerFeatures", 1, "Вимкнено спонсорські пропозиції");

            return "Мінімальний рівень приватності встановлено";
        }
        catch (Exception ex)
        {
            return $"Помилка: {ex.Message}";
        }
    }

    protected void SetRegistryValue(RegistryKey root, string path, string name, object value, string description)
    {
        try
        {
            using (RegistryKey key = root.CreateSubKey(path))
            {
                if (key != null)
                {
                    key.SetValue(name, value);
                    if (!AppliedTweaks.Contains(description))
                    {
                        AppliedTweaks.Add(description);
                    }
                }
            }
        }
        catch { }
    }
}