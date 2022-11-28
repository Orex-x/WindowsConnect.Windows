using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using WindowsConnect.Interfaces;
using WindowsConnect.Services;

using Application = System.Windows.Forms.Application;
using Device = WindowsConnect.Models.Device;
using MessageBox = System.Windows.MessageBox;


namespace WindowsConnect
{
    public partial class MainWindow : Window, ICommandController, IException, ITCPClientService
    {
        private UDPClientService _udpClient;
        private TCPClientService _tcpClient;
        private VolumeService _volumeService;
        private List<Device> _devices;


        protected override void OnClosed(EventArgs e)
        {
            _udpClient.Close();
            base.OnClosed(e);
        }

        public void SetVolume(int volume)
        {
            _volumeService.setVolume(volume);
        }


        public void RequestConnectDevice(Device device)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                var d = _devices.FirstOrDefault(x => x.Name == device.Name);
                if(d != null)
                {
                    var command = CommandHelper.CreateCommand(Command.OpenConnection, SettingsService.getHostInfo());
                    var answer = UDPClientService.SendMessageWithReceive(command, device.IP);
                    if(answer == "200")
                    {
                        txtDeviceName.Text = device.Name;
                        txtDeviceStatus.Text = "подключен";
                        sendWallpaper(device);
                    }
                }
                else
                {
                    PlayStepasSound();
                    var result = MessageBox.Show($"Устройство {device.Name} запрашевает подключение.\n " +
                        $"Подключить данное устройство?", "Добавление устройства", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        _devices.Add(device);
                        Database.Save(Database.DEVICE_PATH, _devices);
                        UDPClientService.SendMessage("200", device.IP, SettingsService.UDP_LISTEN_PORT);
                        txtDeviceName.Text = device.Name;
                        txtDeviceStatus.Text = "подключен";
                        sendWallpaper(device);
                    }
                    else
                    {
                        UDPClientService.SendMessage("500", device.IP, SettingsService.UDP_LISTEN_PORT);
                    }
                }
            }));
        }


        private void sendWallpaper(Device device)
        {
            try
            {
                var path =
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Microsoft\\Windows\\Themes\\TranscodedWallpaper");

                byte[] bytes = File.ReadAllBytes(path);
                
                var command = CommandHelper.CreateCommand(Command.SetWallpaper, bytes);

                _tcpClient.SendMessage(command);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }


        public void Sleep()
        {
            Application.SetSuspendState(PowerState.Hibernate, false, false);
        }

        public void RequestAddDevice(Device device)
        {
            var command = CommandHelper.CreateCommand(Command.SetHostInfo, SettingsService.getHostInfo());
            UDPClientService.SendMessage(command, device.IP, device.Port);
        }

        public void PlayStepasSound()
        {
            var player = new SoundPlayer($"{Environment.CurrentDirectory}\\res\\stepas_sound.wav");
            player.Play();
        }

        public void Exception(Exception ex)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                MessageBox.Show(ex.Message);
            }));
        }

        public void Message(string message)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                MessageBox.Show(message);
            }));
        }

        public void ResetProgress()
        {
            Dispatcher.Invoke(new Action(() =>
            {
                pbStatus.Value = 0;
            }));
         
        }

        public void SetProgress(int progress)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                pbStatus.Value = progress;
            }));
        }

        public void VirtualTouchPadChanged(int x, int y, int action, int pointer)
        {
            Dispatcher.Invoke(new Action(() =>  
            {
                txtMouseLog.Text = $"x = {x} y = {y} action = {action} pointer = {pointer}";
            }));
            MouseService.VirtualTouchPadChanged(x, y, action, pointer);
        }


        public void CloseConnection()
        {
            var command = CommandHelper.CreateCommand(Command.CloseConnection, "");
            _tcpClient.SendMessage(command);
            _tcpClient.Dispose();
            Dispatcher.Invoke(new Action(() =>
            {
                txtDeviceStatus.Text = "отключен";
            }));
            _tcpClient = new TCPClientService(this);
        }

        public MainWindow()
        {
            InitializeComponent();
            _tcpClient = new TCPClientService(this);
            _udpClient = new UDPClientService(this);
            imgQRCode.Source = QRCodeService.getQRCode();
            _volumeService = new VolumeService();
            _devices = Database.Get<List<Device>>(Database.DEVICE_PATH);
            if(_devices == null) _devices = new List<Device>();
        }

       
        private void Button_Click_Open_Folder(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer", "data\\");
        }

        private void Button_Click_Close_Connection(object sender, RoutedEventArgs e)
        {
            CloseConnection();
        }

        public void OpenConnection()
        {
            Dispatcher.Invoke(new Action(() =>
            {
                txtDeviceStatus.Text = "подключен";
            }));
        }
    }
}