using Newtonsoft.Json.Linq;
using System;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace WindowsConnect.Services
{
    public class SettingsService
    {
        public const int UDP_LISTEN_PORT = 5000;
        public const int UDP_SEND_PORT = 5001;
        public const int UDP_SEND_WITH_RECEIVE_PORT = 5002;

        //public const int TCP_LISTEN_PORT = 5002;
        public const int TCP_PORT = 5005;



        //Определяет, является ли адаптер физическим
        public static bool IsAdapterPhysical(string guid)
        {
            //create a management scope object
            var scope = new ManagementScope("\\\\.\\ROOT\\StandardCimv2");

            //create object query
            var query = new ObjectQuery($"SELECT * FROM MSFT_NetAdapter Where DeviceID=\"{guid}\"");

            //create object searcher
            var searcher = new ManagementObjectSearcher(scope, query);

            //get a collection of WMI objects
            var queryCollection = searcher.Get();

            //enumerate the collection.
            foreach (var m in queryCollection)
            {
                bool v = (bool) m["Virtual"];
                return !v;
            }
            return false;
        }

        //Получает все локальные IP-адреса
        public static IPAddress GetIpAddress()
        {
            var ifs = NetworkInterface.GetAllNetworkInterfaces();

            foreach (var interf in ifs)
            {
                var ipprop = interf.GetIPProperties();
                if (ipprop == null) continue;
                var unicast = ipprop.UnicastAddresses;
                if (unicast == null) continue;

                if (IsAdapterPhysical(interf.Id.ToString()))
                {
                    //находим первый Unicast-адрес
                    foreach (var addr in unicast)
                    {
                        if (addr.Address.AddressFamily != AddressFamily.InterNetwork) continue;
                        return addr.Address;
                    }
                }
            }
            return null;
        }


        public static JObject getHostInfo()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());

            var ipAddress = GetIpAddress();

            var json = new JObject();
            json["port"] = UDP_LISTEN_PORT;
            json["localIP"] = ipAddress.ToString();
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
