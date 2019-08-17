using System;
using AdvancedTCP;
using System.Collections.Generic;
using System.Threading;

namespace ClientApp
{
    class Program
    {
        static void Main(string[] args)
        {
            AdvancedTCPClient connection = new AdvancedTCPClient();
            while (true)
            {
                if (!connection.TryConnect("127.0.0.1:10201"))
                {
                    Console.WriteLine("Connection failed. Retrying...");
                }
                else break;
            }
            connection.MessageArrived += ConnectionMessageArrived;
            connection.ClientDisconnected += ConnectionClientDisconnected;
            Console.WriteLine("Connected");
            Console.WriteLine("Mode: string. What to send?");
            while (true)
            {
                string cin = Console.ReadLine();
                List<AdvancedTCPClient> clients = new List<AdvancedTCPClient>();
                if (cin == "ddos")
                {
                    while (true)
                    {
                        var vnt = new AdvancedTCPClient();
                        vnt.Connect("127.0.0.1:10201");
                        vnt.Send("<string>Login</string>");
                        clients.Add(vnt);
                        Thread.Sleep(10);
                    }
                }
                else if (cin == "c/stop")
                {
                    connection.Disconnect();
                    Console.WriteLine("Connection has been interrupted. Press any key for exit.");
                    Console.ReadKey();
                    return;
                }
                connection.Send(cin);
            }
        }

        private static void ConnectionClientDisconnected(Client client)
        {
            Console.WriteLine("Disconnected");
        }

        private static void ConnectionMessageArrived(IMessageBase<object> message)
        {
            switch (message)
            {
                case NetworkXMLMessage msgXml:
                    Console.WriteLine(msgXml);
                    break;
                default:
                    break;
            }


        }
    }
}
