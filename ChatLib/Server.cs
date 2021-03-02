using System;
using System.Collections.Generic;
using System.Text;

namespace ChatLib
{
    
    public class Server
    {
        public List<Room> Rooms { get; set; }


        public Server()
        {
            Rooms = new List<Room>();
        }

        public Room GetRoom(Guid roomId)
        {
            foreach(Room room in Rooms)
            {
                if (room.ID == roomId)
                    return room;
            }
            return null;
        }

        public Room CreateRoom(string name)
        {
            Room room = new Room(name, new List<User>());

            Rooms.Add(room);

            return room;
        }

        public bool DeleteRoom(Guid roomId)
        {
            Room room = GetRoom(roomId);
            if (room != null)
            {
                Rooms.Remove(room);
                return true;
            }
            return false;
        }

        public void JoinRoom(Message message)
        {
            Room room = GetRoom(message.Room.ID);
            if(room != null)
            {
                if (!room.UsersInRoom.Contains(message.User))
                    room.UsersInRoom.Add(message.User);
            }
        }

        public void QuitRoom(Message message)
        {
            Room room = GetRoom(message.Room.ID);
            if (room != null)
            {
                if (!room.UsersInRoom.Contains(message.User))
                    room.UsersInRoom.Remove(message.User);
            }
        }

        


    }
}
