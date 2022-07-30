using Entities;
using Microsoft.AspNetCore.SignalR.Client;
using System.Diagnostics;

namespace SignalR_Client
{
    internal class Program
    {
        const string url = "http://89.208.137.229/notification";
        const string commandStart = "start";
        const string commandStop = "stop";

        const string welcomeMessage = "Welcome to survey client console";
        const string errorMessage = "Such command not exists";

        static async Task Main(string[] args)
        {
            SignalExchangeManager signalManager = new SignalExchangeManager(url);

            Console.WriteLine(welcomeMessage);

            bool isStop = false;

            while (!isStop)
            {
                var message = Console.ReadLine();

                switch (message)
                {
                    case commandStart:
                        signalManager.StartSurveyServiceAsync();
                        break;
                    case commandStop:
                        signalManager.StopSurveyService();
                        isStop = true;
                        break;
                    default:
                        Console.WriteLine(errorMessage);
                        break;
                }
            }

            Console.ReadLine();
        }
    }
}