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

        void VirtualSingleTouchDown(int x, int y);
        void VirtualSingleTouchUp(int x, int y);
        void VirtualSingleTouchMove(int x, int y);
        void VirtualSingleTouchLeftClick();
        void VirtualSingleTouchRightClick();

        void VirtualMultiTouchDown(int x, int y);
        void VirtualMultiTouchUp(int x, int y);
        void VirtualMultiTouchMove(int x, int y);
    }
}
