using SignalR_Server_TestCase.Interfaces;

namespace SignalR_Server_TestCase.Models
{
    public class ClientInfoModel
    {
        public string? IpAddress { get; set; }
        public float MemoryUsage { get; set; }
        public float MemoryTotal { get; set; }
        public float CPUUsagePercentage { get; set; }
        public IEnumerable<StorageDeviceModel> Disks { get; set; }
    }
}
