namespace Entities
{
    public class ClientInfo
    {
        public string? IpAddress { get; set; }
        public long MemoryUsage { get; set; }
        public long MemoryTotal { get; set; }
        public double CPUUsagePercentage { get; set; }
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