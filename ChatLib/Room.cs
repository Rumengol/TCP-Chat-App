using System;
using System.Collections.Generic;
using System.Text;

namespace ChatLib
{
    [Serializable]
    public class Room : User
    {
        public static Room Server = new Room("SERVER", null)
        {
            ID = Guid.Empty
        };

        public Room(string name, List<User> usersinroom = null) : base(name)
        {
            ID = Guid.NewGuid();
            Name = name;
            if (usersinroom != null)
                UsersInRoom = usersinroom;
            else
                UsersInRoom = new List<User>();
            MessageHistory = new List<Message>();
        }
        public List<User> UsersInRoom { get; set; }
        public List<Message> MessageHistory { get; set; }
    }
}
