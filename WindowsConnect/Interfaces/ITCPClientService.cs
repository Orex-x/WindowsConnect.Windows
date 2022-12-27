namespace WindowsConnect.Interfaces
{
    public interface ITCPClientService : IException
    {
        void SetProgress(int progress);
        void CloseConnection();
        void OpenConnection();
    }
}
