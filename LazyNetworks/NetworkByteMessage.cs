using System;
using System.Collections.Generic;
using System.Text;

namespace AdvancedTCP
{
    class NetworkByteMessage : IMessageBase<byte[]>
    {
        public NetworkByteMessage(Client remoteClient, byte[] messageBody)
        {
            MessageBody = messageBody;
            RemoteClient = remoteClient;
        }

        public byte[] MessageBody { get; }

        public Client RemoteClient { get; }

        public bool Unknown { get; set; }
    }
}
