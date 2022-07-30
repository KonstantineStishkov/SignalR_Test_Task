using Entities;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalR_Client
{
    internal class SignalExchangeManager
    {
        private HubConnection hubConnection;
        private SystemInfo systemInfo;

        internal SignalExchangeManager(string url)
        {
            hubConnection = new HubConnectionBuilder()
                            .WithUrl(url)
                            .Build();
            systemInfo = new SystemInfo();
        }
        public async void StartSurveyServiceAsync()
        {
            hubConnection.On<string>("Send", message => Console.WriteLine(message));
            hubConnection.On<string>("SendRequest", OnRequestReceived);

            await hubConnection.StartAsync();
            await hubConnection.SendAsync("ProcessServiceStart");
        }
        public async void StopSurveyService()
        {
            hubConnection.Remove("Send");
            hubConnection.Remove("SendRequest");
            await hubConnection.SendAsync("ProcessServiceStop");
        }
        public void OnRequestReceived(string request)
        {
            systemInfo.OnClientInfoCollected += SendClientInfo;
            systemInfo.StartCollectingClientInfo();
        }
        private void SendClientInfo(ClientInfo client)
        {
            hubConnection.SendAsync("SendInfo", client);
            systemInfo.OnClientInfoCollected -= SendClientInfo;
        }
    }
}
