using Microsoft.Win32;
using System;

namespace CrossOptimizerAPI.ServiceController;

public class MediumPrivacy : MinimumPrivacy
{
    public override string Apply()
    {
        base.Apply();

        try
        {
            SetRegistryValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\PCHealth\ErrorReporting", "DoReport", 0, "Вимкнено звіти про помилки");
            SetRegistryValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update", "AUOptions", 2, "Заборонено автоматичне встановлення оновлень");
            SetRegistryValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppCompat", "DisableInventory", 1, "Вимкнено збір даних про встановлене ПЗ");
            SetRegistryValue(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_TrackProgs", 0, "Вимкнено відстеження запусків програм");
            SetRegistryValue(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Search", "SearchHistoryEnabled", 0, "Вимкнено історію пошуку");
            SetRegistryValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\System", "PublishUserActivities", 0, "Вимкнено публікацію активностей");

            return "Середній рівень приватності встановлено";
        }
        catch (Exception ex)
        {
            return $"Помилка: {ex.Message}";
        }
    }
}