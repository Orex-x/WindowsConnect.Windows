using Newtonsoft.Json;
using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using WindowsConnect.Interfaces;
using WindowsConnect.Models;

namespace WindowsConnect.Services
{
    public class UDPClientService 
    {
        private UdpClient _receiver;
        private ICommandController _commandController;

        public UdpClient getUdpClient()
        {
            return _receiver;
        }

        public UDPClientService(ICommandController commandController)
        {
            _commandController = commandController;
            _receiver = new UdpClient(SettingsService.UDP_LISTEN_PORT);
            _receiver.JoinMulticastGroup(IPAddress.Parse("230.0.0.0"));
            var receiveThread = new Thread(new ThreadStart(ReceiveMessage));
            receiveThread.Start();
        }

        public void Close()
        {
            _receiver.Close();
        }

        public static void SendMessage(byte[] data, string ip, int port)
        {
            var udp = new UdpClient();

            int intValue = data.Length;
            byte[] intBytes = BitConverter.GetBytes(intValue);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(intBytes);

            udp.Send(intBytes, intBytes.Length, ip, port);
            udp.Send(data, data.Length, ip, port);
            udp.Close();
        } 
        
        public static void SendMessage(string message, string ip, int port)
        {
            var data = Encoding.UTF8.GetBytes(message);
            var udp = new UdpClient();

            int intValue = data.Length;
            byte[] intBytes = BitConverter.GetBytes(intValue);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(intBytes);

            udp.Send(intBytes, intBytes.Length, ip, port);
            udp.Send(data, data.Length, ip, port);
            udp.Close();
        }

        public static string SendMessageWithReceive(string message, string ip)
        {
            try
            {
                IPEndPoint remoteIp = null;

                var data = Encoding.UTF8.GetBytes(message);
                var udp = new UdpClient(ip, SettingsService.UDP_SEND_PORT);

                int intValue = data.Length;
                byte[] intBytes = BitConverter.GetBytes(intValue);

                if (BitConverter.IsLittleEndian)
                    Array.Reverse(intBytes);

                udp.Send(intBytes, intBytes.Length);
                udp.Send(data, data.Length);

                udp.Close();

                udp = new UdpClient(SettingsService.UDP_SEND_WITH_RECEIVE_PORT);

                byte[] receiveData = udp.Receive(ref remoteIp);

                message = Encoding.UTF8.GetString(receiveData, 4, receiveData.Length - 4);

                udp.Close();

                return message;
            }
            catch(Exception ex)
            {

            }
            return "";
        }


        private void ReceiveMessage()
        {
            IPEndPoint remoteIp = null;
            try
            {
                while (true)
                {
                    byte[] data = _receiver.Receive(ref remoteIp);

                    int command = BitConverter.ToInt32(data, 0);
       
                    try
                    {
                        string message = "";
                        dynamic jsonObj;
                        Device device = null;
                        switch (command)
                        {
                            case Command.VirtualTouchPadChanged:

                                int x = BitConverter.ToInt32(data, 16);
                                int y = BitConverter.ToInt32(data, 12);
                                int a = BitConverter.ToInt32(data, 8);
                                int p = BitConverter.ToInt32(data, 4);

                                _commandController.VirtualTouchPadChanged(x, y, a, p);
                                break;

                            case Command.ChangeVolume:
                                message = Encoding.UTF8.GetString(data, 4, data.Length - 4);
                                jsonObj = JsonConvert.DeserializeObject(message);

                                int volume = (int) jsonObj;
                                _commandController.SetVolume(volume);
                                break;
                            case Command.Sleep:
                                _commandController.Sleep();
                                break;
                            case Command.PlayStepasSound:
                                _commandController.PlayStepasSound();
                                break;
                            case Command.RequestConnectDevice:
                                message = Encoding.UTF8.GetString(data, 4, data.Length - 4);
                                jsonObj = JsonConvert.DeserializeObject(message);
                                device = new Device()
                                {
                                    Name = jsonObj["Name"],
                                    IP = jsonObj["IP"],
                                    Port = SettingsService.UDP_SEND_PORT,
                                    DateConnect = DateTime.Now
                                };
                                _commandController.RequestConnectDevice(device);
                                break;
                            case Command.RequestAddDevice:
                                message = Encoding.UTF8.GetString(data, 4, data.Length - 4);
                                jsonObj = JsonConvert.DeserializeObject(message);
                                device = new Device()
                                {
                                    Name = jsonObj["Name"],
                                    IP = jsonObj["IP"],
                                    Port = SettingsService.UDP_SEND_PORT,
                                    DateConnect = DateTime.Now
                                };
                                _commandController.RequestAddDevice(device);
                                break;

                          
                            default:
                                
                                break;
                        }
                    }
                    catch(Exception ex)
                    {

                    }
                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
