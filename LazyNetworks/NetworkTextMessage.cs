using System;
using AdvancedTCP;

namespace LazyNetworks
{
    [Obsolete("Not Implemented")]
    public class NetworkTextMessage : IMessageBase<string>
    {
        public NetworkTextMessage(Client client, string messageBody)
        {
            throw new NotImplementedException();
            RemoteClient = client;
            MessageBody = messageBody;
        }

        public string MessageBody { get; } = "";

        public Client RemoteClient { get; }

        public MessageQuestion Question { get; set; }
    }
}
