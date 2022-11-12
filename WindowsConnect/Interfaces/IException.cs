using System;

namespace WindowsConnect.Interfaces
{
    public interface IException
    {
        void Exception(Exception ex);
        void Message(string message);
    }
}
