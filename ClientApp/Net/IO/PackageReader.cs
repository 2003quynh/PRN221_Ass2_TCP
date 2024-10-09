using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ClientApp.Net.IO
{
     class PackageReader : BinaryReader
    {
        private NetworkStream _ns;
        public PackageReader(NetworkStream ns) : base(ns)
        {
            _ns = ns;
        }

        public String ReadMessage()
        {
            byte[] buffer;
            var length = ReadInt32();
            buffer = new byte[length];
            _ns.Read(buffer, 0, length);
            var msg = Encoding.ASCII.GetString(buffer);
            return msg;
        }
    }
}
