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
        #region Constants
        const string ServerMethodSend = "Send";
        const string ServerMethodSendRequest = "SendRequest";

        const string MethodSendMessage = "SendMessage";
        const string ClientMethodSendInfo = "SendInfo";
        const string messageStart = "start";
        const string messageStop = "stop";
        #endregion

        private HubConnection hubConnection;
        private SystemInfo systemInfo;
        private bool hasActiveRequest;
        private string ip;

        internal SignalExchangeManager(string url)
        {
            hubConnection = new HubConnectionBuilder()
                            .WithUrl(url)
                            .Build();
            systemInfo = new SystemInfo();
        }
        public async void StartSurveyServiceAsync()
        {
            ip = await new HttpClient().GetStringAsync("https://api.ipify.org");

            hubConnection.On<string>(ServerMethodSend, message => Console.WriteLine(message));
            hubConnection.On<string>(ServerMethodSendRequest, OnRequestReceived);

            await hubConnection.StartAsync();
            Console.WriteLine($"Connection: {hubConnection.State}");
            hubConnection.SendAsync(MethodSendMessage, messageStart, ip);
        }
        public async void StopSurveyService()
        {
            hubConnection.Remove(ServerMethodSend);
            hubConnection.Remove(ServerMethodSendRequest);
            await hubConnection.SendAsync(MethodSendMessage, messageStop, ip);
        }
        public void OnRequestReceived(string request)
        {
            hasActiveRequest = true;
            Console.WriteLine($"Request Recieved. Period = {request}");
            systemInfo.OnClientInfoCollected += SendClientInfo;
            systemInfo.StartCollectingClientInfo();
        }
        private void SendClientInfo(IEnumerable<string> info)
        {
            if (hasActiveRequest)
            {
                hasActiveRequest = false;
                string[] arr = info.ToArray();
                arr[0] = ip;
                hubConnection.SendAsync(ClientMethodSendInfo, arr);
                systemInfo.OnClientInfoCollected -= SendClientInfo;
            }
        }
    }
}
