using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SignalR_Client
{
    internal class SystemInfo
    {
        public event OnClientInfoCollectedEventHandler OnClientInfoCollected;
        private ReaderWriterLockSlim locker = new ReaderWriterLockSlim();
        private int collectedDataCount;
        private double[] processesCpuUsage;
        public void StartCollectingClientInfo()
        {
            Process[] processes = Process.GetProcesses();
            collectedDataCount = 0;

            GetCpuUsage(processes);
        }
        private async void GetClientInfo(double cpuUsage)
        {
            List<string> info = new List<string>();

            info.Add(string.Empty);
            info.Add(cpuUsage.ToString("P"));

            try
            {
                info.AddRange(GetMemoryInfo());
                info.AddRange(GetDisksInfo());
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            OnClientInfoCollected?.Invoke(info);
        }

        private List<string> GetMemoryInfo()
        {
            if (IsUnix())
                return GetLinuxMemoryInfo();
            else
                return GetWindowsMemoryInfo();
        }

        private List<string> GetWindowsMemoryInfo()
        {
            string output = string.Empty;
            List<string> result = new List<string>();

            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = "wmic";
            info.Arguments = "OS get FreePhysicalMemory,TotalVisibleMemorySize /Value";
            info.RedirectStandardOutput = true;

            using (Process process = Process.Start(info))
            {
                output = process.StandardOutput.ReadToEnd();
            }

            string[] lines = output.Trim().Split("\n");
            string[] freeMemoryParts = lines[0].Split("=", StringSplitOptions.RemoveEmptyEntries);
            string[] totalMemoryParts = lines[1].Split("=", StringSplitOptions.RemoveEmptyEntries);

            result.Add(ConvertBytesToString(long.Parse(totalMemoryParts[1]) - long.Parse(freeMemoryParts[1]), 1));
            result.Add(ConvertBytesToString(long.Parse(totalMemoryParts[1]), 1));
            return result;
        }

        private List<string> GetLinuxMemoryInfo()
        {
            string output = string.Empty;
            List<string> result = new List<string>();

            ProcessStartInfo info = new ProcessStartInfo("free -m");
            info.FileName = "/bin/bash";
            info.Arguments = "-c \"free -m\"";
            info.RedirectStandardOutput = true;

            using (Process process = Process.Start(info))
            {
                output = process.StandardOutput.ReadToEnd();
                Console.WriteLine(output);
            }

            string[] lines = output.Split("\n");
            string[] memory = lines[1].Split(" ", StringSplitOptions.RemoveEmptyEntries);

            result.Add(ConvertBytesToString(long.Parse(memory[2]), 2));
            result.Add(ConvertBytesToString(long.Parse(memory[1]), 2));
            return result;
        }

        private bool IsUnix()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ||
                          RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        }

        private List<string> GetDisksInfo()
        {
            List<string> result = new List<string>();
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.DriveType == DriveType.Fixed && drive.TotalSize > 0)
                {
                    result.Add(drive.Name.Length < 6 ? drive.Name : "None");
                    result.Add(ConvertBytesToString(drive.AvailableFreeSpace));
                    result.Add(ConvertBytesToString(drive.TotalSize));
                }
            }
            return result;
        }
        private string ConvertBytesToString(long bytes, int offset = 0)
        {
            const int grade = 1024;
            const int maxGrade = 4;
            string[] gradeNames = { "bytes", "Kb", "Mb", "Gb", "Tb" };

            int i = 0;
            decimal result = bytes;
            while((result > grade) && (i <= maxGrade))
            {
                result = result / grade;
                i++;
            }

            result = Math.Round(result, 2);
            return string.Format("{0} {1}", result, gradeNames[i + offset]);
        }
        private void GetCpuUsage(Process[] processes)
        {
            processesCpuUsage = new double[processes.Length];

            for (int i = 0; i < processesCpuUsage.Length; i++)
            {
                GetCpuUsageForProcessAsync(processes[i], i, processesCpuUsage.Length);
            }
        }

        private async Task GetCpuUsageForProcessAsync(Process process, int index, int length)
        {
            const int secondsDelay = 2;
            TimeSpan delay = TimeSpan.FromSeconds(secondsDelay);
            double cpuUsage;
            try
            {
                DateTime startTime = DateTime.Now;
                TimeSpan startCpuUsage = process.TotalProcessorTime;

                await Task.Delay(delay);

                DateTime endTime = DateTime.Now;
                TimeSpan endCpuUsage = process.TotalProcessorTime;

                double cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
                double totalMsPassed = (endTime - startTime).TotalMilliseconds;

                cpuUsage = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);
            }
            catch (Exception)
            {
                cpuUsage = 0;
            }

            processesCpuUsage[index] = cpuUsage;
            if (await CheckDataCount(length))
            {
                double cpuUsageTotal = CalculateCpuUsage();
                GetClientInfo(cpuUsageTotal);
            }
        }
        private async Task<bool> CheckDataCount(int targetValue)
        {
            TimeSpan delay = TimeSpan.FromMilliseconds(10);
            while (locker.IsWriteLockHeld)
                Task.Delay(delay);

            try
            {
                locker.EnterWriteLock();
                collectedDataCount++;
                if (collectedDataCount >= targetValue)
                    return true;
            }
            catch (Exception)
            {
                CheckDataCount(targetValue);
            }
            finally
            {
                locker.ExitWriteLock();
            }

            return false;
        }
        private double CalculateCpuUsage()
        {
            return processesCpuUsage.Sum(x => x);
        }
    }
}
