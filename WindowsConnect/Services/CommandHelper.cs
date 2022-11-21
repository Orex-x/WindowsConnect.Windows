using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsConnect.Services
{
    public enum Command
    {
        setHostInfo,
        setWallpaper,
        saveFile,
        virtualSingleTouchDown,
        virtualSingleTouchUp,
        virtualSingleTouchMove,
        virtualSingleTouchRightClick,
        virtualMultiTouchDown,
        virtualMultiTouchUp,
        virtualMultiTouchMove
    }

    public class CommandHelper
    {
        public static string createCommand(Command command, object obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            return "{\"command\" : " + "\"" + command + "\"" + ", \"value\" : " + json + "}";
        }
    }
}
