
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Threading;

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

                        // буфер для получения данных
                        var buffer = new byte[512];
                        // StringBuilder для склеивания полученных данных в одну строку
                        var response = new StringBuilder();
                        int bytes;  // количество полученных байтов
                        do
                        {
                            // получаем данные
                            bytes = await stream.ReadAsync(buffer, 0, buffer.Length);
                            // преобразуем в строку и добавляем ее в StringBuilder
                            response.Append(Encoding.UTF8.GetString(buffer, 0, bytes));
                        }
                        while (bytes > 0); // пока данные есть в потоке 

                        // выводим данные на консоль
                        Console.WriteLine(response);
                        SaveByteArrayToFileWithBinaryWriterCreate(Encoding.UTF8.GetBytes(response.ToString()), "documrnt.docx");


                    }
                    catch (Exception e)
                    {

                    }
                }
            });
           
        }

        public static void SaveByteArrayToFileWithBinaryWriterCreate(byte[] data, string filePath)
        {
            var writer = new BinaryWriter(File.OpenWrite(filePath));
            writer.Write(data);
            writer.Close();
        }

    }
}













/*                        var handler = listener.AcceptTcpClient();
                        var tcpStream = handler.GetStream();

                        byte[] buffer = new byte[512];

                        Console.WriteLine("ку");
                        var message = new StringBuilder();

                        int c;
                        int num = 0;
                        do
                        {
                            num++;
                            c = tcpStream.Read(buffer, 0, buffer.Length);
                            var str = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                            message.Append(str);

                        }
                        while (c > 0);

                        Console.WriteLine(num);
                        var messageStr = message.ToString();
                        var data = Encoding.UTF8.GetBytes(messageStr);


                        SaveByteArrayToFileWithBinaryWriterCreate(data, "documrnt.docx");

*//*                        dynamic jsonObj = JsonConvert.DeserializeObject(message.ToString());
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
                        }*/