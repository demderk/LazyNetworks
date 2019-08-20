using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AdvancedTCP
{
    public sealed class Client
    {
        public Client(TcpClient client)
        {
            TcpClient = client;
            ClientStream = client.GetStream();
        }

        public TcpClient TcpClient { get; }

        public IPEndPoint RemoteIP => (IPEndPoint)TcpClient.Client.RemoteEndPoint;

        public NetworkStream ClientStream { get; }

        internal Dictionary<int, IMessageBase<object>> QuestionsArray { get; } = new Dictionary<int, IMessageBase<object>>();

        internal readonly object QuestionsArrayLock = new object();

        private Random Random { get; } = new Random();

        public void Send(string message)
        {
            XElement node = new XElement("String", message);
            Send(AdvancedTCPExtentions.BuildXMLQuerry(node.ToString()));
        }

        public void Send(byte[] message)
        {
            if (TcpClient.Connected != false)
            {
                ClientStream.Write(message, 0, message.Length);
            }
            else
            {
                throw new InvalidOperationException("Connection not installed");
            }
        }

        public IMessageBase<object> SendQuestion(byte[] message, double timeout = 300)
        {
            int questionCode = Random.Next(0x0000, 0xFFFF);
            while (QuestionsArray.ContainsKey(questionCode))
            {
                questionCode = Random.Next(0x0000, 0xFFFF);
            }
            lock (QuestionsArrayLock)
            {
                QuestionsArray.Add(questionCode, null);
            }
            byte[] questionCodeBytes = BitConverter.GetBytes(questionCode);
            List<byte> messageBytes = new List<byte>()
                {
                    0x0081,
                    0x0002,
                    0x0084,
                    0x0081
                };
            messageBytes.AddRange(questionCodeBytes);
            messageBytes.Add(0x0000);
            messageBytes.Add(0x0084);
            messageBytes.AddRange(message);
            Send(messageBytes.ToArray());
            for (int i = 0; i < timeout / 100; i++)
            {
                if (QuestionsArray[questionCode] != null)
                {
                    break;
                }
                Thread.Sleep(100);
            }
            if (QuestionsArray[questionCode] != null)
            {
                IMessageBase<object> obj = QuestionsArray[questionCode];
                QuestionsArray.Remove(questionCode);
                return obj;
            }
            else
            {
                lock (QuestionsArrayLock)
                {
                    QuestionsArray.Remove(questionCode);
                }
                return null;
            }
        }

        public void SendQuestion(byte[] message, Action<IMessageBase<object>> callback, double timeout = 300)
        {
            Task.Run(() => callback(SendQuestion(message, timeout)));
        }

        public IMessageBase<object> SendQuestion(string message, double timeout = 300)
        {
            return SendQuestion(AdvancedTCPExtentions.BuildXMLQuerry(message), timeout);
        }

        public void SendQuestion(string message, Action<IMessageBase<object>> callback, double timeout = 300)
        {
            Task.Run(() => callback(SendQuestion(AdvancedTCPExtentions.BuildXMLQuerry(message), timeout)));
        }
    }
}
