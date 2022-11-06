using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace WindowsConnect.Services
{
    public class TCPClientService
    {
        public static void SendMessage(string message, string ip, int port)
        {
            TcpClient tcpClient = new TcpClient(ip, 5002);
            NetworkStream stream = tcpClient.GetStream();
            var data = Encoding.UTF8.GetBytes(message);
         
            stream.Write(data, 0, data.Length);
            stream.Close();
            tcpClient.Close();
        }
    }
}
