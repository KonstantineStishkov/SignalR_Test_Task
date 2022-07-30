using Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

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
        private void GetClientInfo(double cpuUsage)
        {
            ClientInfo client = new ClientInfo();

            client.CPUUsagePercentage = cpuUsage;
            client.Disks = GetDisksInfo();

            GetMemoryInfo(ref client);

            OnClientInfoCollected?.Invoke(client);
        }

        private void GetMemoryInfo(ref ClientInfo client)
        {
            if (IsUnix())
                GetLinuxMemoryInfo(ref client);
            else
                GetWindowsMemoryInfo(ref client);
        }

        private static void GetWindowsMemoryInfo(ref ClientInfo client)
        {
            string output = string.Empty;

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

            client.MemoryTotal = long.Parse(totalMemoryParts[1]);
            long freeMemomry = long.Parse(freeMemoryParts[1]);
            client.MemoryUsage = client.MemoryTotal - freeMemomry;
        }

        private static void GetLinuxMemoryInfo(ref ClientInfo client)
        {
            string? output = string.Empty;

            var info = new ProcessStartInfo("free -m");
            info.FileName = "/bin/bash";
            info.Arguments = "-c \"free -m\"";
            info.RedirectStandardOutput = true;

            using (var process = Process.Start(info))
            {
                output = process.StandardOutput.ReadToEnd();
                Console.WriteLine(output);
            }

            var lines = output.Split("\n");
            var memory = lines[1].Split(" ", StringSplitOptions.RemoveEmptyEntries);

            client.MemoryTotal = long.Parse(memory[1]);
            client.MemoryUsage = long.Parse(memory[2]);
        }

        private bool IsUnix()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ||
                          RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        }

        private IEnumerable<Disk> GetDisksInfo()
        {
            List<Disk> disks = new List<Disk>();

            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.DriveType == DriveType.Fixed)
                {
                    disks.Add(new Disk()
                    {
                        Literal = drive.Name.First(),
                        DiskSpaceAvailable = drive.AvailableFreeSpace,
                        DiskSpaceTotal = drive.TotalFreeSpace
                    });
                }
            }

            return disks;
        }

        private void GetCpuUsage(Process[] processes)
        {
            processesCpuUsage = new double[processes.Length];

            for (int i = 0; i < processesCpuUsage.Length; i++)
            {
                Console.WriteLine(i + "/" + processesCpuUsage.Length + ":" + processes[i].ToString());
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
                Console.WriteLine(collectedDataCount++);
                if (collectedDataCount >= targetValue - 1)
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
