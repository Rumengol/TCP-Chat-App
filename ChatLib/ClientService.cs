using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Linq;

namespace ChatLib
{
    public class ClientService
    {
        private Thread clientThread = null;

        public User currentUser;
        private Client server;

        private ChatService service = new ChatService();

        public List<Room> Rooms = new List<Room>();
        public Room activeRoom = new Room(null);

        public event Action UpdateUI;
        public event Action<string, bool> LogWrite;

        public ClientService()
        {
            service.Disconnect += Disconnect;
            service.HandlePing += HandlePing;
            service.LogMessage += LogMessage;
            service.Log += Log;
        }

        public void ConnectToServer(IPAddress address, int port, User user)
        {
            try
            {
                currentUser = user;

                TcpClient TcpClient = new TcpClient();

                TcpClient.Connect(address, port);

                server = new Client()
                {
                    Instance = TcpClient,
                    Stream = TcpClient.GetStream(),
                    Timer = new System.Timers.Timer(5000),
                    Handle = new EventWaitHandle(false, EventResetMode.AutoReset),
                    MessageQueue = new List<Message>(),
                    User = null
                };

                clientThread = new Thread(() => HandleServer())
                {
                    IsBackground = true
                };
                clientThread.Start();
            }
            catch (Exception e)
            {
                Log(e.Message);
            }
        }

        public void PrepareMessage(string content, Guid roomId)
        {
            Room room = Rooms.FirstOrDefault(r => r.ID == roomId);
            Message message = new Message(MessageType.Message, Room.Server.ID, currentUser.ID, content, currentUser, room);

            server.MessageQueue.Add(message);

            LogMessage(message);
        }

        public void JoinRoom(Guid roomId)
        {
            Room room = Rooms.FirstOrDefault(r => r.ID == roomId);
            Message message = new Message(MessageType.JoinRoom, Room.Server.ID, currentUser.ID, string.Empty, currentUser, room);

            server.MessageQueue.Add(message);
        }

        public void QuitRoom(Guid roomId)
        {
            Room room = Rooms.FirstOrDefault(r => r.ID == roomId);
            Message message = new Message(MessageType.QuitRoom, Room.Server.ID, currentUser.ID, string.Empty, currentUser, room);

            server.MessageQueue.Add(message);
        }

        private void HandleServer()
        {
            Message message = new Message(MessageType.Connect, Room.Server.ID, currentUser.ID, "", currentUser, null);

            byte[] data = Convert.ToByteArray(message);
            server.Stream.Write(data, 0, data.Length);
            server.Stream.Flush();

            server.Timer.Elapsed += (s, e) => HeartbeatExpire();
            server.Timer.Start();

            while (server.Instance.Connected)
            {
                try
                {
                    service.Read(server, false);
                    service.Send(server);
                    Thread.Sleep(10);
                }
                catch (Exception e)
                {
                    Log(e.Message);
                }
            }
            server.Instance.Close();
        }


        public void Log(string content)
        {
            LogWrite(content, false);
        }

        public void LogMessage(Message message)
        {
            Room room = Rooms.FirstOrDefault(r => r.ID == message.Room.ID);


            if (room != null)
            {
                room.MessageHistory.Add(message);

                if(room.ID == activeRoom.ID)
                {
                    Log(message.Content);
                }
            }
        }

        private void HeartbeatExpire()
        {
            Log("Fin du timing");
            server.Instance.Close();
            clientThread.Join();
        }

        private void Disconnect()
        {
            server.Instance.Close();
            clientThread.Join();
        }

        private void HandlePing(Ping ping)
        {
            Rooms = ping.AllRooms;
            server.Timer.Interval = 5000;
            UpdateUI();
        }

        
    }
}
