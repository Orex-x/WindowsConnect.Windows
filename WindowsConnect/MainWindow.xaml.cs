using DocumentFormat.OpenXml.EMMA;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using WindowsConnect.Interfaces;
using WindowsConnect.Services;

using Application = System.Windows.Forms.Application;
using Clipboard = System.Windows.Forms.Clipboard;
using Device = WindowsConnect.Models.Device;
using MessageBox = System.Windows.MessageBox;


namespace WindowsConnect
{
    public partial class MainWindow : Window, ICommandController, IException, ITCPClientService
    {
        private KeyboardService _keyboardService;
        private UDPClientService _udpClient;
        private TCPClientService _tcpClient;
        private VolumeService _volumeService;
        private List<Device> _devices;
        private bool isConnect = false;


        protected override void OnClosed(EventArgs e)
        {
            _udpClient.Close();
            base.OnClosed(e);
        }

        public void CloseConnection()
        {
            _tcpClient.SendMessage(Command.CloseConnection);
            _tcpClient.Dispose();
            isConnect = false;
            _tcpClient = new TCPClientService(this);

            Dispatcher.Invoke(new Action(() =>
            {
                txtDeviceStatus.Text = "отключен";
            }));
        }

        public void SetVolume(int volume)
        {
            _volumeService.SetVolume(volume);
        }


        public void RequestConnectDevice(Device device)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                var d = _devices.FirstOrDefault(x => x.IP == device.IP);
                if(d != null)
                {
                   ConnectDevice(d);
                }
                else
                {
                    PlayStepasSound();
                    var result = MessageBox.Show($"Устройство {device.Name} запрашевает подключение.\n " +
                        $"Подключить данное устройство?", "Добавление устройства", MessageBoxButton.YesNo);

                    if (result == MessageBoxResult.Yes)
                    {
                        _devices.Add(device);
                        ConnectDevice(device);
                    }
                    else
                        UDPClientService.SendMessage("500", device.IP, SettingsService.UDP_LISTEN_PORT);
                    
                }
            }));
        }

        public void ConnectDevice(Device device)
        {
            UDPClientService.SendMessage("200", device.IP, SettingsService.UDP_LISTEN_PORT);
            Database.Save(Database.DEVICE_PATH, _devices);
            var command = CommandHelper.CreateCommand(Command.OpenConnection, SettingsService.getHostInfo());
            var answer = UDPClientService.SendMessageWithReceive(command, device.IP);
            if (answer == "200")
            {
                txtDeviceName.Text = device.Name;
                txtDeviceStatus.Text = "подключен";
                SendWallpaper();
                isConnect = true;
            }
        }


        private async void SendWallpaper()
        {
            try
            {
                var path =
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Microsoft\\Windows\\Themes\\TranscodedWallpaper");

                byte[] bytes = File.ReadAllBytes(path);

                await Task.Run(() => {
                    _tcpClient.SendMessage(bytes, Command.SetWallpaper, true);
                });
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


        public void SetProgress(int progress)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                pbStatus.Value = progress;
            }));
        }

        public void VirtualTouchPadChanged(int x, int y, int action, int pointer)
        {
            MouseService.VirtualTouchPadChanged(x, y, action, pointer);
        }


        public MainWindow()
        {
            InitializeComponent();

            if (!Directory.Exists("data")) Directory.CreateDirectory("data");

            var h = SettingsService.getHostInfo();
            txtIP.Text = $"IP: {h["localIP"]}";


          /*  var rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            rkApp.SetValue("Windows Connect", Application.ExecutablePath.ToString());*/

            _tcpClient = new TCPClientService(this);
            _udpClient = new UDPClientService(this);
            imgQRCode.Source = QRCodeService.getQRCode();
            _volumeService = new VolumeService();
            _keyboardService = new KeyboardService();

            _devices = Database.Get<List<Device>>(Database.DEVICE_PATH);

            if(_devices == null) 
                _devices = new List<Device>();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var windowClipboardManager = new ClipboardService(this);
            windowClipboardManager.ClipboardChanged += ClipboardChanged;
        }

        private void ClipboardChanged(object sender, EventArgs e)
        {
            if (isConnect)
            {
                if (Clipboard.ContainsText())
                {
                    var text = Clipboard.GetText();
                    _tcpClient.SendMessage(Encoding.UTF8.GetBytes(text), Command.SetTextClipBoard, false);
                }

                if (Clipboard.ContainsImage())
                {
                    var img = Clipboard.GetImage();
                }
                if (Clipboard.ContainsAudio())
                {

                }
            }
        }


        private void ButtonClickOpenFolder(object sender, RoutedEventArgs e)
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

        public void DownKeyboardHardwareKeyPress(int code)
        {
            _keyboardService.down(code);
        }

        public void UpKeyboardHardwareKeyPress(int code)
        {
            _keyboardService.up(code);
        }

        public void KeyboardPress(char ch)
        {
            _keyboardService.press(ch); 
        }
    }
}