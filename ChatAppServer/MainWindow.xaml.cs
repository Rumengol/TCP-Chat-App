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
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Collections.Concurrent;
using ChatLib;
using System.Collections.ObjectModel;

namespace ChatAppServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region variables
        private bool exit = false;

        private Thread disconnect = null;

        private ServerService service = new ServerService();
        private Server server;

        private ObservableCollection<Room> rooms;
        private ObservableCollection<User> users;
        

        #endregion 
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            server = service.Server;
            service.LogWrite += LogWrite;
            service.ShowConnect += ToggleConnectList;
            service.Active += ToggleActive;

            rooms = new ObservableCollection<Room>();
            users = new ObservableCollection<User>();

            connections.ItemsSource = users;
            RoomList.ItemsSource = rooms;
        }

        private void ToggleActive(bool status)
        {
            Dispatcher.Invoke(new Action<bool>(Active), status);
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

        private void Active(bool status)
        {
            if (!exit)
            {
                if (status)
                {
                    startButton.Content = "Stop";
                    LogWrite("[/ Server started /]");
                }
                else
                {
                    startButton.Content = "Start";
                    LogWrite("[/ Server stopped /]");
                }
            }
        }
        

        private void ToggleConnectList(User user)
        {
            Dispatcher.Invoke(new Action<User>(DoToggleConnectList), user);
        }

        private void DoToggleConnectList(User user)
        {
            if (!users.Contains(user))
                users.Add(user);
            else
            {
                users.Remove(user);
            }
            /*string lblName = string.Format("lblClient_{0}", user.Name);
            Label lbl = FindChild<Label>(connections, lblName);
            if (lbl == null)
            {
                string msg = string.Format("[/ Client {0} connected /]", user.Name);
                LogWrite(msg);

                Label lblCon = new Label()
                {
                    Content = string.Format("Client {0}", user.Name),
                    Cursor = Cursors.Arrow,
                    Name = lblName
                };

                connections.Children.Add(lblCon);
            }
            else
            {
                connections.Children.Remove(lbl);
            }*/
        }

     /*   if (obj.client.Connected)
            {
                try
                {
                    bytes = obj.stream.EndRead(result);
                }
                catch (Exception e)
                {
                    
                }
            }

            if (bytes > 0)
            {
                obj.data.Append(Encoding.UTF8.GetString(obj.buffer, 0, bytes));
                try
                {
                    if (obj.stream.DataAvailable)
                        obj.stream.BeginRead(obj.buffer, 0, obj.buffer.Length, new AsyncCallback(Read), 0);
                    else
                    {
                        obj.log = string.Format("<- Client {0} -> {1}", obj.id, obj.data);
obj.data.Clear();
                        obj.handle.Set();
                        return;
                    }
                }
                catch (Exception e)
                {
                    obj.data.Clear();
                    obj.handle.Set();
                    obj.log = string.Format("[/ {0} /]", e.Message);
                    return;
                }
            }
            else
            {
                obj.client.Close();
                obj.handle.Set();
                obj.log = string.Empty;
                return;
            }
        }*/

        
        private void ToggleServer(object sender, RoutedEventArgs e)
        {

            bool localAddrResult = IPAddress.TryParse(addressBox.Text, out IPAddress localAddr);

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
                service.StartServer(localAddr, port);
            }
        }

        

        private void disconnectButton_Click(object sender, RoutedEventArgs e)
        {
            if(disconnect == null || !disconnect.IsAlive)
            {
                disconnect = new Thread(() => service.DisconnectAll())
                {
                    IsBackground = true
                };
                disconnect.Start();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            exit = true;
            service.listenerActive = false;
            if(disconnect == null || !disconnect.IsAlive)
            {
                disconnect = new Thread(() => service.DisconnectAll())
                {
                    IsBackground = true
                };
                disconnect.Start();
            }
        }

        private void AddRoom_Click(object sender, RoutedEventArgs e)
        {
            RoomCreationDialog roomDialog = new RoomCreationDialog();
            if(roomDialog.ShowDialog() == true)
            {
                Room room = server.CreateRoom(roomDialog.RoomName);

                rooms.Add(room);
            }
        }

        private void DeleteRoom_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            Room roomToDelete = server.GetRoom(Guid.Parse(btn.Tag.ToString()));
            if (server.DeleteRoom(roomToDelete.ID))
            {
                rooms.Remove(roomToDelete);
            }
        }

        private void RoomName_MouseDown(object sender, MouseButtonEventArgs e)
        {
                Room room = server.GetRoom(Guid.Parse(((TextBlock)sender).Tag.ToString()));

                if(room != null)
                {
                    histoLabel.Content = string.Format("{0} :", room.Name);
                    histoBlock.Text = string.Empty;
                    users.Clear();
                    room.UsersInRoom.ForEach(u => users.Add(u));

                    service.activeRoom = room;

                    foreach (Message log in room.MessageHistory)
                        LogWrite(log.Content);
                }
        }

        public static T FindChild<T>(DependencyObject parent, string childName)
   where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }

        private void Username_MouseUp(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
