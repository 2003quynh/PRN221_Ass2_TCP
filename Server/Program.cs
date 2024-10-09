using Server.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Server
{
    class Program
    {
        static List<Client> users;
        static TcpListener _listener;
        static void Main(string[] args)
        {
            users = new List<Client>();
            _listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 7891);
            _listener.Start();
            while (true)
            {
                var client = new Client(_listener.AcceptTcpClient());
                AddUser(client);

            }
            
        }

        public static void AddUser(Client client) 
        {
            users.Add(client);
            BroadcastConnection();
        }

        static void BroadcastConnection()
        {
            if (users.Count > 0) 
            {             
                foreach (var client in users)
                {
                    foreach (var user in users)
                    {
                        var broadcastPacket = new PackageBuilder();
                        broadcastPacket.WriteOpCode(1);
                        broadcastPacket.WriteString(user.Username);
                        broadcastPacket.WriteString(user.Password);
                        broadcastPacket.WriteString(user.UserID.ToString());
                        client.ClientSocket.Client.Send(broadcastPacket.GetPackageBytes());
                    }
                }
            } else
            {
                Console.WriteLine("NooNooo");
            }
        }
        public static void BroadCastMessage(string message)
        {
            foreach (var user in users)
            {
                var messsagePacket = new PackageBuilder();
                messsagePacket.WriteOpCode(5);
                messsagePacket.WriteString(message);
                user.ClientSocket.Client.Send(messsagePacket.GetPackageBytes());
            }
        }

        public static void BroadcastDisconnect( string userId)
        {
            var disconnectedUser = users.Where(x => x.UserID.ToString() == userId).FirstOrDefault();
            users.Remove(disconnectedUser);
            foreach (var client in users)
            {
                var broadcastPackage = new PackageBuilder();
                broadcastPackage.WriteOpCode(10);
                broadcastPackage.WriteString(userId);
                client.ClientSocket.Client.Send(broadcastPackage.GetPackageBytes());
            }
            BroadCastMessage($"{disconnectedUser.Username} Disconnected!");
        }
    }
}