using Newtonsoft.Json;
using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
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

        public UDPClientService(int port, ICommandController commandController)
        {
            _commandController = commandController;
            _receiver = new UdpClient(port);
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
            udp.Send(data, data.Length, ip, port);
            udp.Close();
        } 
        
        public static void SendMessage(string message, string ip, int port)
        {
            var data = Encoding.UTF8.GetBytes(message);
            var udp = new UdpClient();
            udp.Send(data, data.Length, ip, port);
            udp.Close();
        }


        private void ReceiveMessage()
        {
            IPEndPoint remoteIp = null;
            try
            {
                while (true)
                {
                    byte[] data = _receiver.Receive(ref remoteIp);
                    string message = Encoding.UTF8.GetString(data);
                    try
                    {
                        dynamic jsonObj = JsonConvert.DeserializeObject(message);
                        string command = jsonObj["command"];

                        dynamic value = null;
                        Device device = null;

                        switch (command)
                        {
                            case "changeVolume":
                                int volume = jsonObj["value"];
                                _commandController.setVolume(volume);
                                break;
                            case "sleep":
                                _commandController.sleep();
                                break;
                            case "playStepasSound":
                                _commandController.playStepasSound();
                                break;
                            case "addDevice":
                                value = jsonObj["value"];
                                device = new Device()
                                {
                                    Name = value["Name"],
                                    IP = value["IP"],
                                    Port = SettingsService.ClientPort,
                                    DateConnect = DateTime.Now
                                };
                                _commandController.addDevice(device);
                                break;
                            case "requestAddDevice":
                                value = jsonObj["value"];
                                device = new Device()
                                {
                                    Name = value["Name"],
                                    IP = value["IP"],
                                    Port = SettingsService.ClientPort,
                                    DateConnect = DateTime.Now
                                };
                                _commandController.requestAddDevice(device);
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
