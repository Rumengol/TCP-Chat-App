using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Timers;

namespace ChatLib
{
    public class Client
    {
        public Client(TcpClient client, NetworkStream stream, User user, System.Timers.Timer timer, List<Message> messageQueue)
        {
            Instance = client;
            Stream = stream;
            User = user;
            Timer = timer;
            MessageQueue = messageQueue;
        }

        public Client()
        {

        }

        public TcpClient Instance { get; set; }
        public NetworkStream Stream { get; set; }
        public User User { get; set; }
        public System.Timers.Timer Timer { get; set; }
        public List<Message> MessageQueue { get; set; }
        public EventWaitHandle Handle { get; set; }
    }
}
