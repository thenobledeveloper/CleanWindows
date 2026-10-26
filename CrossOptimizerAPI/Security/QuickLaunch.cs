using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CrossOptimizerAPI.Security;

public class QuickLaunch
{
    public void RunAdmin(string fileName, string arguments = "")
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;

        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = true,
            Verb = "runas"
        };

        try
        {
            Process.Start(psi);
        }
        catch { }
    }

    public void OpenExplorer() => RunAdmin("explorer.exe", "C:\\");
    public void OpenCmd() => RunAdmin("cmd.exe");
    public void OpenPowerShell() => RunAdmin("powershell.exe");
    public void OpenTaskMgr() => RunAdmin("taskmgr.exe");
    public void OpenRegedit() => RunAdmin("regedit.exe");
    public void OpenServices() => RunAdmin("services.msc");
    public void OpenRestore() => RunAdmin("rstrui.exe");
}