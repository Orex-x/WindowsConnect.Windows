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

namespace WindowsConnect.Services
{
    public class QRCodeService
    {
        public static BitmapImage getQRCode()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            var hostIP = host.AddressList.ToList()
                .FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);
            var json = new JObject();
            json["port"] = BootService._port;
            json["ip"] = hostIP.ToString();
            json["name"] = host.HostName;
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
