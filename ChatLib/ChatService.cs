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
    public class ChatService
    {
        #region Server Action
        public event Action<Client> Heartbeat;
        public event Action<Client> DisconnectClient;
        public event Action<Message> JoinRoom;
        public event Action<Message> QuitRoom;
        public event Action<Message> HandleMessage;
        public event Action<User> ToggleConnect;
        #endregion

        #region Client Action
        public event Action Disconnect;
        public event Action<Ping> HandlePing;
        public event Action<Message> LogMessage;
        public event Action<string> Log;
        #endregion

        public void Read(Client client, bool fromServer)
        {
            if (client.Instance.Connected)
            {
                if (client.Stream.DataAvailable)
                {
                    Message message = Convert.ConvertMessage(client.Stream);
                    switch (message.Type)
                    {
                        case MessageType.Connect:
                            if (fromServer)
                            {
                                client.User = message.User;
                                ToggleConnect(client.User);
                                Heartbeat(client);
                            }
                            break;

                        case MessageType.Disconnect:
                            if (fromServer)
                            {
                                DisconnectClient(client);
                            }
                            else
                            {
                                Disconnect();
                            }
                            break;

                        case MessageType.Ping:
                            if(!fromServer)
                            {
                                if(message is Ping)
                                    HandlePing(message as Ping);
                            }
                            break;

                        case MessageType.JoinRoom:
                            JoinRoom(message);
                            break;

                        case MessageType.QuitRoom:
                            QuitRoom(message);
                            break;

                        case MessageType.Message:
                            if (fromServer)
                            {
                                HandleMessage(message);
                            }
                            else
                            {
                                LogMessage(message);
                                //Ajouter les messages à la liste, mettre à jour
                            }
                            break;

                        default:
                            break;
                    }
                }
            }
        }


        public void Send(Client client)
        {
            if (client.Stream.CanWrite)
            {
                if (client.MessageQueue.Count > 0)
                {
                    byte[] data = Convert.ToByteArray(client.MessageQueue[0]);
                    try
                    {
                        client.Stream.Write(data, 0, data.Length);
                        client.Stream.Flush();
                    }
                    catch
                    {

                    }

                    if (client.MessageQueue[0].Type == MessageType.Disconnect) DisconnectClient(client);
                    client.MessageQueue.RemoveAt(0);
                }
            }
        }
    }
}
