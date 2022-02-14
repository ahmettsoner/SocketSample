using Newtonsoft.Json;
using SocketSample.Common;
using SocketSample.Socket;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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

namespace SocketSample.WPF.Server
{

    public class MainWindowViewModel : BaseObject
    {
        private ObservableCollection<Event> _Events = new ObservableCollection<Event>();
        public ObservableCollection<Event> Events { get { return _Events; } set { _Events = value; OnPropertyChanged(nameof(Events)); } }

        private ObservableCollection<ClientInfo> _Clients = new ObservableCollection<ClientInfo>();
        public ObservableCollection<ClientInfo> Clients { get { return _Clients; } set { _Clients = value; OnPropertyChanged(nameof(Clients)); } }

        private ClientInfo _SelectedClient;
        public ClientInfo SelectedClient { get { return _SelectedClient; } set { _SelectedClient = value; OnPropertyChanged(nameof(SelectedClient)); } }



        internal void Clear()
        {
            this.Clients.Clear();
            this.Events.Clear();
            this.SelectedClient = null;
        }
    }


    public partial class MainWindow : Window
    {
        public MainWindowViewModel ViewModel { get; set; } = new MainWindowViewModel();
        public ServerSocket Server { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = ViewModel;

            this.Server = new ServerSocket(IPAddress.Any, 3333);

            this.Server.OnLogReceived += (logMessage) =>
            {
                this.AddEvent(logMessage);
            };
            this.Server.OnClientConnected += (client) =>
            {
                this.AddClient(client);
            };
            this.Server.OnClientDisconnected += (client) =>
            {
                this.RemoveClient(client);
            };
            this.Server.OnReceived += (sourceClient, data) =>
            {
                switch (data.Type)
                {
                    case MessageTypes.Welcome:
                        break;
                    case MessageTypes.Bye:
                        this.Server.WelcomeBYEMessage(sourceClient);
                        break;
                    case MessageTypes.GoodBye:
                        break;
                    case MessageTypes.ClientList:
                        break;
                    case MessageTypes.NewClient:
                        break;
                    case MessageTypes.LeftClient:
                        break;
                    case MessageTypes.RoomList:
                        break;
                    case MessageTypes.MessageToAll:
                        this.Server.WelcomeGlobalChatMessage(sourceClient, data.Data);
                        break;
                    case MessageTypes.MessageToClient:
                        string[] messageHeaderParts = data.Data.Split(":TO-");
                        if (messageHeaderParts.Length == 2)
                        {
                            Guid targetClientID = Guid.Parse(messageHeaderParts[0]);
                            var targetClient = this.Server.GetClient(targetClientID);
                            this.Server.WelcomeChatMessageToClient(sourceClient, targetClient, messageHeaderParts[1]);
                        }
                        break;
                    case MessageTypes.MessageToRoom:
                        break;
                    case MessageTypes.SetUsername:
                        this.Server.WelcomeSetUsernameMessage(sourceClient, data.Data);
                        break;
                    case MessageTypes.ConfirmChangedUsername:
                        break;
                    case MessageTypes.NotifyChangedUsernameOthers:
                        break;
                    default:
                        break;
                }
            };
        }


        private void btnStartServer_Click(object sender, RoutedEventArgs e)
        {
            this.Server.Initialize();
            this.Server.StartServer();
        }
        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            SendMessageAction();
        }
        private void txtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendMessageAction();
            }
        }


        private void SendMessageAction()
        {
            if (this.ViewModel.SelectedClient != null)
            {
                this.Server.SendChatMessageToClient(this.ViewModel.SelectedClient, txtMessage.Text);
            }
            else
            {
                this.Server.SendChatMessageToAllClients(txtMessage.Text);
            }
            txtMessage.Clear();
        }





        private void AddEvent(string message)
        {
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                this.ViewModel.Events.Insert(0, new Event(message));
            });
        }
        private void ClearClient()
        {
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                this.ViewModel.Clients.Clear();
            });
        }
        private void AddClient(ClientInfo client)
        {
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                this.ViewModel.Clients.Add(client);
            });
        }
        private void RemoveClient(ClientInfo client)
        {
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                this.ViewModel.Clients.Remove(client);
            });
        }
    }
}
