using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Configuration;
using ChatLib;
using System.Collections.ObjectModel;

namespace ChatAppClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region variables
        private bool connected = false;
        private bool exit = false;

        private ClientService service = new ClientService();


        private ObservableCollection<Room> rooms = new ObservableCollection<Room>();
        private ObservableCollection<User> users = new ObservableCollection<User>();

        #endregion 
        public MainWindow()
        {
            InitializeComponent();
            service.currentUser = new User(usernameTextBox.Text);

            service.LogWrite += LogWrite;
            service.UpdateUI += Service_UpdateUI;

            DataContext = this;

            RoomList.ItemsSource = rooms;
            UserList.ItemsSource = users;
        }

        private void Service_UpdateUI()
        {
            Dispatcher.Invoke(new Action(UpdateUI));
        }

        private void UpdateUI()
        {
            rooms.Clear();
            users.Clear();
            service.Rooms.ForEach(r => rooms.Add(r));
            service.activeRoom.UsersInRoom.ForEach(u => users.Add(u));
        }

        private void LogWrite(string log = null, bool good = false)
        {
            if (!exit && good)
            {
                histoBlock.Text += string.Format("{0} : {1} \r\n", DateTime.Now.ToString("HH:mm"), log);
            }
            else
            {
                Dispatcher.Invoke(new Action<string, bool>(LogWrite), new object[] { log, true });
            }
        }

        private void Connected(bool status)
        {
            if (!exit)
            {
                if (status)
                {
                    connected = true;
                    connectButton.Content = "Disconnect";
                    LogWrite("[/ Connected /]");
                }
                else
                {
                    connected = false;
                    connectButton.Content = "Connect";
                    LogWrite("[/ Disconnected /]");
                }
            }
        }

        /*private void Read(IAsyncResult result)
        {
            int bytes = 0;

            if (obj.client.Connected)
            {
                try
                {
                    bytes = obj.stream.EndRead(result);
                }
                catch (Exception e)
                {
                    Dispatcher.Invoke(new Action<string>(LogWrite), string.Format("[/ {0} /]", e));
                }
            }

            if (bytes > 0)
            {
                obj.data.Append(Encoding.UTF8.GetString(obj.buffer, 0, bytes));
                try
                {
                    if (obj.stream.DataAvailable)C:\Users\Rumengol\source\repos\ChatApp\ChatAppServer\ViewModels\
                        obj.stream.BeginRead(obj.buffer, 0, obj.buffer.Length, new AsyncCallback(Read), 0);
                    else
                    {
                        Dispatcher.Invoke(new Action<string>(LogWrite), obj.data.ToString());
                        obj.data.Clear();
                        obj.handle.Set();
                    }
                }
                catch (Exception e)
                {
                    obj.data.Clear();
                    Dispatcher.Invoke(new Action<string>(LogWrite), e.Message);
                    obj.handle.Set();
                }
            }
            else
            {
                obj.client.Close();
                obj.handle.Set();
            }
        }

        private void TaskSend(string msg)
        {
            if (send == null || send.IsCompleted)
            {
                send = Task.Factory.StartNew(() => Send(msg));
                Dispatcher.Invoke(new Action<string>(LogWrite), string.Format("<- YOU -> {0}", msg));
            }
            else
                send.ContinueWith(antecedent => Send(msg));
        }

        private void Send(string msg)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(msg);
            if (obj.client.Connected)
            {
                try
                {
                    obj.stream.BeginWrite(buffer, 0, buffer.Length, new AsyncCallback(Write), null);
                }
                catch (Exception e)
                {
                    Dispatcher.Invoke(new Action<string>(LogWrite), e.Message);
                }
            }
        }

        private void Write(IAsyncResult result)
        {
            if (obj.client.Connected)
            {
                try
                {
                    obj.stream.EndWrite(result);
                }
                catch (Exception e)
                {
                    Dispatcher.Invoke(new Action<string>(LogWrite), e.Message);
                }
            }
        }


        private void Connection(IPAddress localAddr, int port)
        {
                        try
                        {
                            obj = new Client();
                            obj.client = new TcpClient();
                            obj.client.Connect(localAddr, port);
                            obj.stream = obj.client.GetStream();
                            obj.buffer = new byte[obj.client.ReceiveBufferSize];
                            obj.data = new StringBuilder();
                            obj.handle = new EventWaitHandle(false, EventResetMode.AutoReset);

                            Dispatcher.Invoke(new Action<bool>(Connected), true);
                            while (obj.client.Connected)
                            {
                                try
                                {
                                    obj.stream.BeginRead(obj.buffer, 0, obj.buffer.Length, new AsyncCallback(Read), obj);
                                    obj.handle.WaitOne();
                                }
                                catch (Exception e)
                                {
                                    Dispatcher.Invoke(new Action<string>(LogWrite), e.Message);
                                }
                            }
                            obj.client.Close();
                            Dispatcher.Invoke(new Action<bool>(Connected), false);
                        }
                        catch (Exception e)
                        {
                            Dispatcher.Invoke(new Action<string>(LogWrite), e.Message);
                        }

        }*/

        private void ConnectToServer(object sender, RoutedEventArgs e)
        {
            if (!connected)
            {
                try
                {
                    bool localAddrResult = IPAddress.TryParse(addressTextBox.Text, out IPAddress localAddr);

                    if (!localAddrResult)
                        LogWrite("[/ Address is not valid /]");

                    bool portResult = int.TryParse(portTextBox.Text, out int port);
                    if (!portResult)
                        LogWrite("[/ Port number is not valid /]");
                    else if (port < 0 || port > 65535)
                    {
                        portResult = false;
                        LogWrite("[/ Port number is out of range /]");
                    }
                    if (localAddrResult && portResult)
                    {
                        service.ConnectToServer(localAddr, port, service.currentUser);
                    }

                    Connected(true);
                }
                catch (Exception ex)
                {
                    LogWrite(ex.Message);
                }
            }
            else
            {
                Connected(false);
            }
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (connected)
            {
                exit = true;
            }
        }

        private void sendBox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                e.Handled = true;
                if(sendBox.Text.Length > 0)
                {
                    string msg = sendBox.Text;
                    sendBox.Clear();
                    sendBox.Focus();

                    service.PrepareMessage(msg, service.activeRoom.ID);
                }
            }
        }

        private void RoomName_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Room room = rooms.FirstOrDefault(r => r.ID == (Guid.Parse(((TextBlock)sender).Tag.ToString())));

            if (room != null && room.ID != service.activeRoom.ID)
            {
                histoLabel.Content = string.Format("{0} :", room.Name);
                histoBlock.Text = string.Empty;

                if (service.activeRoom.Name != null)
                    service.QuitRoom(service.activeRoom.ID);

                service.activeRoom = room;
                service.JoinRoom(room.ID);

                LogWrite(service.activeRoom.Name);

                foreach (Message log in room.MessageHistory)
                    LogWrite(log.Content);
            }

            UpdateUI();
        }

        private void Username_MouseUp(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
