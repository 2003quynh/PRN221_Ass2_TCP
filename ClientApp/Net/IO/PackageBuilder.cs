using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientApp.Net.IO
{
    //them du lieu vao luong bo nho (memory stream)

    internal class PackageBuilder
    {
        MemoryStream _ms;
        public PackageBuilder()
        {
            _ms = new MemoryStream();
        }

        public void WriteOpCode(byte opcode)
        {
            _ms.WriteByte(opcode);
        }

        public void WriteString(string msg)
        {
            if (string.IsNullOrEmpty(msg))
            {
                throw new ArgumentNullException(nameof(msg), "Message cannot be null or empty.");
            }
            var msgLength = msg.Length;
            _ms.Write(BitConverter.GetBytes(msgLength));
            _ms.Write(Encoding.ASCII.GetBytes(msg));
        }
        public byte[] GetPacketBytes()
        {
            return _ms.ToArray();
        }

        public void WriteByte(byte[] data)
        {
            _ms.Write(data, 0, data.Length);
        }
    }
}
