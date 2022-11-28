using Newtonsoft.Json;
using System;
using System.IO;
namespace WindowsConnect.Services
{
    public class Database
    {
        public const string DEVICE_PATH = "devices";

        public static void Save<T>(string path, T obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            File.WriteAllText(path, json);
        }

        public static T Get<T>(string path)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(File.ReadAllText(path));
            }catch(FileNotFoundException e)
            {
                File.Create(path);
            }
            catch(Exception e)
            {
                
            }
            return default(T);
        }
    }
}
