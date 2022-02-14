using System;
using System.Collections.Generic;
using System.Text;

namespace Common
{
    public class Event : BaseObject
    {
        private DateTime _Timestamp;
        public DateTime Timestamp { get { return _Timestamp; } set { _Timestamp = value; OnPropertyChanged(nameof(Timestamp)); } }
        private string _Content;
        public string Content { get { return _Content; } set { _Content = value; OnPropertyChanged(nameof(Content)); } }

        public Event(string Content)
        {
            this.Timestamp = DateTime.Now;
            this.Content = Content;
        }
        public Event(DateTime Timestamp, string Content)
        {
            this.Timestamp = Timestamp;
            this.Content = Content;
        }
    }
}
