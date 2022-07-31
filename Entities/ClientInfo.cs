namespace Entities
{
    public class ClientInfo
    {
        public string IpAddress { get; set; }
        public string MemoryUsage { get; set; }
        public string MemoryTotal { get; set; }
        public string CPUUsagePercentage { get; set; }
        public Disk? MainDisk
        {
            get
            {
                return Disks.FirstOrDefault();
            }
        }
        public IEnumerable<Disk>? Disks { get; set; }
    }
}