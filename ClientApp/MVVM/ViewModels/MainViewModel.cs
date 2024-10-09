using ClientApp.MVVM.Command;
using ClientApp.MVVM.Models;
using ClientApp.Net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ClientApp.MVVM.ViewModels
{
    class MainViewModel
    {
        public string Username { get; set; }
        public string Password { get; set; }    
        public string Message { get; set; }
        public string SelectedFilePath { get; set; }
        private string _connectionStatus = "Disconnected";
        public string ConnectionStatus
        {
            get => _connectionStatus;
            set
            {
                _connectionStatus = value;
                OnPropertyChanged("ConnectionStatus");
            }
        }

        public ObservableCollection<UserModel> Users { get; set; }
        public ObservableCollection<string> Messages { get; set; }

        public RelayCommand ConnectToServerCommand { get; set; }
        public RelayCommand LogoutCommand { get; set; }
        public RelayCommand SendMessageCommand { get; set; }
        public RelayCommand DisconnectCommand { get; set; }
        public RelayCommand SendFileCommand { get; set; }

        private Server _server;

        public MainViewModel() {
            Users = new ObservableCollection<UserModel>();
            Messages = new ObservableCollection<string>();

            _server = new Server();
            _server.connectedEvent += UserConnected;
            _server.msgReceivedEvent += MessageReceived;
            _server.userDisconnectEvent += RemoveUser;

            ConnectToServerCommand = new RelayCommand(o => _server.ConnectToServer(Username, Password), o => !string.IsNullOrEmpty(Username));
            LogoutCommand = new RelayCommand(o => _server.Logout(Username));
            SendMessageCommand = new RelayCommand(o => _server.SendMessageToServer(Message), o => !string.IsNullOrEmpty(Message));
            DisconnectCommand = new RelayCommand(o => _server.Disconnect(Username));
            SendFileCommand = new RelayCommand(o => _server.SendFileToServer(SelectedFilePath));
        }

       
        private void RemoveUser()
        {
            var userId = _server.PackageReader.ReadMessage();
            var user = Users.Where(x=>x.UserID == userId).FirstOrDefault();
            Application.Current.Dispatcher.Invoke(() => Users.Remove(user));
        }

        private void MessageReceived()
        {
            var msg = _server.PackageReader.ReadMessage();
            Application.Current.Dispatcher.Invoke(() => Messages.Add(msg));
        }

        private void UserConnected()
        {
            var user = new UserModel
            {
                Username = _server.PackageReader.ReadMessage(),
                Password = _server.PackageReader.ReadMessage(),
                UserID = _server.PackageReader.ReadMessage(),

            };
            if(!Users.Any(x => x.UserID == user.UserID))
            {
                Application.Current.Dispatcher.Invoke(() => Users.Add(user));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
