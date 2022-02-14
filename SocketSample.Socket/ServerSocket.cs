using SocketSample.Common;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace SocketSample.Socket
{
    public class ServerSocket
    {
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

    }
}
