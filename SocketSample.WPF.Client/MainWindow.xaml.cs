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

namespace SocketSample.WPF.Client
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
        public ClientSocket Client { get; set; }


        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = ViewModel;

            this.Client = new ClientSocket("127.0.0.1", 3333);

            this.Client.OnLogReceived += (logMessage) =>
            {
                this.AddEvent(logMessage);
            };
            this.Client.OnNewClientJoined += (client) =>
            {
                this.AddClient(client);
            };
            this.Client.OnClientLeft += (client) =>
            {
                this.RemoveClient(client);
            };
            this.Client.OnClientListReceived += (clientList) =>
            {
                this.ClearClient();
                foreach (var x in clientList)
                {
                    this.AddClient(x);
                }
            };
            this.Client.OnReceived += (data) =>
            {
                switch (data.Type)
                {
                    case MessageTypes.Welcome:
                        this.Client.Client.ID = Guid.Parse(data.Data);
                        this.SetClient();
                        this.Client.WelcomeHelloMessage(data.Data);
                        this.Client.SendSetUsernameMessage(this.GetUsername());
                        break;
                    case MessageTypes.Bye:
                        break;
                    case MessageTypes.GoodBye:
                        this.Client.WelcomeGoodByeMessage();
                        break;
                    case MessageTypes.ClientList:
                        this.Client.WelcomeClientListMessage(data.Data);
                        break;
                    case MessageTypes.NewClient:
                        this.Client.WelcomeNewClientMessage(data.Data);
                        break;
                    case MessageTypes.LeftClient:
                        this.Client.WelcomeClientLeftMessage(data.Data);
                        break;
                    case MessageTypes.RoomList:
                        break;
                    case MessageTypes.MessageToAll:
                        this.Client.WelcomeChatMessage(data.Data);
                        break;
                    case MessageTypes.MessageToClient:
                        string[] messageHeaderParts = data.Data.Split(":FROM-");
                        if (messageHeaderParts.Length == 2)
                        {
                            Guid sourceClientID = Guid.Parse(messageHeaderParts[0]);
                            ClientInfo sourceClient = this.Client.GetClient(sourceClientID);
                            this.Client.WelcomeChatMessageFromClient(sourceClient, messageHeaderParts[1]);
                        }
                        break;
                    case MessageTypes.MessageToRoom:
                        break;
                    case MessageTypes.SetUsername:
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

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            this.ViewModel.Clear();
            this.Client.Initialize();
            this.Client.Connect();
        }
        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            SendMessageAction();
        }
        private void btnDisconnect_Click(object sender, RoutedEventArgs e)
        {
            this.Client.SendBYEMessageToClient();
            this.ViewModel.Clear();
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
                ClientInfo ci = this.Client.GetClient(this.ViewModel.SelectedClient.ID);
                this.Client.SendChatMessageToClient(ci, txtMessage.Text);
            }
            else
            {
                this.Client.SendGlobalChatMessage(txtMessage.Text);
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
        private void SetClient()
        {
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                this.Title = $"Client<{this.Client.Client.ID.ToString()}>";
            });
        }
        private string GetUsername()
        {
            string result = null;
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                result = txtUsername.Text;
            });

            return result;
        }

    }

}
