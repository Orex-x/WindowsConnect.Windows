using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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
                        switch (command)
                        {
                            case "changeVolume":
                                int volume = jsonObj["value"];
                                _commandController.setVolume(volume);
                                break;
                            case "sleep":
                                _commandController.sleep();
                                break;
                            case "addDevice":
                                dynamic value = jsonObj["value"];
                                var device = new Device()
                                {
                                    Name = value["Name"],
                                    DateConnect = DateTime.Now
                                };
                                _commandController.addDevice(device);
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
