
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Markup;
using System.Windows.Threading;
using WindowsConnect.Models;

namespace WindowsConnect.Services
{
    public class TCPClientService
    {

        public static void SendMessage(string message, string ip, int port)
        {
            var tcpClient = new TcpClient(ip, SettingsService.TCP_SEND_PORT);
            var stream = tcpClient.GetStream();
            var data = Encoding.UTF8.GetBytes(message);
         
            stream.Write(data, 0, data.Length);
            stream.Close();
            tcpClient.Close();
        }

        public async static Task Receive()
        {
            await Task.Run(async () =>
            {
                var ipEndPoint = new IPEndPoint(IPAddress.Any, SettingsService.TCP_LISTEN_PORT);
                var listener = new TcpListener(ipEndPoint);
                listener.Start();

                while (true)
                {
                    try
                    {
                        var handler = listener.AcceptTcpClient();
                        var stream = handler.GetStream();

                        byte[] buffer = new byte[1000000];
                        int count = 0;
                        int bytesReceived = 0;
                        do
                        {
                            bytesReceived = await stream.ReadAsync(buffer, count, buffer.Length - count);
                            count += bytesReceived;
                        }
                        while (stream.DataAvailable);

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

                                break;
                        }
                    }
                    catch (Exception e)
                    {

                    }
                }
            });
           
        }
    }
}










/*

Console.WriteLine(num);
var messageStr = message.ToString();
var data = Encoding.UTF8.GetBytes(messageStr);


SaveByteArrayToFileWithBinaryWriterCreate(data, "documrnt.docx");

dynamic jsonObj = JsonConvert.DeserializeObject(message.ToString());
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

        var d = Encoding.UTF8.GetBytes(f.Data);


        SaveByteArrayToFileWithBinaryWriter(d, f.Name);
        break;
}
*/



/*var handler = listener.AcceptTcpClient();
var stream = handler.GetStream();

byte[] buffer = new byte[700000];
int count = 0;
int bytesReceived = 0;
do
{
    bytesReceived = await stream.ReadAsync(buffer, count, buffer.Length - count);
    count += bytesReceived;
}
while (stream.DataAvailable);

File.WriteAllBytes("testFile", buffer);*/