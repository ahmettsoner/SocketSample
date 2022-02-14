using Newtonsoft.Json;
using SocketSample.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SocketSample.Socket
{
    public class ClientSocket
    {
        public OnReceivedHandlerClient OnReceived;
        public OnLogHandlerClient OnLogReceived;
        public OnClientListReceivedHandlerClient OnClientListReceived;
        public OnNewClientReceivedHandlerClient OnNewClientJoined;
        public OnClientLeftReceivedHandlerClient OnClientLeft;

        private void AddLog(string logMessage)
        {
            if (OnLogReceived != null)
            {
                OnLogReceived(logMessage);
            }
        }




        public ClientInfo Client { get; private set; } = new ClientInfo();
        private System.Net.Sockets.Socket clientSocket;
        private byte[] buffers = new byte[1024];
        private IPEndPoint endPoint;



        public IPAddress Host { get; private set; }
        public int Port { get; private set; }
        public List<ClientInfo> Clients { get; private set; } = new List<ClientInfo>();


        public ClientSocket(string Host, int Port)
        {
            this.Host = IPAddress.Parse(Host);
            this.Port = Port;
        }
        public ClientSocket(IPAddress Host, int Port)
        {
            this.Host = Host;
            this.Port = Port;
        }


        public void Initialize()
        {
            clientSocket = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            endPoint = new IPEndPoint(this.Host, this.Port);
        }
        public void Disconnect()
        {
            try
            {
                clientSocket.Close();
                clientSocket.Dispose();
            }
            catch (SocketException ex)
            {
                this.AddLog($"ERROR - {ex.Message}");
            }
            catch (ObjectDisposedException ex)
            {
                this.AddLog($"ERROR - {ex.Message}");
            }
        }
        public void Connect()
        {
            try
            {
                clientSocket.BeginConnect(endPoint, ConnectCallback, null);
            }
            catch (SocketException ex)
            {
                this.AddLog($"ERROR - {ex.Message}");
            }
            catch (ObjectDisposedException ex)
            {
                this.AddLog($"ERROR - {ex.Message}");
            }
        }
        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                clientSocket.EndConnect(ar);
                Receive();
            }
            catch (SocketException ex)
            {
                this.AddLog($"ERROR - {ex.Message}");
            }
            catch (ObjectDisposedException ex)
            {
                this.AddLog($"ERROR - {ex.Message}");
            }
        }
        private void Receive()
        {
            clientSocket.BeginReceive(buffers, 0, buffers.Length, SocketFlags.None, ReceiveCallback, null);
        }
        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                int received = clientSocket.EndReceive(ar);

                if (received == 0)
                {
                    return;
                }



                DataPackage dataPackage = new DataPackage(buffers, received);

                if (OnReceived != null)
                {
                    OnReceived(dataPackage);
                }

                if (clientSocket.Connected)
                {
                    Receive();
                }
            }
            catch (SocketException ex)
            {
                this.AddLog($"ERROR - {ex.Message}");
            }
            catch (ObjectDisposedException ex)
            {
                this.AddLog($"ERROR - {ex.Message}");
            }
        }
        private void Send(MessageTypes messageType, string message)
        {
            try
            {
                DataPackage dataPackage = new DataPackage(messageType, message);
                byte[] payload = dataPackage.ToByteArray();

                clientSocket.BeginSend(payload, 0, payload.Length, SocketFlags.None, SendCallback, null);
            }
            catch (SocketException ex)
            {
                this.AddLog($"ERROR - {ex.Message}");
            }
            catch (ObjectDisposedException ex)
            {
                this.AddLog($"ERROR - {ex.Message}");
            }
        }
        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                clientSocket.EndSend(ar);
            }
            catch (SocketException ex)
            {
                this.AddLog($"ERROR - {ex.Message}");
            }
            catch (ObjectDisposedException ex)
            {
                this.AddLog($"ERROR - {ex.Message}");
            }
        }



        public ClientInfo GetClient(Guid clientID)
        {
            ClientInfo result = null;

            result = Clients.SingleOrDefault(o => o.ID == clientID);

            return result;
        }



        public void SendBYEMessageToClient()
        {
            Send(MessageTypes.Bye, null);
        }
        public void SendSetUsernameMessage(string username)
        {
            Send(MessageTypes.SetUsername, username);
        }
        public void SendGlobalChatMessage(string message)
        {
            MessageInfo mi = new MessageInfo();
            mi.From = Client;
            mi.Content = message;

            var data = JsonConvert.SerializeObject(mi);

            string messageText = $"{message}";
            Send(MessageTypes.MessageToAll, messageText);
            this.AddLog($"> {message}");
        }
        public void SendChatMessageToClient(ClientInfo targetClient, string message)
        {
            string messageText = messageText = $"{targetClient.ID.ToString()}:TO-{message}";
            Send(MessageTypes.MessageToClient, messageText);
            this.AddLog($"{targetClient.ID} > {message}");
        }


        public void WelcomeHelloMessage(string message)
        {
            this.AddLog($"Server : {message}");
        }
        public void WelcomeGoodByeMessage()
        {
            this.Disconnect();
            this.AddLog($"Disconnected from Server");
        }
        public void WelcomeClientListMessage(string message)
        {
            List<ClientInfo> cil = JsonConvert.DeserializeObject<List<ClientInfo>>(message);

            this.Clients.Clear();
            this.Clients.AddRange(cil);

            if (OnClientListReceived != null)
            {
                OnClientListReceived(cil);
            }
        }
        public void WelcomeNewClientMessage(string message)
        {
            ClientInfo ci = JsonConvert.DeserializeObject<ClientInfo>(message);

            this.Clients.Add(ci);
            if (OnNewClientJoined != null)
            {
                OnNewClientJoined(ci);
            }
        }
        public void WelcomeClientLeftMessage(string message)
        {
            ClientInfo ci = JsonConvert.DeserializeObject<ClientInfo>(message);

            this.Clients.Remove(ci);
            if (OnClientLeft != null)
            {
                OnClientLeft(ci);
            }

            this.AddLog($"({ci.ID.ToString()}) Left...");
        }
        public void WelcomeChatMessage(string message)
        {
            this.AddLog($"Server > : {message}");
        }
        public void WelcomeChatMessageFromClient(ClientInfo sourceClient, string message)
        {
            this.AddLog($"{sourceClient.Username} > : {message}");
        }
    }
}
