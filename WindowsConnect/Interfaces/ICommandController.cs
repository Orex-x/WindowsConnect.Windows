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
        void setVolume(int volume);
        void addDevice(Device device);
        void sleep();
    }
}
