using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using Newtonsoft.Json.Linq;
using QRCoder;
using System.Net.NetworkInformation;

namespace WindowsConnect.Services
{
    public class QRCodeService
    {

        public static string GetMACAddress()
        {
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            string sMacAddress = string.Empty;
            IPInterfaceProperties properties = nics[0].GetIPProperties();
            sMacAddress = nics[0].GetPhysicalAddress().ToString();

            int y = 0;
            for(int i = 1; i<=5; i++, y++)
            {
                sMacAddress = sMacAddress.Insert(i*2 + y, ":");
            }

            return sMacAddress;
        }


        public static BitmapImage getQRCode()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            var hostIP = host.AddressList.ToList()
                .FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);


            var json = new JObject();
            json["port"] = BootService._port;
            json["ip"] = hostIP.ToString();
            json["name"] = host.HostName;
            json["macAddress"] = GetMACAddress();
            string jsonString = json.ToString();


            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(jsonString, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new QRCode(qrCodeData);
            var qrCodeImage = qrCode.GetGraphic(20);

            return ToBitmapImage(qrCodeImage);

        }


        public static BitmapImage ToBitmapImage(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }
    }
}
