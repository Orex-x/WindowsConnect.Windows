using System;
using System.Collections.ObjectModel;
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
    public partial class MainWindow : Window, ICommandController
    {
        private UDPClientService _udpClient;
        private VolumeService _volumeService;
        private ObservableCollection<Device> _device = new ObservableCollection<Device>();


        protected override void OnClosed(EventArgs e)
        {
            _udpClient.Close();
            base.OnClosed(e);
        }

        public void setVolume(int volume)
        {
            _volumeService.setVolume(volume);
        }

        public async void addDevice(Device device)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                var result = MessageBox.Show($"Устройство {device.Name} запрашевает подключение.\n " +
                    $"Подключить данное устройство?", "Добавление устройства", MessageBoxButton.YesNo);
                if(result == MessageBoxResult.Yes)
                {
                    _device.Add(device);
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

                TCPClientService.SendMessage(command, device.IP, device.Port);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }


        public void sleep()
        {
            Application.SetSuspendState(PowerState.Hibernate, false, false);
        }

        public void requestAddDevice(Device device)
        {
            var command = CommandHelper.createCommand(Command.setHostInfo, SettingsService.getHostInfo());
            UDPClientService.SendMessage(command, device.IP, device.Port);
        }

        public void playStepasSound()
        {
            var player = new SoundPlayer($"{Environment.CurrentDirectory}\\res\\stepas_sound.wav");
            player.Play();
        }

        public MainWindow()
        {
            InitializeComponent();
            Devices.ItemsSource = _device;
            _udpClient = new UDPClientService(SettingsService.UDP_LISTEN_PORT, this);
            imgQRCode.Source = QRCodeService.getQRCode();
            _volumeService = new VolumeService();
            TCPClientService.Receive();
        }
    }
}
