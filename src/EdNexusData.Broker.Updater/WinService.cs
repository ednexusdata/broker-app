using System.Diagnostics;

namespace EdNexusData.Broker.Updater;

public class WindowsService
{
    static void StopWindowsService(string serviceName)
    {
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/c net stop {serviceName}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (Process process = new Process { StartInfo = psi })
        {
            process.Start();
            process.WaitForExit();
            Console.WriteLine($"Stopped service: {serviceName}");
        }
    }

    static void StopWindowsServicePowerShell(string serviceName)
    {
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-Command \"Stop-Service -Name '{serviceName}'\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (Process process = new Process { StartInfo = psi })
        {
            process.Start();
            process.WaitForExit();
            Console.WriteLine($"Stopped service: {serviceName}");
        }
    }

    static void StartWindowsService(string serviceName)
    {
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/c net start {serviceName}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (Process process = new Process { StartInfo = psi })
        {
            process.Start();
            process.WaitForExit();
            Console.WriteLine($"Started service: {serviceName}");
        }
    }

    static void StartWindowsServicePowerShell(string serviceName)
    {
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-Command \"Start-Service -Name '{serviceName}'\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (Process process = new Process { StartInfo = psi })
        {
            process.Start();
            process.WaitForExit();
            Console.WriteLine($"Started service: {serviceName}");
        }
    }

    static void CheckServiceStatus(string serviceName)
    {
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-Command \"Get-Service -Name '{serviceName}'\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (Process process = new Process { StartInfo = psi })
        {
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            Console.WriteLine(output);
        }
    }
}