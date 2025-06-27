using System.Diagnostics;

namespace EdNexusData.Broker.Updater;

public class Systemd
{
    static void StopSystemdService(string serviceName)
    {
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = "systemctl",
            Arguments = $"stop {serviceName}",
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

    static void StartSystemdService(string serviceName)
    {
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = "systemctl",
            Arguments = $"start {serviceName}",
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
            FileName = "systemctl",
            Arguments = $"status {serviceName}",
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

    static void CheckParentProcess(int pid)
    {
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = "ps",
            Arguments = $"-o ppid= -p {pid}",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (Process process = new Process { StartInfo = psi })
        {
            process.Start();
            string output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();

            if (output == "1")
            {
                Console.WriteLine($"Process {pid} was started by systemd.");
            }
            else
            {
                Console.WriteLine($"Process {pid} was NOT started by systemd. Parent PID: {output}");
            }
        }
    }
}