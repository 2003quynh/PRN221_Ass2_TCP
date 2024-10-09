using Server.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class Client
    {
         
        public string Username { get; set; }
        public string Password { get; set; }
        public Guid UserID { get; set; }
        public TcpClient ClientSocket { get; set; }

        PackageReader _packetReader;
        public Client(TcpClient client) 
        {
            ClientSocket = client;
            UserID = Guid.NewGuid();
            _packetReader = new PackageReader(ClientSocket.GetStream());

            var opCode = _packetReader.ReadByte();
            Username = _packetReader.ReadMessage();
            Password = _packetReader.ReadMessage();

            if (!Login(Username, Password))
            {
                // If login fails, close the client connection
                Console.WriteLine($"{DateTime.Now}: Client connection failed!");
                ClientSocket.Close();
                return; // Exit the constructor if login fails
            }

            //Console.WriteLine($"{DateTime.Now}: Client [{Username}] has connected! bebe");
            //Program.AddUser(this);
            Task.Run(() => Process());

        }

        private bool Login(String username, String password)
        {
            

            // Authenticate the user
            if (AuthenticateUser(username, password))
            {
                Username = username; // Set the username if authenticated
                Console.WriteLine($"{DateTime.Now}: Client {Username} has connected!");

                // Notify successful login
                SendResponse(0, "SUCCESS"); // OpCode 0 for response
                return true; // Indicate successful login
            } else
            {
                // Authentication failed, notify client and return false
                SendResponse(0, "FAILURE"); // OpCode 0 for response
                return false; // Indicate failed login
            }
            
        }

        private bool AuthenticateUser(string username, string password)
        {
            switch (username)
            {
                case "admin":
                    return username == "admin" && password == "123";
                case "Quynh":
                    return username == "Quynh" && password == "123456";
                case "Hiu":
                    return username == "Hiu" && password == "123456";
                default:
                    return false;
            }
            //return username == "admin" && password == "123";

        }

        private void SendResponse(byte opcode, string message)
        {
            var responsePackage = new PackageBuilder();
            responsePackage.WriteOpCode(opcode);
            responsePackage.WriteString(message);
            ClientSocket.Client.Send(responsePackage.GetPackageBytes());
        }
        public void Process()
        {
            while (true)
            {
                try
                {
                    var opcode = _packetReader.ReadByte();
                    switch (opcode)
                    {
                       
                        case 5: // Message opcode
                            var msg = _packetReader.ReadMessage();
                            Console.WriteLine($"{DateTime.Now} message received: {msg} by {Username}");
                            Program.BroadCastMessage($"[{DateTime.Now}] : [{Username}] : {msg}");
                            break;
                        case 1: // Logout opcode
                            Console.WriteLine($"{Username} has logged out.");
                            ClientSocket.Close();
                            return; // Exit the process loop
                        default:
                            break;
                    }
                } catch (Exception)
                {
                    Console.WriteLine($"{UserID.ToString()} has disconnected!");
                    Program.BroadcastDisconnect(UserID.ToString());
                    ClientSocket.Close();
                    break;
                }
            }
        }

    }
}
