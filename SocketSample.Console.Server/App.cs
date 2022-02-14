using SocketSample.Common;
using SocketSample.Socket;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace SocketSample.Console.Server
{
    public class App
    {
        public ServerSocket Server { get; set; }
        private List<Event> _Events = new List<Event>();
        public List<Event> Events { get { return _Events; } set { _Events = value; } }

        private List<ClientInfo> _Clients = new List<ClientInfo>();
        public List<ClientInfo> Clients { get { return _Clients; } set { _Clients = value; } }

        private ClientInfo _SelectedClient;
        public ClientInfo SelectedClient { get { return _SelectedClient; } set { _SelectedClient = value; } }

        public void Run()
        {
            this.Initialize();
            this.Server.Initialize();
            this.Server.StartServer();

            SendMessageAction();
        }

        public void Initialize()
        {
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


        private void SendMessageAction()
        {
            System.Console.Write("Message:\r");
            string msg = System.Console.ReadLine();
            if (this.SelectedClient != null)
            {
                this.Server.SendChatMessageToClient(this.SelectedClient, msg);
            }
            else
            {
                this.Server.SendChatMessageToAllClients(msg);
            }

            SendMessageAction();
        }


        private void AddEvent(string message)
        {
            System.Console.WriteLine(message);
        }
        private void ClearClient()
        {
            this.Clients.Clear();
        }
        private void AddClient(ClientInfo client)
        {
            this.Clients.Add(client);
        }
        private void RemoveClient(ClientInfo client)
        {
            this.Clients.Remove(client);
        }

    }
}
