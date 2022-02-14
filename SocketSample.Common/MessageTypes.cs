using System;
using System.Collections.Generic;
using System.Text;

namespace SocketSample.Common
{
    public enum MessageTypes : byte
    {
        Welcome = 100,
        Bye = 110,
        GoodBye = 111,

        ClientList = 10,
        NewClient = 11,
        LeftClient = 12,

        RoomList = 21,

        MessageToAll = 30,
        MessageToClient = 31,
        MessageToRoom = 32,

        SetUsername = 40,
        ConfirmChangedUsername = 41,
        NotifyChangedUsernameOthers = 42
    }
}
