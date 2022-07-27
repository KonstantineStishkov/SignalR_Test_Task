using SignalR_Server_TestCase.Interfaces;
using SignalR_Server_TestCase.Models;
using System.Linq;

namespace SignalR_Server_TestCase.Mocks
{
    public class MockClients : IAllClients
    {
        const string ip1 = "192.168.0.1";
        const string ip2 = "172.0.0.1";
        public readonly IAllStorages storages = new MockDisks();
        public IEnumerable<ClientInfoModel> AllClients
        {
            get
            {
                return new List<ClientInfoModel>()
                {
                    new ClientInfoModel { IpAddress = ip1,
                                          MemoryUsage = 31.6f,
                                          MemoryTotal = 64,
                                          CPUUsagePercentage = 0.125f,
                                          Disks = storages.AllStorageDevices.Where(x => x.IpAddress == ip1).Select(x => x) },
                    new ClientInfoModel { IpAddress = ip2, 
                                          MemoryUsage = 14.62f, 
                                          MemoryTotal = 64, 
                                          CPUUsagePercentage = 0.743f,
                                          Disks = storages.AllStorageDevices.Where(x => x.IpAddress == ip2).Select(x => x) }
                };
            }
        }
    }
}
