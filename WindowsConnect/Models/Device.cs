using System;
using System.ComponentModel;

namespace WindowsConnect.Models
{
    public class Device : INotifyPropertyChanged
    {
        public string _name;

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        public DateTime _dateConnect;

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
