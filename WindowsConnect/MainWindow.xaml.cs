using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Media;
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
        private ObservableCollection<Device> _device = new ObservableCollection<Device>();


        protected override void OnClosed(EventArgs e)
        {
            _udpClient.Close();
            base.OnClosed(e);
        }

        public void SetVolume(int volume)
        {
            _volumeService.setVolume(volume);
        }


        public void AddDevice(Device device)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                PlayStepasSound();
                var result = MessageBox.Show($"Устройство {device.Name} запрашевает подключение.\n " +
                    $"Подключить данное устройство?", "Добавление устройства", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    UDPClientService.SendMessage("200", device.IP, SettingsService.UDP_LISTEN_PORT);
                    _tcpClient = new TCPClientService(this, device);
                    _device.Add(device);
                }
                else
                {
                    UDPClientService.SendMessage("500", device.IP, SettingsService.UDP_LISTEN_PORT);
                }
            }));

            sendWallpaper(device);
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
                
                var command = CommandHelper.createCommand(Command.setWallpaper, bytes);

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
            var command = CommandHelper.createCommand(Command.setHostInfo, SettingsService.getHostInfo());
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

        public MainWindow()
        {
            InitializeComponent();
            Devices.ItemsSource = _device;
            _udpClient = new UDPClientService(this);
            imgQRCode.Source = QRCodeService.getQRCode();
            _volumeService = new VolumeService();
        }
    }
}