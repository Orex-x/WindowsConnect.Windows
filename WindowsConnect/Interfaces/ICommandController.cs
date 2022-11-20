using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
