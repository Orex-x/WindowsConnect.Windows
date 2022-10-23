using Newtonsoft.Json.Linq;
using QRCoder;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Media;
using System.Runtime.InteropServices;
using WindowsConnect.Services;
using WindowsConnect.Interfaces;
using WindowsConnect.Models;
using AudioSwitcher.AudioApi;
using Device = WindowsConnect.Models.Device;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Windows.Forms;
using Application = System.Windows.Forms.Application;

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

        public void addDevice(Device device)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                _device.Add(device);
            }));
        }

        public void sleep()
        {
            Application.SetSuspendState(PowerState.Hibernate, false, false);
        }

        public MainWindow()
        {
            InitializeComponent();
            Devices.ItemsSource = _device;
            _udpClient = new UDPClientService(BootService._port, this);
            imgQRCode.Source = QRCodeService.getQRCode();
            _volumeService = new VolumeService();
        }
    }
}
