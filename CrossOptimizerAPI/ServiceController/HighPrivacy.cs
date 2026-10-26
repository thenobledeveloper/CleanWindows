using Microsoft.Win32;
using System;

namespace CrossOptimizerAPI.ServiceController;

public class HighPrivacy : MediumPrivacy
{
    public override string Apply()
    {
        base.Apply();

        try
        {
            SetRegistryValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "AllowCortana", 0, "Вимкнено Cortana");
            SetRegistryValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\System", "EnableActivityFeed", 0, "Вимкнено стрічку активності");
            SetRegistryValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors", "DisableLocation", 1, "Вимкнено служби геолокації");
            SetRegistryValue(Registry.CurrentUser, @"Software\Microsoft\InputPersonalization", "RestrictImplicitTextCollection", 1, "Вимкнено аналіз тексту");
            SetRegistryValue(Registry.CurrentUser, @"Software\Microsoft\InputPersonalization", "RestrictImplicitInkCollection", 1, "Вимкнено аналіз рукописного вводу");
            SetRegistryValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "DoNotShowFeedbackNotifications", 1, "Вимкнено запити на відгуки");
            SetRegistryValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Biometrics", "Enabled", 0, "Вимкнено біометричні датчики");
            SetRegistryValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\SettingSync", "DisableSettingSync", 2, "Вимкнено хмарну синхронізацію налаштувань");

            return "Максимальний рівень приватності встановлено";
        }
        catch (Exception ex)
        {
            return $"Помилка: {ex.Message}";
        }
    }
}