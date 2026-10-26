using Avalonia.Controls;
using Avalonia.Interactivity;
using CrossOptimizerAPI;
using CrossOptimizerAPI.DeleteWebApps;
using CrossOptimizerAPI.RegistryManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace OptimizerUI
{
    public class DialogItem
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public bool IsChecked { get; set; }
        public object RawData { get; set; }
    }

    public partial class MainWindow : Window
    {
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

        private readonly CrossControlAPI _api = new CrossControlAPI();
        private string _currentDialogMode = "";
        private List<DialogItem> _allDialogItems = new List<DialogItem>();
        private string _lastClickedPrivacy = "";

        public MainWindow()
        {
            InitializeComponent();
            InitializeRestorePoint();
        }

        private async void InitializeRestorePoint()
        {
            MainContentArea.IsEnabled = false;
            StatusResult.Text = "Статус: Створення точки відновлення... Зачекайте.";
            string result = await _api.ExecuteRestorePointAsync();
            StatusResult.Text = "Статус: " + result;
            MainContentArea.IsEnabled = true;
        }

        public void OnOpenServicesDialogClick(object sender, RoutedEventArgs e)
        {
            _currentDialogMode = "Services";
            DialogTitle.Text = "Виберіть служби для вимкнення";
            SearchTextBox.Text = string.Empty;
            _allDialogItems = _api.GetOptimizationServices()
                               .Where(s => !s.IsDisabled)
                               .Select(s => new DialogItem { Id = s.ServiceName, DisplayName = s.DisplayName, IsChecked = false, RawData = s })
                               .ToList();

            DialogListBox.ItemsSource = _allDialogItems;
            OverlayDialog.IsVisible = true;
        }

        public void OnOpenAppsDialogClick(object sender, RoutedEventArgs e)
        {
            _currentDialogMode = "Apps";
            DialogTitle.Text = "Виберіть додатки для видалення";
            SearchTextBox.Text = string.Empty;
            _allDialogItems = _api.GetAppsList()
                               .Select(a => new DialogItem { Id = a.DisplayName, DisplayName = a.DisplayName, IsChecked = false, RawData = a })
                               .ToList();

            DialogListBox.ItemsSource = _allDialogItems;
            OverlayDialog.IsVisible = true;
        }

        public void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            var text = SearchTextBox.Text?.Trim().ToLower() ?? "";
            if (string.IsNullOrEmpty(text))
            {
                DialogListBox.ItemsSource = _allDialogItems;
            }
            else
            {
                DialogListBox.ItemsSource = _allDialogItems
                    .Where(i => i.DisplayName != null && i.DisplayName.ToLower().Contains(text))
                    .ToList();
            }
        }

        public async void OnDialogApplyClick(object sender, RoutedEventArgs e)
        {
            OverlayDialog.IsVisible = false;
            int count = 0;

            foreach (var item in _allDialogItems.Where(i => i.IsChecked).ToList())
            {
                if (_currentDialogMode == "Services")
                {
                    bool confirm = await ShowConfirmDialogAsync($"Зараз буде вимкнена служба {item.DisplayName}, ви впевнені?");
                    if (confirm)
                    {
                        _api.ApplyServiceDisable(item.Id);
                        count++;
                    }
                }
                else if (_currentDialogMode == "Apps" && item.RawData is AppModel app)
                {
                    bool confirm = await ShowConfirmDialogAsync($"Зараз буде видалена програма {item.DisplayName}, ви впевнені?");
                    if (confirm)
                    {
                        _api.RemoveSelectedApp(app);
                        count++;
                    }
                }
            }

            StatusResult.Text = _currentDialogMode == "Services"
                ? $"Вимкнено служб: {count}"
                : $"Видалення додатків: {count}";
        }

        private System.Threading.Tasks.Task<bool> ShowConfirmDialogAsync(string message)
        {
            var tcs = new System.Threading.Tasks.TaskCompletionSource<bool>();
            var dialog = new Window
            {
                Title = "Підтвердження",
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false,
                Padding = new Avalonia.Thickness(20),
                MinWidth = 300,
                MinHeight = 120
            };

            var text = new TextBlock
            {
                Text = message,
                Margin = new Avalonia.Thickness(0, 0, 0, 20),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap
            };

            var btnYes = new Button
            {
                Content = "Так",
                Margin = new Avalonia.Thickness(5),
                Width = 80,
                HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center
            };
            btnYes.Click += (s, e) => { tcs.TrySetResult(true); dialog.Close(); };

            var btnNo = new Button
            {
                Content = "Ні",
                Margin = new Avalonia.Thickness(5),
                Width = 80,
                HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center
            };
            btnNo.Click += (s, e) => { tcs.TrySetResult(false); dialog.Close(); };

            var panelButtons = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            };
            panelButtons.Children.Add(btnYes);
            panelButtons.Children.Add(btnNo);

            var mainPanel = new StackPanel();
            mainPanel.Children.Add(text);
            mainPanel.Children.Add(panelButtons);

            dialog.Content = mainPanel;
            dialog.Closed += (s, e) => { tcs.TrySetResult(false); };

            dialog.ShowDialog(this);
            return tcs.Task;
        }

        public void OnDialogCancelClick(object sender, RoutedEventArgs e)
        {
            OverlayDialog.IsVisible = false;
        }

        public void OnLockUIClick(object sender, RoutedEventArgs e) => MainContentArea.IsEnabled = false;
        public void OnUnlockUIClick(object sender, RoutedEventArgs e) => MainContentArea.IsEnabled = true;
        public void OnCleanLocalClick(object sender, RoutedEventArgs e) => StatusResult.Text = _api.ExecuteLocalCleanup();
        public void OnCleanGlobalClick(object sender, RoutedEventArgs e) => StatusResult.Text = _api.ExecuteGlobalCleanup();

        public void OnPrivacyMinClick(object sender, RoutedEventArgs e)
        {
            string res = _api.SetMinPrivacy(_lastClickedPrivacy == "MIN");
            if (res == "ALREADY_DONE")
            {
                StatusResult.Text = "Ця оптимізація вже виконана. Натисніть ще раз для повтору.";
                _lastClickedPrivacy = "MIN";
            }
            else
            {
                StatusResult.Text = res;
                _lastClickedPrivacy = "";
            }
        }

        public void OnPrivacyMedClick(object sender, RoutedEventArgs e)
        {
            string res = _api.SetMedPrivacy(_lastClickedPrivacy == "MED");
            if (res == "ALREADY_DONE")
            {
                StatusResult.Text = "Ця оптимізація вже виконана. Натисніть ще раз для повтору.";
                _lastClickedPrivacy = "MED";
            }
            else
            {
                StatusResult.Text = res;
                _lastClickedPrivacy = "";
            }
        }

        public void OnPrivacyMaxClick(object sender, RoutedEventArgs e)
        {
            string res = _api.SetMaxPrivacy(_lastClickedPrivacy == "MAX");
            if (res == "ALREADY_DONE")
            {
                StatusResult.Text = "Ця оптимізація вже виконана. Натисніть ще раз для повтору.";
                _lastClickedPrivacy = "MAX";
            }
            else
            {
                StatusResult.Text = res;
                _lastClickedPrivacy = "";
            }
        }

        public void OnShowSystemWarningClick(object sender, RoutedEventArgs e)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                string title = "Що конкретно вимикає програма? Для інформації:";
                string text = "Увага! Програма може внести наступні зміни (залежно від застосованих параметрів):\n\n" +
                              "1. ПРИВАТНІСТЬ (РЕЄСТР):\n" +
                              " • Телеметрія — HKLM\\...\\DataCollection\\AllowTelemetry = 0\n" +
                              " • Персоналізація — HKCU\\...\\Privacy\\TailoredExperiencesWithDiagnosticDataEnabled = 0\n" +
                              " • Рекламний ID — HKCU\\...\\AdvertisingInfo\\Enabled = 0\n" +
                              " • Спонсорські пропозиції — HKLM\\...\\CloudContent\\DisableWindowsConsumerFeatures = 1\n" +
                              " • Звіти помилок — HKLM\\...\\ErrorReporting\\DoReport = 0\n" +
                              " • Автооновлення — HKLM\\...\\Auto Update\\AUOptions = 2\n" +
                              " • Збір даних ПЗ — HKLM\\...\\AppCompat\\DisableInventory = 1\n" +
                              " • Відстеження запусків — HKCU\\...\\Explorer\\Advanced\\Start_TrackProgs = 0\n" +
                              " • Історія пошуку — HKCU\\...\\Search\\SearchHistoryEnabled = 0\n" +
                              " • Публікація активностей — HKLM\\...\\System\\PublishUserActivities = 0\n" +
                              " • Cortana — HKLM\\...\\Windows Search\\AllowCortana = 0\n" +
                              " • Стрічка активності — HKLM\\...\\System\\EnableActivityFeed = 0\n" +
                              " • Геолокація — HKLM\\...\\LocationAndSensors\\DisableLocation = 1\n" +
                              " • Аналіз тексту — HKCU\\...\\InputPersonalization\\RestrictImplicitTextCollection = 1\n" +
                              " • Рукописний ввід — HKCU\\...\\InputPersonalization\\RestrictImplicitInkCollection = 1\n" +
                              " • Запити відгуків — HKLM\\...\\DataCollection\\DoNotShowFeedbackNotifications = 1\n" +
                              " • Біометрія — HKLM\\...\\Biometrics\\Enabled = 0\n" +
                              " • Синхронізація хмари — HKLM\\...\\SettingSync\\DisableSettingSync = 2\n\n" +
                              "2. СЛУЖБИ (Вимкнення, Start = 4):\n" +
                              " • DiagTrack — HKLM\\SYSTEM\\CurrentControlSet\\Services\\DiagTrack\\Start = 4\n" +
                              " • dmwappushservice — HKLM\\SYSTEM\\CurrentControlSet\\Services\\dmwappushservice\\Start = 4\n" +
                              " • WerSvc — HKLM\\SYSTEM\\CurrentControlSet\\Services\\WerSvc\\Start = 4\n" +
                              " • XblAuthManager — HKLM\\SYSTEM\\CurrentControlSet\\Services\\XblAuthManager\\Start = 4\n" +
                              " • XblGameSave — HKLM\\SYSTEM\\CurrentControlSet\\Services\\XblGameSave\\Start = 4\n" +
                              " • XboxNetApiSvc — HKLM\\SYSTEM\\CurrentControlSet\\Services\\XboxNetApiSvc\\Start = 4\n" +
                              " • PhoneSvc — HKLM\\SYSTEM\\CurrentControlSet\\Services\\PhoneSvc\\Start = 4\n\n" +
                              "3. ВИДАЛЕННЯ ПРОГРАМ:\n" +
                              " • Будьте обережні при видаленні програм! Перевіряйте доцільність деінсталяції.";

                MessageBox(IntPtr.Zero, text, title, 0x00000030);
            }
        }

        public void OnOpenCmdClick(object sender, RoutedEventArgs e) => _api.LaunchCmd();
        public void OnOpenPowerShellClick(object sender, RoutedEventArgs e) => _api.LaunchPowerShell();
        public void OnOpenExplorerClick(object sender, RoutedEventArgs e) => _api.LaunchExplorer();
        public void OnOpenTaskMgrClick(object sender, RoutedEventArgs e) => _api.LaunchTaskMgr();
        public void @OnOpenRegeditClick(object sender, RoutedEventArgs e) => _api.LaunchRegedit();
        public void OnOpenServicesClick(object sender, RoutedEventArgs e) => _api.LaunchServices();
        public void OnRollbackClick(object sender, RoutedEventArgs e) => _api.RollbackChanges();
        public void OnShowStatsClick(object sender, RoutedEventArgs e)
        {
            var s = _api.SessionStats;
            StatusResult.Text = $"Видалено: {s.FilesDeleted} ф. / {s.FoldersDeleted} п. | Додатків: {s.AppsRemoved} | Служб: {s.ServicesDisabled} | Твіків: {s.RegistryTweaksApplied}";
        }
    }
}