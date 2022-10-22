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

namespace WindowsConnect
{
    public partial class MainWindow : Window, ICommandController
    {
        private UDPClientService _udpClient;
        private VolumeService _volumeService;

        protected override void OnClosed(EventArgs e)
        {
            _udpClient.Close();
            base.OnClosed(e);
        }

        public void setVolume(int volume)
        {
            _volumeService.setVolume(volume);
        }

        public MainWindow()
        {
            InitializeComponent();
            _udpClient = new UDPClientService(BootService._port, this);
            imgQRCode.Source = QRCodeService.getQRCode();
            _volumeService = new VolumeService();
        }
    }
}
