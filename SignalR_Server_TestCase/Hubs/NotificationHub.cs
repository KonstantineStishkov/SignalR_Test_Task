using Entities;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR;
using SignalR_Server_TestCase.Models;
using SignalR_Server_TestCase.ViewModels;

namespace SignalR_Server_TestCase.Hubs
{
    public class NotificationHub : Hub
    {
        const string startServiceMessage = "-start";
        const string stopServiceMessage = "-stop";
        const string requestMessage = "Client Info Requested";
        const string successStartMessage = "Service successfully started";
        const string successStopMessage = "Service successfully stopped";
        const string wrongMessage = "No such command";

        List<ClientModel> clients = new List<ClientModel>();
        NpgSqlAdapter sqlAdapter = new NpgSqlAdapter();

        public Task SendMessage(string message)
        {
            if (message.Trim() == startServiceMessage)
                return ProcessServiceStart();

            if (message.Trim() == stopServiceMessage)
                return ProcessServiceStop();

            return Clients.Caller.SendAsync("Send", $"Server Message: {wrongMessage}");
        }
        public Task SendInfo(ClientInfo info)
        {
            ClientModel? client = GetClient();
            return Task.CompletedTask;
        }
        private Task ProcessServiceStop()
        {
            ClientModel client = GetClient();
            client.IsActive = false;
            client.TokenSource.Cancel();
            sqlAdapter.UpdateClient(client);

            return Task.CompletedTask;
        }
        private Task ProcessServiceStart()
        {
            ClientModel client = GetClient();

            SendRequests(Clients.Caller, client.TokenSource.Token);
            return Clients.Caller.SendAsync("Send", $"Server Message: {successStartMessage}");
        }
        private async Task SendRequests(IClientProxy? client, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                client.SendAsync("SendRequest");
                client.SendAsync("Send", requestMessage);
                await Task.Delay(OverallInfo.Period);
            }

            client.SendAsync("Send", $"Server Message: {successStopMessage}");

            return;
        }
        private ClientModel GetClient()
        {
            ClientModel client = clients.Find(x => x.ConnectionId == Context.ConnectionId);
            if(client == null)
            {
                client = new ClientModel()
                {
                    IpAddress = Context.Features.Get<HttpConnectionFeature>().RemoteIpAddress.ToString(),
                    ConnectionId = Context.ConnectionId,
                    IsActive = true,
                    TokenSource = new CancellationTokenSource()
                };
                clients.Add(client);
                sqlAdapter.AddClient(client);
            }

            return client;
        }
    }
}
