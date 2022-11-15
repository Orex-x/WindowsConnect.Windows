namespace WindowsConnect.Interfaces
{
    public interface ITCPClientService : IException
    {
        void setProgress(int progress);
        void resetProgress();
    }
}
