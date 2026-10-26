using System;
using System.IO;
using System.Runtime.InteropServices;

namespace CrossOptimizerAPI.CleanTrash;

public class TheCleanerFolderGlobalTEMP
{
    // метод для очищення глобальної системної папки темп
    public (int files, int folders) StartCleaning()
    {
        // оголошуємо змінні за замовчуванням
        int filesDeleted = 0;
        int foldersDeleted = 0;

        // перевіряємо платформу
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            try
            {
                // звичайний шлях до системного мусора
                string winTempPath = @"C:\Windows\Temp";

                // використовуємо змінну для пошука
                string[] files = Directory.GetFiles(winTempPath);
                foreach (var file in files)
                {
                    // видаляємо файли з папки та додаємо значенні до лічильника ++
                    try { File.Delete(file); filesDeleted++; } catch { } // ігноруємо файли які зайняті системою
                }

                // використовуємо змінну для пошука
                string[] dirs = Directory.GetDirectories(winTempPath);
                foreach (var dir in dirs)
                {
                    // видаляємо папки, та додаємо значення до лічильника
                    try { Directory.Delete(dir, true); foldersDeleted++; } catch { } // ігноруємо папки які зайняті системою
                }

                // повертаємо значення лічильника
                return (filesDeleted, foldersDeleted);
            }
            catch
            {
                // обробка помилки, повертаємо значення за замовчуванням
                return (filesDeleted, foldersDeleted);
            }
        }

        return (0, 0);
    }
}