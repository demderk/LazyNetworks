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
using LazyNetworks;

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
            IMessageBase<object> message = GetByteMessage(client, ReadBytesRube(client.ClientStream));
            if (message != null)
            {
                MessageArrived?.Invoke(message);
            }
        }

        private IMessageBase<object> GetByteMessage(Client client, byte[] allBytes)
        {
            // TODO: Do 8 byte - per message fixed size, not 3-6
            if (allBytes.Length < 3)
            {
                return new NetworkByteMessage(client, allBytes) { Unknown = true };
            }
            else
            {
                byte[] messageBytes = new byte[allBytes.Length - 3];
                Array.Copy(allBytes, 3, messageBytes, 0, allBytes.Length - 3);
                if (((EnvironmentalVariables)allBytes[0] == EnvironmentalVariables.StartSymbol &&
                    (EnvironmentalVariables)allBytes[2] == EnvironmentalVariables.EndSymbol))
                {
                    switch ((MessageDataType)allBytes[1])
                    {
                        case MessageDataType.XMLDocument:
                            List<byte> messageBytesXml = new List<byte>();
                            string messageText = null;
                            if (messageBytes.Length > 1)
                            {
                                for (int ia = 0; ia < messageBytes.Length; ia++)
                                {
                                    if (messageBytes[ia] != 0x0003)
                                    {
                                        messageBytesXml.Add(messageBytes[ia]);
                                    }
                                    else
                                    {
                                        //TODO: multiple data
                                        break;
                                    }
                                }
                                messageText = Encoding.Unicode.GetString(messageBytesXml.ToArray());
                            }
                            return new NetworkXMLMessage(client, messageText);
                        case MessageDataType.Question:
                            List<byte> messageBytesQs = new List<byte>();
                            if ((EnvironmentalVariables)messageBytes[0] == EnvironmentalVariables.StartSymbol &&
                                (EnvironmentalVariables)messageBytes[6] == EnvironmentalVariables.EndSymbol)
                            {
                                byte[] questionBytes = new byte[4];
                                Array.Copy(messageBytes, 1, questionBytes, 0, 4);
                                int questionCode = BitConverter.ToInt32(questionBytes, 0);
                                bool questionAnswer = BitConverter.ToBoolean(messageBytes, 5);
                                byte[] clearMessageQs = new byte[messageBytes.Length - 7];
                                Array.Copy(messageBytes, 7, clearMessageQs, 0, messageBytes.Length - 7);
                                messageBytesQs.AddRange(clearMessageQs);
                                if (questionAnswer)
                                {
                                    lock (client.QuestionsArrayLock)
                                    {
                                        if (client.QuestionsArray.ContainsKey(questionCode))
                                        {
                                            client.QuestionsArray[questionCode] = GetByteMessage(client, clearMessageQs);
                                        }
                                        else
                                        {
                                            return null;
                                        }
                                    }
                                }
                                else
                                {
                                    IMessageBase<object> child = GetByteMessage(client, clearMessageQs);
                                    child.Question = new MessageQuestion(client) { IsAnswer = false, QuestionCode = questionCode };
                                    return child;
                                }
                            }
                            else
                            {
                                return null;
                            }

                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    return new NetworkByteMessage(client, allBytes) { Unknown = true };
                }
            }
            return null;
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
