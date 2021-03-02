using System;
using System.Collections.Generic;
using System.Text;

namespace ChatLib
{
    [Serializable]
    public class Message
    {
        public Message(MessageType type, Guid toId, Guid fromId, string content, User user, Room room)
        {
            Type = type;
            ToID = toId;
            FromID = fromId;
            Content = content;
            User = user;
            Room = room;
        }

        public Message()
        {

        }

        public MessageType Type { get; set; }
        public Guid ToID { get; set; }
        public Guid FromID { get; set; }
        public string Content { get; set; }
        public User User { get; set; }
        public Room Room { get; set; }  
    }

    public enum MessageType
    {
        Message,
        Connect,
        Disconnect,
        Ping,
        JoinRoom,
        QuitRoom
    }
}
