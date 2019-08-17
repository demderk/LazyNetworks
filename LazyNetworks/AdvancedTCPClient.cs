using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace AdvancedTCP
{


    public class AdvancedTCPClient : AdvancedTCPBase
    {

        public Client Client { get; private set; }

        public AdvancedTCPClient()
        {

        }

        public event DisconnectedClientEventHandler ClientDisconnected;

        private CancellationTokenSource ConnectionCancelSource { get; } = new CancellationTokenSource();

        private void Background()
        {
            Rethrow(Task.Run(() =>
            {
                while (!ConnectionCancelSource.Token.IsCancellationRequested)
                {
                    if (SocketConnected(Client.TcpClient.Client))
                    {
                        MessageArrivedCheck();
                    }
                    else
                    {
                        Disconnect();
                    }
                    Thread.Sleep(10);
                }
            }));
        }

        public void Disconnect()
        {
            ConnectionCancelSource.Cancel();
            ClientDisconnected?.Invoke(Client);
            Client.TcpClient.Close();
            Client = null;
        }

        private void MessageArrivedCheck()
        {
            if (Client.ClientStream.DataAvailable)
            {
                MessageAvailable(Client);
            }
        }

        public void Connect(IPEndPoint ip)
        {
            TcpClient client = new TcpClient();
            client.Connect(ip);
            Client = new Client(client);
            Background();
        }

        public void Connect(string ip)
        {
            Connect(TCPExtentions.ParseIPEndPoint(ip));
        }

        public bool TryConnect(IPEndPoint ip)
        {
            try
            {
                Connect(ip);
                return true;
            }
            catch (SocketException)
            {
                return false;
            }
        }

        public bool TryConnect(string ip)
        {
            return TryConnect(TCPExtentions.ParseIPEndPoint(ip));
        }

        public void Send(string message)
        {
            base.Send(Client, message);
        }

        public void Send(byte[] message)
        {
            base.Send(Client, message);
        }
    }
}
