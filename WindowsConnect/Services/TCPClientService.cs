using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsConnect.DialogWindows;
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

        private readonly Channel<string> _channel;


        private readonly EndPoint _remoteEndPoint;

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

            if (_tcpClient.Connected)
            {
                _stream.Close();
                _tcpClient.Close();
            } 
            if (disposing) _tcpClient.Dispose();
        }

        ~TCPClientService() => Dispose(false);

        #endregion

        public TCPClientService(ITCPClientService tcpClientServiceListener, Device device)
        {
            /*
            _tcpClientServiceListener = tcpClientServiceListener;
            _tcpClient = new TcpClient(device.IP, SettingsService.TCP_PORT);
            _stream = _tcpClient.GetStream();

            var ipEndPoint = new IPEndPoint(IPAddress.Any, SettingsService.TCP_LISTEN_PORT);
            _listener = new TcpListener(ipEndPoint);
            _listener.Start();
            */
           
            _tcpClientServiceListener = tcpClientServiceListener;
            _listener = new TcpListener(IPAddress.Any, SettingsService.TCP_PORT);
            _listener.Start();
            _tcpClient = _listener.AcceptTcpClient();
            _stream = _tcpClient.GetStream();
            _remoteEndPoint = _tcpClient.Client.RemoteEndPoint;
            _channel = Channel.CreateUnbounded<string>();
            Receive();
        }

        public void SendMessage(string message)
        {
            var data = Encoding.UTF8.GetBytes(message);

            int intValue = data.Length;
            byte[] intBytes = BitConverter.GetBytes(intValue);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(intBytes);

            _stream.Write(intBytes, 0, intBytes.Length);
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
                       
                        byte[] headerBuffer = new byte[4];
                        int bytesReceived = await _stream.ReadAsync(headerBuffer, 0, 4);
                        if (bytesReceived != 4)
                            continue;

                        if (BitConverter.IsLittleEndian)
                            Array.Reverse(headerBuffer);

                        int length = BitConverter.ToInt32(headerBuffer, 0);
                       
                        byte[] buffer = new byte[length];
                        int count = 0;

                        do
                        {
                            bytesReceived = await _stream.ReadAsync(buffer, count, buffer.Length - count);
                            count += bytesReceived;
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
                                _tcpClientServiceListener.Message($"пришел файл. Размер {length} байт");
                                string name = value["name"];
                                //SendMessage("go");
                                await downloadFile("data\\" + name);
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

        public async Task downloadFile(string name)
        {
            await Task.Run( async () =>
            {
               
                byte[] headerBuffer = new byte[8];
                int bytesReceived = await _stream.ReadAsync(headerBuffer, 0, 8);
                if (bytesReceived != 8)
                    return;

                if (BitConverter.IsLittleEndian)
                    Array.Reverse(headerBuffer);

                long length = BitConverter.ToInt64(headerBuffer, 0);

                byte[] buffer = new byte[1024];
                long count = 0;

                using (FileStream fstream = new FileStream(name, FileMode.OpenOrCreate))
                {
                    do
                    {
                        // bytesReceived = await _stream.ReadAsync(buffer, count, buffer.Length - count);
                        bytesReceived = await _stream.ReadAsync(buffer, 0, buffer.Length);
                        fstream.Write(buffer, 0, bytesReceived);
                        count += bytesReceived;
                        int p = (int)getProgress(length, count);
                        _tcpClientServiceListener.setProgress(p);
                        if (count > length) break;

                    }
                    while (count != length);


                }
                //File.WriteAllBytes("data\\"+ name, buffer);
                _tcpClientServiceListener.resetProgress();
                var file = new FileInfo(name);
                _tcpClientServiceListener.Message($"Файл {name} сохранен по пути {file.FullName}");
            });
        }

        private void WriteBytes(Stream streamToRead, Stream streamToWrite)
        {
                byte[] buffer = new byte[1024];
                int nRead = 0;
                while ((nRead = streamToRead.Read(buffer, 0, buffer.Length)) > 0)
                {
                    streamToWrite.Write(buffer, 0, nRead);
                }
        }



        public long getProgress(long sum, long value)
        {
            return value * 100 / sum;
        }
    }
}