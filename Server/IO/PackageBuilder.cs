using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.IO
{
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
        public byte[] GetPackageBytes()
        {
            return _ms.ToArray();
        }
    }
}
