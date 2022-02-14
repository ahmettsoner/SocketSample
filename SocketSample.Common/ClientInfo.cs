using System;
using System.Collections.Generic;
using System.Text;

namespace SocketSample.Common
{
    public class ClientInfo : BaseObject
    {
        private Guid _ID;
        public Guid ID { get { return _ID; } set { _ID = value; OnPropertyChanged(nameof(ID)); } }
        private string _Username;
        public string Username { get { return _Username; } set { _Username = value; OnPropertyChanged(nameof(Username)); } }
        private string _Host;
        public string Host { get { return _Host; } set { _Host = value; OnPropertyChanged(nameof(Host)); } }
        private int _Port;
        public int Port { get { return _Port; } set { _Port = value; OnPropertyChanged(nameof(Port)); } }

        public ClientInfo()
        {

        }
        public ClientInfo(Guid ID)
        {
            this.ID = ID;
        }
    }
}
