using WindowsConnect.Models;

namespace WindowsConnect.Interfaces
{
    public interface ICommandController
    {
        void SetVolume(int volume);
        void AddDevice(Device device);
        void Sleep();
        void PlayStepasSound();
        void RequestAddDevice(Device device);

        void VirtualTouchPadChanged(int x, int y, int action, int pointer);
    }
}
