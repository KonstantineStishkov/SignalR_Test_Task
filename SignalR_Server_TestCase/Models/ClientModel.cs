using Entities;
using SignalR_Server_TestCase.Interfaces;
using System.Linq;

namespace SignalR_Server_TestCase.Models
{
    public class ClientModel
    {
        public string? IpAddress { get; set; }
        public float MemoryUsage { get; set; }
        public float MemoryTotal { get; set; }
        public float CPUUsagePercentage { get; set; }
        public StorageDeviceModel? MainDisk
        {
            get
            {
                return Disks.FirstOrDefault();
            }
        }
        public IEnumerable<StorageDeviceModel>? Disks { get; set; }
        ClientInfo Info { get; set; }
        public string ConnectionId { get; set; }
        public bool IsActive { get; set; }
        public CancellationTokenSource TokenSource { get; set; }
        //public ClientInfoModel(Client client, string connectionId, bool isActive, CancellationToken token)
        //{
        //    Client = client;
        //    ConnectionId = connectionId;
        //    IsActive = isActive;
        //    Token = token;
        //}
    }
}
