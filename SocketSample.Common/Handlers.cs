using System;
using System.Collections.Generic;
using System.Text;

namespace SocketSample.Common
{
    public delegate void OnLogHandlerServer(string logMessage);
    public delegate void OnReceivedHandlerServer(ClientInfo client, DataPackage dataPackage);
    public delegate void OnClientConnectedHandlerServer(ClientInfo client);
    public delegate void OnClientDisconnectedHandlerServer(ClientInfo client);
    public delegate void OnClientSendGlobalChatMessageHandlerServer(ClientInfo client);

    public delegate void OnLogHandlerClient(string logMessage);
    public delegate void OnReceivedHandlerClient(DataPackage dataPackage);
    public delegate void OnClientListReceivedHandlerClient(List<ClientInfo> clients);
    public delegate void OnNewClientReceivedHandlerClient(ClientInfo client);
    public delegate void OnClientLeftReceivedHandlerClient(ClientInfo client);
}
