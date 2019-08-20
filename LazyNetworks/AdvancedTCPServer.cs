using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Linq;
using System.Threading;
using System.Globalization;
using System.Xml;
using System.IO;

namespace AdvancedTCP
{
    public delegate void ConnectedClientEventHandler(TcpClient client, CancelEventArgs e);

    public delegate void DisconnectedClientEventHandler(Client client);

    public sealed class AdvancedTCPServer : AdvancedTCPBase
    {
        private TcpListener TcpServer { get; }

        private List<Client> Clients { get; } = new List<Client>();

        private readonly object ClientsLockObj = new object();

        public int OperationsDelay { get; set; } = 100;

        public AdvancedTCPServer(IPEndPoint endpint)
        {
            TcpServer = new TcpListener(endpint);
            TcpServer.Start();
            Start();
        }

        public event ConnectedClientEventHandler ClientConnected;

        public event DisconnectedClientEventHandler ClientDisconnected;

        public event EventHandler ServerStopped;

        private CancellationTokenSource ServerStop { get; } = new CancellationTokenSource();

        private void Start()
        {

            Rethrow(Task.Run(() =>
            {
                ClientsUpdater();
                while (!ServerStop.Token.IsCancellationRequested)
                {
                    ClientAvailableCheck();
                    MessageArrivedCheck();
                }
            }));
        }

        // New client accepting.
        private async void ClientsUpdater()
        {
            while (!ServerStop.Token.IsCancellationRequested)
            {
                TcpClient newClient;
                try
                {
                    newClient = await TcpServer.AcceptTcpClientAsync();
                }
                catch (ObjectDisposedException)
                {
                    if (ServerStop.Token.IsCancellationRequested)
                    {
                        return;
                    }
                    else
                    {
                        continue;
                    }
                }
                CancelEventArgs cancel = new CancelEventArgs();
                ClientConnected?.Invoke(newClient, cancel);
                if (cancel.Cancel)
                {
                    newClient.Close();
                }
                Clients.Add(new Client(newClient));
            }
        }

        // Client Available check.
        private void ClientAvailableCheck()
        {
            lock (ClientsLockObj)
            {
                for (int i = 0; i < Clients.Count; i++)
                {
                    Client client = Clients[i];
                    if (!SocketConnected(client.TcpClient.Client))
                    {
                        ClientDisconnected?.Invoke(client);
                        client.ClientStream.Close();
                        Clients.Remove(client);
                    }
                }
            }
            Thread.Sleep(OperationsDelay);
        }

        // Message could arrived.
        private void MessageArrivedCheck()
        {
            for (int i = 0; i < Clients.Count; i++)
            {
                var client = Clients[i];
                lock (ClientsLockObj)
                {
                    try
                    {
                        if (client.ClientStream.DataAvailable)
                        {
                            MessageAvailable(client);
                        }
                    }
                    catch (ObjectDisposedException)
                    {
                        Clients.Remove(client);
                    }
                }
            }
            Thread.Sleep(OperationsDelay);
        }

        public void Stop()
        {
            ServerStop.Cancel();
            TcpServer.Stop();
            foreach (var item in Clients)
            {
                item.TcpClient.Close();
            }
            Clients.Clear();
            ServerStopped?.Invoke(this, null);
        }

        public Client GetClient(string endPoint)
        {
            return GetClient(AdvancedTCPExtentions.ParseIPEndPoint(endPoint));
        }

        public Client GetClient(IPEndPoint ipPoint)
        {
            return Clients.Where(x => x.RemoteIP == ipPoint).FirstOrDefault();
        }

        public Client[] GetClients(IPAddress ip)
        {
            return Clients.Where(x => x.RemoteIP.Address.ToString() == ip.ToString()).ToArray();
        }

        public Client[] GetClients(string ip)
        {
            IPAddress ipAddress = IPAddress.Parse(ip);
            return GetClients(ipAddress);
        }

        public void SendAll(byte[] message)
        {
            lock (ClientsLockObj)
            {
                for (int i = 0; i < Clients.Count; i++)
                {
                    Clients[0].Send(message);
                }
            }
        }

        public void SendAll(string message)
        {
            lock (ClientsLockObj)
            {
                for (int i = 0; i < Clients.Count; i++)
                {
                    Clients[0].Send(message);
                }
            }
        }

    }
}
