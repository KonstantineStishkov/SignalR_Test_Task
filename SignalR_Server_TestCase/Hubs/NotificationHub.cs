using Entities;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR;
using SignalR_Server_TestCase.Models;
using SignalR_Server_TestCase.ViewModels;

namespace SignalR_Server_TestCase.Hubs
{
    public class NotificationHub : Hub
    {
        #region Constants
        const string startServiceMessage = "start";
        const string stopServiceMessage = "stop";
        const string requestMessageFormat = "Client Info Requested. Period = {0}";
        const string successStartMessage = "Service successfully started";
        const string successStopMessage = "Service successfully stopped";
        const string wrongMessage = "No such command";
        const string acceptedMessage = "Request accepted";
        #endregion

        List<ClientInfoModel> clients = new List<ClientInfoModel>();
        NpgSqlAdapter sqlAdapter = new NpgSqlAdapter();

        public Task SendMessage(string message, string ip)
        {
            Clients.Caller.SendAsync("Send", $"Server Message: {acceptedMessage}({message})");
            if (message.Trim() == startServiceMessage)
                return ProcessServiceStart(ip);

            if (message.Trim() == stopServiceMessage)
                return ProcessServiceStop(ip);

            return Clients.Caller.SendAsync("Send", $"Server Message: {wrongMessage}");
        }

        public Task SendInfo(string[] info)
        {
            ClientInfoModel client = new ClientInfoModel();
            client.Info = ComposeClientInfo(info);

            Clients.Caller.SendAsync("Send", $"Client Memory: {client.Info.MemoryUsage} / {client.Info.MemoryTotal}");
            Clients.Caller.SendAsync("Send", $"Client CPU Usage: {client.Info.CPUUsagePercentage}");

            foreach(var disk in client.Info.Disks)
            {
                Clients.Caller.SendAsync("Send", $"Disk: {disk.Literal}:{disk.DiskSpaceAvailable}/{disk.DiskSpaceTotal}");
            }

            try
            {
                sqlAdapter.AddInfo(client.Info);
            }
            catch(Exception ex)
            {
                Clients.Caller.SendAsync("Send", $"Exception: {ex.Message}");
            }
            return Task.CompletedTask;
        }

        private ClientInfo ComposeClientInfo(string[] info)
        {
            ClientInfo client = new ClientInfo()
            {
                IpAddress = info[0],
                CPUUsagePercentage = info[1],
                MemoryUsage = info[2],
                MemoryTotal = info[3],
            };
            int startCount = 4;
            int diskParametersCount = 3;
            int disksCount = (info.Length - startCount) / diskParametersCount;

            List<Disk> disks = new List<Disk>();
            for (int i = 0; i < disksCount; i++)
            {
                disks.Add(new Disk()
                {
                    Literal = info[(diskParametersCount * i) + startCount],
                    DiskSpaceAvailable = info[(diskParametersCount * i) + 1 + startCount],
                    DiskSpaceTotal = info[(diskParametersCount * i) + 2 + startCount]
                });
            }
            client.Disks = disks;
            return client;
        }
        public Task ProcessServiceStop(string ip)
        {
            ClientInfoModel client = GetClient(Clients.Caller, ip);
            client.IsActive = false;
            client.TokenSource.Cancel();
            sqlAdapter.UpdateClient(client);

            return Task.CompletedTask;
        }
        public Task ProcessServiceStart(string ip)
        {
            Clients.Caller.SendAsync("Send", $"Server Message: Surveying started");
            ClientInfoModel client = GetClient(Clients.Caller, ip);
            SendRequests(Clients.Caller, client.TokenSource.Token);
            return Clients.Caller.SendAsync("Send", $"Server Message: {successStartMessage}");
        }
        private async Task SendRequests(IClientProxy? client, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                client.SendAsync("SendRequest", OverallInfo.Period.TotalSeconds.ToString());
                client.SendAsync("Send", string.Format(requestMessageFormat, OverallInfo.Period));
                await Task.Delay(OverallInfo.Period);
            }

            client.SendAsync("Send", $"Server Message: {successStopMessage}");

            return;
        }
        private ClientInfoModel GetClient(IClientProxy caller, string ip)
        {
            ClientInfoModel client = clients.Find(x => x.Info.IpAddress == ip);
            if(client == null)
            {
                caller.SendAsync("Send", $"Server Message: Creating new user");
                client = new ClientInfoModel()
                {
                    Info = new ClientInfo()
                    {
                        IpAddress = ip
                    }
,                   
                    ConnectionId = Context.ConnectionId,
                    IsActive = true,
                    TokenSource = new CancellationTokenSource()
                };
                caller.SendAsync("Send", $"Server Message: user created");
                clients.Add(client);
                sqlAdapter.AddClient(client);
                caller.SendAsync("Send", $"Server Message: user saved");
            }

            return client;
        }
    }
}
