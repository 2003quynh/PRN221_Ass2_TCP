using ClientApp.Net.IO;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace ClientApp.Net
{
     class Server
    {
        TcpClient tcpClient;
        private static volatile bool _isConnect = false;
        public PackageReader PackageReader;

        public event Action msgReceivedEvent;
        public event Action connectedEvent;
        public event Action userDisconnectEvent;
        public Server() { 
            tcpClient = new TcpClient();
        }

        public void ConnectToServer(string username, String password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Username and password must not be null or empty");
            }

            _isConnect = true;
            if (!tcpClient.Connected)
            {
                tcpClient.Connect("127.0.0.1", 7891);
                PackageReader = new PackageReader(tcpClient.GetStream());
                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                {
                    var connectPacket = new PackageBuilder();
                    connectPacket.WriteOpCode(0);
                    connectPacket.WriteString(username);
                    connectPacket.WriteString(password);
                    tcpClient.Client.Send(connectPacket.GetPacketBytes()); 
                }
                
                ReadPackages();
            }
        }
        private bool CanLogout(object parameter)
        {
            return tcpClient != null && tcpClient.Connected;
        }

        public void Logout(string username)
        {
            if (_isConnect)
            {
                _isConnect = false;

                // Send a logout message to the server
                if (tcpClient.Connected)
                {
                    var disconnectPacket = new PackageBuilder();
                    disconnectPacket.WriteOpCode(1);  // OpCode 1 for disconnect
                    disconnectPacket.WriteString(username);
                    tcpClient.Client.Send(disconnectPacket.GetPacketBytes());
                }

                tcpClient.Close();
                Console.WriteLine($"{username} has logged out.");
            }
        }

        public void Disconnect(string username)
        {
            if (_isConnect)
            {
                _isConnect = false;  // Stop the reading loop
            }

            // Send a disconnect message if necessary
            if (tcpClient.Connected)
            {
                var disconnectPacket = new PackageBuilder();
                disconnectPacket.WriteOpCode(1);  // OpCode 1 for disconnect
                disconnectPacket.WriteString(username);
                tcpClient.Client.Send(disconnectPacket.GetPacketBytes());
            }

            // Close the connection
            tcpClient.Close();
            Console.WriteLine("Disconnected from the server.");
        }
        private void HandleLoginResponse()
        {
            string response = PackageReader.ReadMessage();  // Read login response from server

            if (response == "SUCCESS")
            {
                Console.WriteLine("Login successful.");
                // Trigger any additional login success actions
            } else if (response == "FAILURE")
            {
                Console.WriteLine("Login failed. Disconnecting...");
                tcpClient.Close();  // Close connection if login failed
                _isConnect = false;  // Stop the connection loop
            }
        }

        private void ReadPackages()
        {
            //_isConnect = true;
            Task.Run(() =>
            {
                while (_isConnect)
                {
                    var opcode = PackageReader.ReadByte();
                    switch (opcode)
                    {
                        case 0:  // Login response
                            HandleLoginResponse();
                            break;
                        case 1:
                            connectedEvent?.Invoke();
                            break;
                        case 2:  // File transfer opcode
                            ReceiveFile();
                            break;
                        case 5:
                            msgReceivedEvent?.Invoke();
                            break;
                        case 10:
                            userDisconnectEvent?.Invoke();
                            break;
                        default:
                            Console.WriteLine("Nhom nhom");
                            break;
                    
                    }

                }
            });
        }
        private void ReceiveFile()
        {
            try
            {
                // Read the file name
                var fileNameLength = PackageReader.ReadInt32();
                var fileNameBytes = new byte[fileNameLength];
                PackageReader.Read(fileNameBytes, 0, fileNameLength);
                string fileName = Encoding.ASCII.GetString(fileNameBytes);

                // Read the file size
                var fileSize = PackageReader.ReadInt32();

                // Read the file content
                byte[] fileBuffer = new byte[fileSize];
                int bytesRead = 0;
                while (bytesRead < fileSize)
                {
                    bytesRead += PackageReader.Read(fileBuffer, bytesRead, fileSize - bytesRead);
                }

                // Save the file to a predefined location
                var savePath = Path.Combine("ReceivedFiles", fileName);
                Directory.CreateDirectory("ReceivedFiles");
                File.WriteAllBytes(savePath, fileBuffer);

                Console.WriteLine($"File received: {fileName}, saved to {savePath}");
            } catch (Exception ex)
            {
                Console.WriteLine($"Error receiving file: {ex.Message}");
            }
        }

        public void SendMessageToServer(string message)
        {
            var messagePackage = new PackageBuilder();
            messagePackage.WriteOpCode(5);
            messagePackage.WriteString(message);
            tcpClient.Client.Send(messagePackage.GetPacketBytes());
        }

        public void SendFileToServer(string filePath)
        {
            //if (!tcpClient.Connected || string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            //{
            //    Console.WriteLine("Connection is not established or invalid file.");
            //    return;
            //}

            FileDialog fd = new OpenFileDialog();
            if(fd.ShowDialog() == true)
            {
                // Read the file into a byte array
                byte[] fileBytes = File.ReadAllBytes(filePath);
                var fileName = Path.GetFileName(filePath);
                var fileNameBytes = Encoding.ASCII.GetBytes(fileName);

                // Build the package for file transfer
                var fileTransferPackage = new PackageBuilder();
                fileTransferPackage.WriteOpCode(2);  // Assuming OpCode 2 is for file transfer
                fileTransferPackage.WriteString(fileName); // Send the file name
                fileTransferPackage.WriteByte(BitConverter.GetBytes(fileBytes.Length));  // Send the file size
                fileTransferPackage.WriteByte(fileBytes);  // Send the file data

                // Send the file package
                tcpClient.Client.Send(fileTransferPackage.GetPacketBytes());
                Console.WriteLine($"File '{fileName}' sent to server.");
            }

            
        }
    }
}
