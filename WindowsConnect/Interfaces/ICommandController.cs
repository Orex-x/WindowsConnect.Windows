using WindowsConnect.Models;

namespace WindowsConnect.Interfaces
{
    public interface ICommandController
    {
        void SetVolume(int volume);
        void RequestConnectDevice(Device device);
        void Sleep();
        void PlayStepasSound();
        void RequestAddDevice(Device device);
        void VirtualTouchPadChanged(int x, int y, int action, int pointer);
        void ClickButtonCSCTE(int code);

        void DownButtonCSCTE(int code);
        void UpButtonCSCTE(int code);
    }
}
