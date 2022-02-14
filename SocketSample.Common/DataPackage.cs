using System;
using System.Collections.Generic;
using System.Text;

namespace SocketSample.Common
{
    public class DataPackage
    {
        public string Data { get; set; }
        public MessageTypes Type { get; set; }

        public DataPackage(MessageTypes type, string data)
        {
            this.Type = type;
            this.Data = data;
        }

        public DataPackage(byte[] data, int limit)
        {
            if (data.Length > 0)
            {
                this.Type = (MessageTypes)data[0];
                this.Data = Encoding.ASCII.GetString(data, 1, limit - 1);
            }
        }

        public byte[] ToByteArray()
        {
            List<byte> result = new List<byte>();

            result.Add((byte)this.Type);

            if (!string.IsNullOrEmpty(this.Data))
            {
                var data = Encoding.ASCII.GetBytes(this.Data);
                result.AddRange(data);
            }

            return result.ToArray();
        }
    }
}
