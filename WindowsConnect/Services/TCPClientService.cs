using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using WindowsConnect.Interfaces;
using WindowsConnect.Models;

namespace WindowsConnect.Services
{
    public class TCPClientService : IDisposable
    {

        private ITCPClientService _tcpClientServiceListener;

        private readonly TcpClient _tcpClient;
        private readonly NetworkStream _stream;
        private readonly TcpListener _listener;

        #region Disposable

        bool disposed;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        private void Dispose(bool disposing)
        {
            if (disposed)
                throw new ObjectDisposedException(typeof(TCPClientService).FullName);

            disposed = true;
            _listener.Stop();

            if (_tcpClient.Connected) _stream.Close();
            if (disposing) _tcpClient.Dispose();
        }

        ~TCPClientService() => Dispose(false);

        #endregion

        public TCPClientService(ITCPClientService tcpClientServiceListener, Device device)
        {
            _tcpClientServiceListener = tcpClientServiceListener;
            _tcpClient = new TcpClient(device.IP, SettingsService.TCP_SEND_PORT);
            _stream = _tcpClient.GetStream();
            var ipEndPoint = new IPEndPoint(IPAddress.Any, SettingsService.TCP_LISTEN_PORT);
            _listener = new TcpListener(ipEndPoint);
            _listener.Start();
            Receive();
        }

        public void SendMessage(string message)
        {
            var data = Encoding.UTF8.GetBytes(message);
            _stream.Write(data, 0, data.Length);
        }

        private async Task Receive()
        {
            await Task.Yield();
            await Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        var handler = _listener.AcceptTcpClient();
                        var stream = handler.GetStream();

                        byte[] headerBuffer = new byte[4];
                        int bytesReceived = await stream.ReadAsync(headerBuffer, 0, 4);
                        if (bytesReceived != 4)
                            break;

                        if (BitConverter.IsLittleEndian)
                            Array.Reverse(headerBuffer);

                        int length = BitConverter.ToInt32(headerBuffer, 0);
                        _tcpClientServiceListener.Message($"пришел файл. Размер {length} байт");
                        _tcpClientServiceListener.resetProgress();
                        byte[] buffer = new byte[length];
                        int count = 0;

                        do
                        {
                            bytesReceived = await stream.ReadAsync(buffer, count, buffer.Length - count);
                            count += bytesReceived;
                            _tcpClientServiceListener.setProgress(getProgress(length, count));
                            if(count > length) break;
                        }
                        while (count != length);

                        string buffer_string = Encoding.UTF8.GetString(buffer);
                        dynamic jsonObj = JsonConvert.DeserializeObject(buffer_string.ToString());
                        string command = jsonObj["command"];

                        dynamic value = null;
                        switch (command)
                        {
                            case "saveFile":
                                value = jsonObj["value"];

                                var f = new MyFile()
                                {
                                    Name = value["name"],
                                    Data = value["data"]
                                };
                                File.WriteAllBytes(f.Name, f.Data);

                                var file = new FileInfo(f.Name);
                                _tcpClientServiceListener.Message($"Файл {f.Name} сохранен по пути {file.FullName}");
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        _tcpClientServiceListener.Exception(e);
                    }
                }
            });
           
        }

        public int getProgress(int sum, int value)
        {
            return value * 100 / sum;
        }
    }
}