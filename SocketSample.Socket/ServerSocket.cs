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
    public class ServerSocket
    {
        public OnReceivedHandlerServer OnReceived;
        public OnLogHandlerServer OnLogReceived;
        public OnClientConnectedHandlerServer OnClientConnected;
        public OnClientDisconnectedHandlerServer OnClientDisconnected;

        private void AddLog(string logMessage)
        {
            if (OnLogReceived != null)
            {
                OnLogReceived(logMessage);
            }
        }


        private Guid ServerID = new Guid();
        private System.Net.Sockets.Socket serverSocket;
        private Dictionary<ClientInfo, System.Net.Sockets.Socket> clientSockets = new Dictionary<ClientInfo, System.Net.Sockets.Socket>();
        private byte[] buffers = new byte[1024];
        private IPEndPoint endPoint;

        public IPAddress Host { get; private set; }
        public int Port { get; private set; }
        public int ClientLimit { get; private set; }


        public ServerSocket(string Host, int Port, int ClientLimit = 0)
        {
            this.Host = IPAddress.Parse(Host);
            this.Port = Port;
            this.ClientLimit = ClientLimit;
        }
        public ServerSocket(IPAddress Host, int Port)
        {
            this.Host = Host;
            this.Port = Port;
        }


        public void Initialize()
        {
            serverSocket = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            endPoint = new IPEndPoint(this.Host, this.Port);
        }
        public void StartServer()
        {
            try
            {
                this.AddLog("Server Starting...");
                serverSocket.Bind(endPoint);
                serverSocket.Listen(ClientLimit);
                this.AddLog("Waiting for clients");
                Accept();
            }
            catch (Exception ex)
            {
                this.AddLog($"ERROR - {ex.Message}");
            }
        }
        public void StopServer()
        {
            foreach (KeyValuePair<ClientInfo, System.Net.Sockets.Socket> x in clientSockets)
            {
                x.Value.Shutdown(SocketShutdown.Both);
                x.Value.Close();
            }

            serverSocket.Close();
        }


        private void Accept()
        {
            serverSocket.BeginAccept(AcceptCallback, null);
        }
        private void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                var clientSocket = serverSocket.EndAccept(ar);

                ClientInfo ci = CreateClient(clientSocket);
                clientSockets.Add(ci, clientSocket);

                this.SendHelloMessageToNewClient(ci);

                this.AddLog($"Client ({ci.ID.ToString()}) connecting");
                if (OnClientConnected != null)
                {
                    OnClientConnected(ci);
                }
                this.AddLog($"Client ({ci.ID.ToString()}) connected");


                Receive(clientSocket, ci);

                Accept();
            }
            catch (Exception ex)
            {
                this.AddLog($"ERROR - {ex.Message}");
            }
        }
        private void Receive(System.Net.Sockets.Socket clientSocket, ClientInfo client)
        {
            if (clientSocket.Connected)
            {
                clientSocket.BeginReceive(buffers, 0, buffers.Length, SocketFlags.None, new AsyncCallback((ar) => { ReceiveCallback(ar, client); }), null);
            }
        }
        private void ReceiveCallback(IAsyncResult ar, ClientInfo client)
        {
            try
            {
                if (clientSockets.ContainsKey(client))
                {
                    System.Net.Sockets.Socket s = clientSockets[client];

                    if (s == null)
                    {
                        this.AddLog($"({client.ID.ToString()}) not exists ");
                        return;
                    }

                    int received = s.EndSend(ar);

                    DataPackage dataPackage = new DataPackage(buffers, received);

                    if (OnReceived != null)
                    {
                        OnReceived(client, dataPackage);
                    }

                    Receive(s, client);
                }
            }
            catch (Exception ex)
            {
                this.AddLog($"ERROR - {ex.Message}");
            }
        }
        private void SendToClient(ClientInfo client, MessageTypes messageType, string message = null)
        {
            try
            {
                if (client != null)
                {
                    if (clientSockets.ContainsKey(client))
                    {
                        System.Net.Sockets.Socket s = clientSockets[client];

                        if (s == null)
                        {
                            this.AddLog($"({client.ID.ToString()}) not exists ");
                            return;
                        }

                        DataPackage dataPackage = new DataPackage(messageType, message);
                        byte[] payload = dataPackage.ToByteArray();

                        if (s.Connected)
                        {
                            s.BeginSend(payload, 0, payload.Length, SocketFlags.None, new AsyncCallback((ar) => { SendCallback(ar, client, message); }), null);

                            Receive(s, client);
                        }
                    }
                }
                else
                {
                    this.AddLog($"Client not defined");
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
        private void SendToAll(MessageTypes messageType, string message = null)
        {
            try
            {
                DataPackage dataPackage = new DataPackage(messageType, message);
                byte[] payload = dataPackage.ToByteArray();

                foreach (var x in clientSockets)
                {
                    if (x.Value.Connected)
                    {
                        x.Value.BeginSend(payload, 0, payload.Length, SocketFlags.None, new AsyncCallback((ar) => { SendCallback(ar, x.Key, message); }), null);

                        Receive(x.Value, x.Key);
                    }
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
        private void SendCallback(IAsyncResult ar, ClientInfo client, string message)
        {
            try
            {
                if (client != null)
                {
                    if (clientSockets.ContainsKey(client))
                    {
                        System.Net.Sockets.Socket s = clientSockets[client];

                        if (s == null)
                        {
                            this.AddLog($"({client.ID.ToString()}) not exists ");
                            return;
                        }

                        this.AddLog($"{message} to ({client.ID.ToString()}) ");
                        s.EndSend(ar);
                    }
                }
                else
                {
                    this.AddLog($"Client not defined");
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





        private ClientInfo CreateClient(System.Net.Sockets.Socket clientSocket)
        {
            ClientInfo result = null;

            Guid newClientID = Guid.NewGuid();
            IPEndPoint localIpEndPoint = clientSocket.LocalEndPoint as IPEndPoint;

            result = new ClientInfo(newClientID);
            result.Host = localIpEndPoint.Address.ToString();
            result.Port = localIpEndPoint.Port;

            return result;
        }
        public ClientInfo GetClient(Guid clientID)
        {
            ClientInfo result = null;

            foreach (var x in clientSockets)
            {
                if (clientID.ToString() == x.Key.ID.ToString())
                {
                    result = x.Key;
                    break;
                }
            }

            return result;
        }




        private void SendHelloMessageToNewClient(ClientInfo newClient)
        {
            SendToClient(newClient, MessageTypes.Welcome, newClient.ID.ToString());
        }
        private void SendGoodByeMessageToClient(ClientInfo targetClient)
        {
            SendToClient(targetClient, MessageTypes.GoodBye);
        }
        private void SendClientListToClient(ClientInfo client)
        {
            List<ClientInfo> cil = clientSockets.Keys.Where(o => o.ID.ToString() != client.ID.ToString()).ToList();
            var data = JsonConvert.SerializeObject(cil);

            SendToClient(client, MessageTypes.ClientList, data);
        }
        private void SendNewClientToAllClients(ClientInfo newClient)
        {
            foreach (var x in clientSockets)
            {
                if (newClient.ID.ToString() != x.Key.ID.ToString())
                {
                    var data = JsonConvert.SerializeObject(newClient);
                    SendToClient(x.Key, MessageTypes.NewClient, data);
                }
            }
        }
        private void SendClientLeftToAllClients(ClientInfo leftClient)
        {
            foreach (var x in clientSockets)
            {
                if (leftClient.ID.ToString() != x.Key.ID.ToString())
                {
                    var data = JsonConvert.SerializeObject(leftClient);
                    SendToClient(x.Key, MessageTypes.LeftClient, data);
                }
            }
        }
        private void SendChatMessageToAllClientsExceptClient(ClientInfo sourceClient, string message)
        {
            string messageText = message;

            foreach (var x in clientSockets)
            {
                if (sourceClient.ID.ToString() != x.Key.ID.ToString())
                {
                    SendToClient(x.Key, MessageTypes.MessageToAll, messageText);
                }
            }
        }
        public void SendChatMessageToClient(ClientInfo targetClient, string message)
        {
            string messageText = message;

            SendToClient(targetClient, MessageTypes.MessageToClient, messageText);
        }
        public void SendChatMessageToAllClients(string message)
        {
            string messageText = message;

            SendToAll(MessageTypes.MessageToAll, messageText);
        }
        public void SendChatMessageToClient(ClientInfo sourceClient, ClientInfo targetClient, string message)
        {
            string messageText = $"{sourceClient.ID.ToString()}:FROM-" + message;

            if (targetClient != null)
            {
                if (clientSockets.ContainsKey(targetClient))
                {
                    SendToClient(targetClient, MessageTypes.MessageToClient, messageText);
                }
            }
            else
            {
                this.AddLog($"Client not exists - {targetClient.ID}");
            }
        }



        public void WelcomeBYEMessage(ClientInfo sourceClient)
        {
            if (sourceClient != null)
            {
                if (clientSockets.ContainsKey(sourceClient))
                {
                    SendGoodByeMessageToClient(sourceClient);
                    this.AddLog($"({sourceClient.ID.ToString()}) disconnected");
                    SendClientLeftToAllClients(sourceClient);

                    if (OnClientDisconnected != null)
                    {
                        OnClientDisconnected(sourceClient);
                    }

                    clientSockets.Remove(sourceClient);
                    //s.Close();
                    //s.Dispose();
                }
            }
            else
            {
                this.AddLog($"Client not exists - {sourceClient.ID}");
            }
        }
        public void WelcomeSetUsernameMessage(ClientInfo sourceClient, string username)
        {
            if (sourceClient != null)
            {
                sourceClient.Username = username;
                this.SendClientListToClient(sourceClient);
                this.SendNewClientToAllClients(sourceClient);
            }
            else
            {
                this.AddLog($"Client not exists - {sourceClient.ID}");
            }
        }
        public void WelcomeGlobalChatMessage(ClientInfo sourceClient, string messageContent)
        {
            this.AddLog($"{sourceClient.Username} > : {messageContent}");

            SendChatMessageToAllClientsExceptClient(sourceClient, messageContent);
        }
        public void WelcomeChatMessageToClient(ClientInfo sourceClient, ClientInfo targetClient, string messageContent)
        {
            SendChatMessageToClient(sourceClient, targetClient, messageContent);
        }

    }
}
