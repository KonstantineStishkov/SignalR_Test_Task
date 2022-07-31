using Microsoft.AspNetCore.SignalR.Client;
using System.Diagnostics;
using System.Net;
using System.Net.Http;

namespace SignalR_Client
{
    internal class Program
    {
        #region Constants
        const string url = "http://89.208.137.229/notification";
        const string commandStart = "start";
        const string commandStop = "stop";
        const string help = "?";

        const string welcomeMessage = "Welcome to survey client console. Input \"?\" to get list of available commands.";
        const string errorMessage = "Such command not exists";
        #endregion

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
                        Console.WriteLine("Starting surveying...");
                        signalManager.StartSurveyServiceAsync();
                        break;
                    case commandStop:
                        Console.WriteLine("Surveying stopped");
                        signalManager.StopSurveyService();
                        isStop = true;
                        break;
                    case help:
                        Console.WriteLine(commandStart + " - starts surveying system info");
                        Console.WriteLine(commandStop + " - stops surveying system info");
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