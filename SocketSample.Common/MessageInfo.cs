using System;
using System.Collections.Generic;
using System.Text;

namespace SocketSample.Common
{
    public class MessageInfo : BaseObject
    {
        private ClientInfo _From;
        public ClientInfo From { get { return _From; } set { _From = value; OnPropertyChanged(nameof(From)); } }
        private ClientInfo _To;
        public ClientInfo To { get { return _To; } set { _To = value; OnPropertyChanged(nameof(To)); } }
        private string _Content;
        public string Content { get { return _Content; } set { _Content = value; OnPropertyChanged(nameof(Content)); } }
    }
}
