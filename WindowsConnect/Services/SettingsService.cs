using Newtonsoft.Json.Linq;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace WindowsConnect.Services
{
    public class SettingsService
    {
        public const int UDP_LISTEN_PORT = 5000;
        public const int UDP_SEND_PORT = 5001;

        public const int TCP_LISTEN_PORT = 5002;
        public const int TCP_SEND_PORT = 5003;

        public static JObject getHostInfo()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            var hostIP = host.AddressList.ToList()
                .FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);

            var json = new JObject();
            json["port"] = UDP_LISTEN_PORT;
            json["localIP"] = hostIP.ToString();
            json["name"] = host.HostName;
            json["macAddress"] = GetMACAddress();
            return json;

        }

        private static string GetMACAddress()
        {
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            string sMacAddress = string.Empty;
            IPInterfaceProperties properties = nics[0].GetIPProperties();
            sMacAddress = nics[0].GetPhysicalAddress().ToString();

            int y = 0;
            for (int i = 1; i <= 5; i++, y++)
            {
                sMacAddress = sMacAddress.Insert(i * 2 + y, ":");
            }

            return sMacAddress;
        }
    }
}
