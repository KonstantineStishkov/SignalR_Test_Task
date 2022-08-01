using Entities;

namespace SignalR_Server_TestCase.Models
{
    public class ClientInfoModel
    {
        public ClientInfo Info { get; set; }
        public string ConnectionId { get; set; }
        public bool IsActive { get; set; }
        public CancellationTokenSource TokenSource { get; set; }
    }
}
