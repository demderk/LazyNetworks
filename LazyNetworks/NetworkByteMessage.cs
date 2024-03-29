﻿using System;
using System.Collections.Generic;
using System.Text;

namespace AdvancedTCP
{
    public sealed class NetworkByteMessage : IMessageBase<byte[]>
    {
        public NetworkByteMessage(Client remoteClient, byte[] messageBody)
        {
            MessageBody = messageBody;
            RemoteClient = remoteClient;
        }

        public byte[] MessageBody { get; }

        public Client RemoteClient { get; }

        public bool Unknown { get; set; }

        public MessageQuestion Question { get; set; }
    }
}
