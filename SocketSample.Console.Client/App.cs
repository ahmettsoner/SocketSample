using SocketSample.Common;
using SocketSample.Socket;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocketSample.Console.Client
{
    public class App
    {
        public ClientSocket Client { get; set; }
        private List<Event> _Events = new List<Event>();
        public List<Event> Events { get { return _Events; } set { _Events = value; } }

        private List<ClientInfo> _Clients = new List<ClientInfo>();
        public List<ClientInfo> Clients { get { return _Clients; } set { _Clients = value; } }

        private ClientInfo _SelectedClient;
        public ClientInfo SelectedClient { get { return _SelectedClient; } set { _SelectedClient = value; } }



        internal void Clear()
        {
            this.Clients.Clear();
            this.Events.Clear();
            this.SelectedClient = null;
        }

        public void Run()
        {
            this.Initialize();
            this.Clear();
            this.Client.Initialize();
            this.Client.Connect();

            SendMessageAction();
        }

        public void Initialize()
        {
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


        private void SendMessageAction()
        {
            System.Console.Write("Message:\r");
            string msg = System.Console.ReadLine();
            if (this.SelectedClient != null)
            {
                this.Client.SendChatMessageToClient(this.SelectedClient, msg);
            }
            else
            {
                this.Client.SendGlobalChatMessage(msg);
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
        private void SetClient()
        {
            System.Console.Title = $"Client<{this.Client.Client.ID.ToString()}>";
        }
        private string GetUsername()
        {
            string result = null;

            System.Console.WriteLine("Username:");
            result = System.Console.ReadLine();

            return result;
        }
    }
}
