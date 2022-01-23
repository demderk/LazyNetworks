using System;
using AdvancedTCP;
using System.Net;

namespace ServerApp
{
    class Program
    {
        static readonly AdvancedTCPServer server = new AdvancedTCPServer(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 10201));
        static void Main()
        {
            server.MessageArrived += MessageArrived;
            server.ClientConnecting += ClientConnecting;
            server.ClientDisconnected += ClientDisconnected;
            while (true)
            {
                Console.Write("Server > ");
                string cin = Console.ReadLine();
                switch (cin)
                {
                    case "s/stop":
                        server.Stop();
                        Console.WriteLine("Server has been stopped. Press any key to contunue.");
                        Console.ReadKey();
                        return;
                    default:
                        server.SendAll(cin);
                        break;
                }
            }
        }

        private static void ClientDisconnected(Client client)
        {
            Console.WriteLine($"[{client.Adress}] Client disconnected.");
        }

        private static void ClientConnecting(System.Net.Sockets.TcpClient client, System.ComponentModel.CancelEventArgs e)
        {
            Console.WriteLine($"[{client.Client.RemoteEndPoint}] Client connected.");
        }

        private static void MessageArrived(IMessageBase<object> message)
        {
            Console.Write($"[{message.RemoteClient.TcpClient.Client.RemoteEndPoint}] > ");
            if (message.Question != null && message.Question.IsAnswer == false)
            {
                switch (message)
                {
                    case NetworkXMLMessage xmlMsg:
                        message.Question.Answer($"<string>[SERVER MIRROR > ] {xmlMsg}</string>");
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
