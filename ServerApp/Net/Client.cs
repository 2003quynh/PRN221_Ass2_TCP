using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerApp.Net
{
    internal class Client
    {
        TcpListener tcpListener;
        public Client()
        {
            tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), 7891);
        }

        public void AcceptClient()
        {
            tcpListener.Start();
            var client = tcpListener.AcceptTcpClient();

        }
    }
}
