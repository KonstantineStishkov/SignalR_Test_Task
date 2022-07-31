using Entities;
using System.Linq;

namespace SignalR_Server_TestCase.Models
{
    public class ClientInfoModel
    {
        public ClientInfo Info { get; set; }
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
