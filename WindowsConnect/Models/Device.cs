using System;
using System.ComponentModel;

namespace WindowsConnect.Models
{
    public class Device : INotifyPropertyChanged
    {
        private string _name;

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        private string _ip;
        public string IP
        {
            get { return _ip; }
            set
            {
                _ip = value;
                OnPropertyChanged("IP");
            }
        }

        private int _port;
        public int Port
        {
            get { return _port; }
            set
            {
                _port = value;
                OnPropertyChanged("Port");
            }
        }


        private DateTime _dateConnect;

        public DateTime DateConnect
        {
            get { return _dateConnect; }
            set
            {

                _dateConnect = value;
                OnPropertyChanged("DateConnect");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
