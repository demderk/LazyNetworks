using System.Net;
using System.Net.Sockets;


namespace AdvancedTCP
{
    public class Client
    {
        public Client(TcpClient client)
        {
            TcpClient = client;
            ClientStream = client.GetStream();
        }

        public TcpClient TcpClient { get; }

        public IPEndPoint RemoteIP => (IPEndPoint)TcpClient.Client.RemoteEndPoint;

        public NetworkStream ClientStream { get; }

    }
}
