using System;
using System.Collections.Generic;
using System.Text;
using LazyNetworks;

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
                    (byte)EnvironmentalVariables.StartSymbol,
                    (byte)MessageDataType.Question,
                    (byte)EnvironmentalVariables.EndSymbol,
                    (byte)EnvironmentalVariables.StartSymbol
                };
            messageBytes.AddRange(questionCodeBytes);
            messageBytes.Add(1); // IsAnswer (IsAnswer ? 1 : 0)
            messageBytes.Add((byte)EnvironmentalVariables.EndSymbol);
            messageBytes.AddRange(message);
            Client.Send(messageBytes.ToArray());
        }

        public void Answer(string message)
        {
            Answer(AdvancedTCPExtentions.BuildXMLQuerry(message));
        }
    }
}