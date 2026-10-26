using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace CrossOptimizerAPI.CleanTrash
{
    public class CleanLocalTemp
    {
        // Основний метод для запуску очистки користувацької папки темп
        public (int files, int folders) StartCleaning()
        {
            // оголошуємо змінні за замовчуванням
            int filesDeleted = 0;
            int foldersDeleted = 0;

            // перевірка плафторми, де зараз працює проєкт
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return (0, 0);
            }

            try
            {
                // отримуємо шлях
                string localTempPath = Path.GetTempPath();

                // перебираємо та видялємо всі файли з папки
                foreach (var file in Directory.GetFiles(localTempPath))
                {
                    // обробка файлів які не можуть бути видалені (зайнятно системою)
                    try { File.Delete(file); filesDeleted++; } catch { } // збільшення лічильника при успішному видаленні
                }

                // перебиражмо всі папки та видаляємо їх
                foreach (var dir in Directory.GetDirectories(localTempPath))
                {
                    // обробка папок які зайняті системою
                    try { Directory.Delete(dir, true); foldersDeleted++; } catch { } // збільшення лічильника при успішному видаленні
                }

                // отримати значення, скільки видалено папок та файлів
                return (filesDeleted, foldersDeleted);
            }
            catch
            {
                // при невдачі отримуємо значення за замовчуванням
                return (filesDeleted, foldersDeleted);
            }
        }

        // Допоміжний метод для отримання версії віндовс, непотрібно
        private string GetWindowsVersion()
        {
            var os = Environment.OSVersion;
            if (os.Version.Major == 10)
            {
                return os.Version.Build >= 22000 ? "Windows 11" : "Windows 10";
            }
            return os.VersionString;
        }
    }
}