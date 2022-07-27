using SignalR_Server_TestCase.Models;

namespace SignalR_Server_TestCase.Interfaces
{
    public interface IAllStorages
    {
        public IEnumerable<StorageDeviceModel> AllStorageDevices { get; }
    }
}
