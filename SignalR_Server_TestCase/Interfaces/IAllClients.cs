using SignalR_Server_TestCase.Models;

namespace SignalR_Server_TestCase.Interfaces
{
    public interface IAllClients
    {
        public IEnumerable<ClientModel>? AllClients { get; }
    }
}
