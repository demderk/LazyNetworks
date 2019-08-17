using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace AdvancedTCP
{
    public delegate void MessageArrivedEventHandler(IMessageBase<object> message);

    public abstract class AdvancedTCPBase
    {
        public virtual event MessageArrivedEventHandler MessageArrived;

        protected async void Rethrow(Task task)
        {
            try
            {
                await task;
            }
            catch
            {
                throw;
            }
        }

        private byte[] BuildXMLQuerry(string text)
        {
            bool extended = text.Length > 255 ? true : false;
            List<byte> bytes = new List<byte>();
            bytes.Add(0x0081);
            bytes.Add(0x0001);
            bytes.Add(0x0084);
            bytes.AddRange(Encoding.Unicode.GetBytes(text));
            if (extended)
            {
                bytes.Add(0x0003);
            }
            return bytes.ToArray();
        }

        public string GetObjectXMLString<T>(T toSerialize)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            StringBuilder result = new StringBuilder();

            using (TextWriter text = new StringWriter(result))
            {
                xmlSerializer.Serialize(text, toSerialize);
            }

            return result.ToString();

        }

        public XmlDocument GetObjectXMLDocument<T>(T toSerialize)
        {
            XmlDocument document = new XmlDocument();
            document.LoadXml(GetObjectXMLString(toSerialize));
            return document;
        }

        protected static byte[] ReadBytesRube(NetworkStream stream)
        {
            List<byte> data = new List<byte>();
            while (stream.DataAvailable)
            {
                byte[] byteDataArray = new byte[1024];
                int count = stream.Read(byteDataArray, 0, 1024);
                byte[] finalBytes = new byte[count];
                Array.Copy(byteDataArray, finalBytes, count);
                data.AddRange(finalBytes);
            }
            return data.ToArray();
        }

        protected virtual void MessageAvailable(Client client)
        {
            IMessageBase<object> message = null;
            byte[] bytes = new byte[3];
            if (client.ClientStream.Read(bytes, 0, 3) < 3)
            {
                message = new NetworkByteMessage(client, bytes) { Unknown = true };
            }
            else
            {
                if ((bytes[0] == 0x0081 && bytes[2] == 0x0084))
                {
                    switch ((MessageDataType)bytes[1])
                    {
                        case MessageDataType.XMLDocument:
                            byte[] rawBytes = ReadBytesRube(client.ClientStream);
                            List<byte> messageBytes = new List<byte>();
                            string messageText = null;
                            if (rawBytes.Length > 1)
                            {
                                for (int ia = 0; ia < rawBytes.Length; ia++)
                                {
                                    if (rawBytes[ia] != 0x0003)
                                    {
                                        messageBytes.Add(rawBytes[ia]);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                messageText = Encoding.Unicode.GetString(messageBytes.ToArray());
                            }
                            message = new NetworkXMLMessage(client, messageText);
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    message = new NetworkByteMessage(client, ReadBytesRube(client.ClientStream)) { Unknown = true };
                }
            }
            MessageArrived?.Invoke(message);
        }

        public virtual void Send(Client client, string message)
        {
            XElement node = new XElement("String", message);
            Send(client, BuildXMLQuerry(node.ToString()));
        }

        public void Send(Client client, byte[] message)
        {
            if (client != null)
            {
                ((Stream)client.ClientStream).Write(message,0,message.Length);
            }
            else
            {
                throw new InvalidOperationException("Connection not installed");
            }
        }

        protected bool SocketConnected(Socket s)
        {
            //https://stackoverflow.com/questions/2661764/how-to-check-if-a-socket-is-connected-disconnected-in-c
            bool part1 = s.Poll(1000, SelectMode.SelectRead);
            bool part2 = (s.Available == 0);
            if (part1 && part2)
                return false;
            else
                return true;
        }
    }
}
