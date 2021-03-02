using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ChatLib
{
    public class ServerService
    {
        public struct ClientFull
        {
            public Client Client;
            public Thread Thread;
        }

        public ChatService service = new ChatService();
        public Server Server = new Server();
        public bool listenerActive = false;
        public Room activeRoom;

        private ConcurrentDictionary<Guid, ClientFull> list = new ConcurrentDictionary<Guid, ClientFull>();
        public List<Message> publicMessages = new List<Message>();

        private Thread serverThread = null;

        public event Action<string, bool> LogWrite;
        public event Action<User> ShowConnect;
        public event Action<bool> Active;

        public ServerService()
        {

            service.DisconnectClient += DisconnectClient;
            service.HandleMessage += HandleMessage;
            service.Heartbeat += Heartbeat;
            service.JoinRoom += Server.JoinRoom;
            service.QuitRoom += Server.QuitRoom;
            service.ToggleConnect += ToggleConnect;
        }

        public void StartServer(IPAddress localAddr, int port)
        {
            listenerActive = !listenerActive;
            ToggleStateUI(listenerActive);

            if (listenerActive && serverThread == null)
            {

                serverThread = new Thread(() => Listener(localAddr, port))
                {
                    IsBackground = true
                };
                serverThread.Start();
            }
        }
        public void DisconnectAll()
        {
            foreach (KeyValuePair<Guid, ClientFull> client in list)
            {
                client.Value.Client.Instance.Close();
                client.Value.Thread.Join();
            }

        }

        private void Listener(IPAddress localAddr, int port)
        {
            TcpListener listener = null;
            try
            {
                listener = new TcpListener(localAddr, port);
                listener.Start();

                while (listenerActive)
                {
                    if (listener.Pending())
                    {
                        try
                        {
                            TcpClient Tcpclient = listener.AcceptTcpClient();

                            Client client = new Client()
                            {
                                Instance = Tcpclient,
                                Stream = Tcpclient.GetStream(),
                                User = new User(String.Empty),
                                Timer = new System.Timers.Timer(2000),
                                MessageQueue = new List<Message>(),
                                Handle = new EventWaitHandle(false, EventResetMode.AutoReset)
                            };

                            Thread th = new Thread(() => Connection(client))
                            {
                                IsBackground = true
                            };
                            th.Start();


                            ClientFull client1 = new ClientFull()
                            {
                                Client = client,
                                Thread = th
                            };

                            list.TryAdd(client.User.ID, client1);
                        }
                        catch (Exception e)
                        {
                            Log(e.Message);
                        }
                    }
                    else
                    {
                        Thread.Sleep(500);
                    }
                }
            }
            catch (Exception e)
            {
                Log(e.Message);
            }
            finally
            {
                if (listener != null)
                    listener.Server.Close();

            }
        }

        private void Connection(Client client)
        {

            client.Timer.Start();
            client.Timer.Elapsed += (sender, e) => Heartbeat(client);

            
            //TaskSend(msg, obj.id);

            while (client.Instance.Connected)
            {
                try
                {
                    service.Read(client, true);
                    service.Send(client);
                    Thread.Sleep(10);
                }
                catch (Exception e)
                {
                    Log(e.Message);
                }
            }

            DisconnectClient(client);
        }

        private void HandleMessage(Message message)
        {
            if (message.ToID == Room.Server.ID)
            {
                foreach (KeyValuePair<Guid, ClientFull> client in list)
                {
                    if (message.Room.UsersInRoom.Contains(client.Value.Client.User))
                    {
                        client.Value.Client.MessageQueue.Add(message);
                    }

                }
                message.Content = string.Format("[{0}] {1} : {2}", DateTime.Now.ToString("HH:mm"), message.User.Name, message.Content);
                Log(message);
                publicMessages.Add(message);
            }
            else
                list.FirstOrDefault(kvp => kvp.Value.Client.User.ID == message.ToID).Value.Client.MessageQueue.Add(message);

        }

        private void DisconnectClient(Client client)
        {

            client.Instance.Close();
            string msg = string.Format("[/ Client {0} deconnected /]", client.User.Name);
            Log(msg);
            ToggleConnect(client.User);
            //TaskSend(msg, obj.id);
            list.TryRemove(client.User.ID, out ClientFull tmp);
        }



        private void Heartbeat(Client client)
        {
            if (client.Instance.Connected)
            {
                if (client.Stream.CanWrite)
                {
                    try
                    {
                        Ping message = new Ping(client.User.ID, Server.Rooms);
                        byte[] data = Convert.ToByteArray(message);
                        client.Stream.Write(data, 0, data.Length);
                        client.Stream.Flush();
                        client.Timer.Interval = 2000;
                    }
                    catch (Exception e)
                    {
                        DisconnectClient(client);
                        Log(e.Message);
                        Log(string.Format("[/ Client {0} timed out ! /]", client.User.Name));
                    }
                }
            }
        }


        public void Log(string content)
        {
            LogWrite(content, false);
        }

        public void Log(Message message)
        {
            Room room = Server.GetRoom(message.Room.ID);

            if (room != null)
            {
                room.MessageHistory.Add(message);
                if(activeRoom != null && room.ID == activeRoom.ID)
                    LogWrite(message.Content, false);
            }
        }

        public void ToggleConnect(User user)
        {
            ShowConnect(user);
        }

        public void ToggleStateUI(bool status)
        {
            Active(status);
        }
    }
}
