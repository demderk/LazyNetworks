using System;
using AdvancedTCP;
using System.Net;

namespace ServerApp
{
    class Program
    {
        static void Main(string[] args)
        {
            AdvancedTCPServer server = new AdvancedTCPServer(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 10201));
            server.MessageArrived += MessageArrived;
            server.ClientConnected += ClientConnected;
            server.ClientDisconnected += ClientDisconnected;
            while (true)
            {
                Console.Write("Server > ");
                string cin = Console.ReadLine();
                switch (cin)
                {
                    case "s/stop":
                        server.Stop();
                        Console.WriteLine("Server stopped. Press any key to contunue.");
                        Console.ReadKey();
                        return;
                    default:
                        server.Send(server.GetClients("127.0.0.1")[0], cin);
                        break;
                }
            }
        }

        private static void ClientDisconnected(Client client)
        {
            Console.WriteLine($"[{client.TcpClient.Client.RemoteEndPoint}] Client disconnected.");
        }

        private static void ClientConnected(System.Net.Sockets.TcpClient client, System.ComponentModel.CancelEventArgs e)
        {
            Console.WriteLine($"[{client.Client.RemoteEndPoint}] Client connected.");
        }

        private static void MessageArrived(IMessageBase<object> message)
        {
            Console.Write($"[{message.RemoteClient.TcpClient.Client.RemoteEndPoint}] > ");
            switch (message)
            {
                case NetworkXMLMessage msgXml:
                    Console.WriteLine(msgXml.MessageBody.InnerText);
                    break;
                default:
                    break;
            }
        }
    }
}
