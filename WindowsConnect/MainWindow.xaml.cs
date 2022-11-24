﻿using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
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
        private Device _device;


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
                if(_device != null)
                {

                    UDPClientService.SendMessage("200", device.IP, SettingsService.UDP_LISTEN_PORT);
                    _tcpClient = new TCPClientService(this);
                    txtDeviceName.Text = device.Name;
                    txtDeviceStatus.Text = "подключен";
                    sendWallpaper(device);
                }
                else
                {
                    PlayStepasSound();
                    var result = MessageBox.Show($"Устройство {device.Name} запрашевает подключение.\n " +
                        $"Подключить данное устройство?", "Добавление устройства", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        _device = device;
                        Database.Save(Database.DEVICE_PATH, device);
                        UDPClientService.SendMessage("200", device.IP, SettingsService.UDP_LISTEN_PORT);
                        _tcpClient = new TCPClientService(this);
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
            _device = null;
            _tcpClient.Dispose();
            Dispatcher.Invoke(new Action(() =>
            {
                txtDeviceName.Text = "";
            }));
            
        }

        public MainWindow()
        {
            InitializeComponent();

            _udpClient = new UDPClientService(this);
            imgQRCode.Source = QRCodeService.getQRCode();
            _volumeService = new VolumeService();
            AutoConnect();
        }

        public async void AutoConnect()
        {
            await Task.Run(() => {
                var device = Database.Get<Device>(Database.DEVICE_PATH);
                if (device != null)
                {
                    _device = device;
                   
                    Dispatcher.Invoke(new Action(() =>
                    {
                        txtDeviceName.Text = _device.Name;
                    }));
                    while (true)
                    {
                        if (TCPClientService.CheckConnection(device))
                        {
                            Dispatcher.Invoke(new Action(() =>
                            {
                                txtDeviceStatus.Text = "Шлем запрос на подключение...";
                            }));
                            var command = CommandHelper.CreateCommand(Command.OpenConnection, SettingsService.getHostInfo());
                            UDPClientService.SendMessage(command, device.IP, SettingsService.UDP_SEND_PORT);
                            break;
                        }
                        else
                        {
                            Dispatcher.Invoke(new Action(() =>
                            {
                                txtDeviceStatus.Text = "Не в сети";
                            }));
                        }
                            
                    }
                }
            });
        }

        private void Button_Click_Open_Folder(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer", "data\\");
        }

        private void Button_Click_Close_Connection(object sender, RoutedEventArgs e)
        {
            CloseConnection();
        }
    }
}