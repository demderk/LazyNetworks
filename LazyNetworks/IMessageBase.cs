﻿namespace AdvancedTCP
{
    public interface IMessageBase<out T>
    {

        T MessageBody { get; }

        Client RemoteClient { get; }

    }

}
