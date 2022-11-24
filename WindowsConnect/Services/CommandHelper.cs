using Newtonsoft.Json;

namespace WindowsConnect.Services
{
    /*public enum Command
    {
        SetHostInfo,
        SetWallpaper,
        SaveFile,
        virtualSingleTouchDown,
        virtualSingleTouchUp,
        virtualSingleTouchMove,
        virtualSingleTouchRightClick,
        virtualMultiTouchDown,
        virtualMultiTouchUp,
        virtualMultiTouchMove,
        virtualSingleTouchHookMove
    }
*/

    public static class Command
    {
        public const int ChangeVolume = 0;
        public const int AddDevice = 1;
        public const int Sleep = 2;
        public const int RequestAddDevice = 3;
        public const int SetHostInfo = 4;
        public const int SetWallpaper = 5;
        public const int PlayStepasSound = 6;
        public const int SaveFile = 7;
        public const int VirtualTouchPadChanged = 8;
        public const int CloseConnection = 9;
        public const int OpenConnection = 10;

    }


    public class CommandHelper
    {
        public static string CreateCommand(int command, object obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            return "{\"command\" : " + "\"" + command + "\"" + ", \"value\" : " + json + "}";
        }
    }
}
