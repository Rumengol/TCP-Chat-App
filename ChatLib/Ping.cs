using System;
using System.Collections.Generic;
using System.Text;

namespace ChatLib
{
    [Serializable]
    public class Ping : Message
    {
        public Ping(Guid toId, List<Room> rooms)
        {
            Type = MessageType.Ping;
            ToID = toId;
            FromID = Room.Server.ID;
            AllRooms = rooms;
            User = Room.Server;
            Content = string.Empty;
        }

        public List<Room> AllRooms { get; set; }
    }
}
