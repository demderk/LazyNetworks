using System;
using System.Collections.Generic;
using System.Text;

namespace AdvancedTCP
{
    public sealed class MessageQuestion
    {
        public MessageQuestion(Client client)
        {
            Client = client;
        }

        private Client Client { get; }

        public int QuestionCode { get; set; }

        public bool IsAnswer { get; set; }

        public void Answer(byte[] message)
        {
            byte[] questionCodeBytes = BitConverter.GetBytes(QuestionCode);
            List<byte> messageBytes = new List<byte>()
                {
                    0x0081,
                    0x0002,
                    0x0084,
                    0x0081
                };
            messageBytes.AddRange(questionCodeBytes);
            messageBytes.Add(1);
            messageBytes.Add(0x0084);
            messageBytes.AddRange(message);
            Client.Send(messageBytes.ToArray());
        }

        public void Answer(string message)
        {
            Answer(AdvancedTCPExtentions.BuildXMLQuerry(message));
        }
    }
}