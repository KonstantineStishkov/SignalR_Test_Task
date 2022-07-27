using SignalR_Server_TestCase.Interfaces;
using SignalR_Server_TestCase.Models;

namespace SignalR_Server_TestCase.Mocks
{
    public class MockDisks : IAllStorages
    {
        public IEnumerable<StorageDeviceModel> AllStorageDevices
        {
            get
            {
                return new List<StorageDeviceModel>()
                {
                    new StorageDeviceModel { IpAddress = "192.168.0.1", Literal = 'C', DiskSpaceAvailable = 134.5f, DiskSpaceTotal = 512 },
                    new StorageDeviceModel { IpAddress = "192.168.0.1", Literal = 'D', DiskSpaceAvailable = 114.23f, DiskSpaceTotal = 256 },
                    new StorageDeviceModel { IpAddress = "192.168.0.1", Literal = 'D', DiskSpaceAvailable = 567.43f, DiskSpaceTotal = 1024 },
                    new StorageDeviceModel { IpAddress = "172.0.0.1", Literal = 'C', DiskSpaceAvailable = 1166.32f, DiskSpaceTotal = 2048 }
                };
            }
        }
    }
}
