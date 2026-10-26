using CrossOptimizerAPI.CleanTrash;
using CrossOptimizerAPI.DeleteWebApps;
using CrossOptimizerAPI.RegistryManager;
using CrossOptimizerAPI.ServiceController;
using CrossOptimizerAPI.Security;
using CrossOptimizerAPI.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using System.Text.Encodings.Web;

namespace CrossOptimizerAPI
{
    public class HistoryEntry
    {
        public DateTime Time { get; set; }
        public string Message { get; set; }
    }

    public class CrossControlAPI
    {
        private readonly CleanLocalTemp _localCleaner = new CleanLocalTemp();
        private readonly TheCleanerFolderGlobalTEMP _globalCleaner = new TheCleanerFolderGlobalTEMP();
        private readonly DeleteBlackWebApps _appManager = new DeleteBlackWebApps();
        private readonly DisableBlackServices _serviceManager = new DisableBlackServices();
        private readonly AutoRestorePoint _autoRestore = new AutoRestorePoint();
        private readonly QuickLaunch _quickLaunch = new QuickLaunch();
        private readonly string _sessionFile = "session.state";

        private dynamic _activePrivacy;

        public OptimizationStats SessionStats { get; } = new OptimizationStats();
        public string StartupStatus { get; private set; }

        public QuickLaunch QuickLaunch { get; set; }
        public AutoRestorePoint AutoRestorePoint { get; set; }
        public HighPrivacy HighPrivacy { get; set; }
        public MediumPrivacy MediumPrivacy { get; set; }
        public MinimumPrivacy MinimumPrivacy { get; set; }
        public OptimizationStats OptimizationStats { get; set; }
        public DeleteBlackWebApps DeleteBlackWebApps { get; set; }
        public DisableBlackServices DisableBlackServices { get; set; }
        public CleanLocalTemp CleanLocalTemp { get; set; }
        public TheCleanerFolderGlobalTEMP TheCleanerFolderGlobalTEMP { get; set; }

        public CrossControlAPI()
        {
            if (File.Exists(_sessionFile)) File.Delete(_sessionFile);
        }

        private bool IsAlreadyApplied(string level)
        {
            if (!File.Exists(_sessionFile)) return false;
            string content = File.ReadAllText(_sessionFile);
            return content.Contains(level);
        }

        private void MarkAsApplied(string level)
        {
            File.AppendAllText(_sessionFile, level + ";");
        }

        private void LogHistory(string message)
        {
            string filePath = "journal_my_history.txt";
            List<HistoryEntry> entries = new List<HistoryEntry>();

            if (File.Exists(filePath))
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    entries = JsonSerializer.Deserialize<List<HistoryEntry>>(json) ?? new List<HistoryEntry>();
                }
                catch
                {
                    entries = new List<HistoryEntry>();
                }
            }

            entries.Add(new HistoryEntry
            {
                Time = DateTime.Now,
                Message = message
            });

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            string updatedJson = JsonSerializer.Serialize(entries, options);
            File.WriteAllText(filePath, updatedJson);
        }

        public async Task<string> ExecuteRestorePointAsync()
        {
            StartupStatus = await _autoRestore.CreateOnStartupAsync();
            return StartupStatus;
        }

        public string ExecuteGlobalCleanup()
        {
            var (files, folders) = _globalCleaner.StartCleaning();
            SessionStats.FilesDeleted += files;
            SessionStats.FoldersDeleted += folders;
            return $"Глобальна очистка: {files} файлів, {folders} папок.";
        }

        public string ExecuteLocalCleanup()
        {
            var (files, folders) = _localCleaner.StartCleaning();
            SessionStats.FilesDeleted += files;
            SessionStats.FoldersDeleted += folders;
            return $"Локальна очистка: {files} файлів, {folders} папок.";
        }

        public List<AppModel> GetAppsList() => _appManager.GetInstalledPrograms();

        public string RemoveSelectedApp(AppModel app)
        {
            string result = _appManager.ExecuteUninstall(app);
            if (result.Contains("ініційовано") || result.Contains("успішно"))
            {
                SessionStats.AppsRemoved++;
                LogHistory($"Була спроба видалення програми {app.DisplayName}");
            }
            return result;
        }

        public string ApplyServiceDisable(string name)
        {
            string result = _serviceManager.DisableService(name);
            if (result == "Успішно")
            {
                SessionStats.ServicesDisabled++;
                LogHistory($"Користувач вимикає службу {name}");
            }
            return result;
        }

        public string SetMinPrivacy(bool force = false)
        {
            if (!force && IsAlreadyApplied("MIN")) return "ALREADY_DONE";

            _activePrivacy = new MinimumPrivacy();
            string res = _activePrivacy.Apply();
            if (!res.ToLower().Contains("помилка"))
            {
                if (!IsAlreadyApplied("MIN")) SessionStats.RegistryTweaksApplied += 5;
                MarkAsApplied("MIN");
            }
            return res;
        }

        public string SetMedPrivacy(bool force = false)
        {
            if (!force && IsAlreadyApplied("MED")) return "ALREADY_DONE";

            _activePrivacy = new MediumPrivacy();
            string res = _activePrivacy.Apply();
            if (!res.ToLower().Contains("помилка"))
            {
                if (!IsAlreadyApplied("MED")) SessionStats.RegistryTweaksApplied += 12;
                MarkAsApplied("MED");
            }
            return res;
        }

        public string SetMaxPrivacy(bool force = false)
        {
            if (!force && IsAlreadyApplied("MAX")) return "ALREADY_DONE";

            _activePrivacy = new HighPrivacy();
            string res = _activePrivacy.Apply();
            if (!res.ToLower().Contains("помилка"))
            {
                if (!IsAlreadyApplied("MAX")) SessionStats.RegistryTweaksApplied += 25;
                MarkAsApplied("MAX");
            }
            return res;
        }

        public List<ServiceModel> GetOptimizationServices() => _serviceManager.GetTargetServices();
        public List<string> GetRemovedAppsList() => _appManager.RemovedAppsNames;
        public List<string> GetDisabledServicesList() => _serviceManager.DisabledServicesNames;
        public List<string> GetAppliedPrivacyTweaks() => _activePrivacy.AppliedTweaks;

        public void LaunchExplorer() => _quickLaunch.OpenExplorer();
        public void LaunchCmd() => _quickLaunch.OpenCmd();
        public void LaunchPowerShell() => _quickLaunch.OpenPowerShell();
        public void LaunchTaskMgr() => _quickLaunch.OpenTaskMgr();
        public void LaunchRegedit() => _quickLaunch.OpenRegedit();
        public void LaunchServices() => _quickLaunch.OpenServices();
        public void RollbackChanges() => _quickLaunch.OpenRestore();
    }
}