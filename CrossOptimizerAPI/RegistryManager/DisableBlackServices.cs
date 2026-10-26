using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace CrossOptimizerAPI.RegistryManager
{
    public class ServiceModel
    {
        public string ServiceName { get; set; }
        public string DisplayName { get; set; }
        public bool IsDisabled { get; set; }
    }

    public class DisableBlackServices
    {
        public List<string> DisabledServicesNames { get; } = new List<string>();

        public ServiceModel ServiceModel
        {
            get => default;
            set { }
        }

        private readonly List<string> _blackList = new List<string>
        {
            "DiagTrack",
            "dmwappushservice",
            "WerSvc",
            "XblAuthManager",
            "XblGameSave",
            "XboxNetApiSvc",
            "PhoneSvc"
        };

        public List<ServiceModel> GetTargetServices()
        {
            List<ServiceModel> services = new List<ServiceModel>();

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                services.Add(new ServiceModel { ServiceName = "None", DisplayName = "Доступно лише у Windows", IsDisabled = true });
                return services;
            }

            try
            {
                using (RegistryKey hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                using (RegistryKey servicesKey = hklm.OpenSubKey(@"SYSTEM\CurrentControlSet\Services"))
                {
                    if (servicesKey == null) return services;

                    foreach (string name in _blackList)
                    {
                        using (RegistryKey serviceKey = servicesKey.OpenSubKey(name))
                        {
                            if (serviceKey == null) continue;

                            string displayName = serviceKey.GetValue("DisplayName")?.ToString() ?? name;
                            object startVal = serviceKey.GetValue("Start");
                            int startType = startVal != null ? Convert.ToInt32(startVal) : 0;

                            services.Add(new ServiceModel
                            {
                                ServiceName = name,
                                DisplayName = displayName,
                                IsDisabled = (startType == 4)
                            });
                        }
                    }
                }
            }
            catch { }
            return services;
        }

        public string DisableService(string serviceName)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return "Не підтримується";

            try
            {
                ProcessStartInfo configInfo = new ProcessStartInfo
                {
                    FileName = "sc.exe",
                    Arguments = $"config \"{serviceName}\" start= demand",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    Verb = "runas"
                };

                ProcessStartInfo stopInfo = new ProcessStartInfo
                {
                    FileName = "sc.exe",
                    Arguments = $"stop \"{serviceName}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    Verb = "runas"
                };

                using (Process configProc = Process.Start(configInfo))
                {
                    configProc?.WaitForExit();
                }

                using (Process stopProc = Process.Start(stopInfo))
                {
                    stopProc?.WaitForExit();
                }

                if (!DisabledServicesNames.Contains(serviceName))
                    DisabledServicesNames.Add(serviceName);

                return "Успішно";
            }
            catch (Exception ex)
            {
                return $"Помилка: {ex.Message}";
            }
        }
    }
}