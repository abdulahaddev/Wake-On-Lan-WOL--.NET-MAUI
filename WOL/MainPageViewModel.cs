using System.ComponentModel;

namespace WOL
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        private bool _isDeviceOn;
        private bool _isLoaderOn;

        public bool IsDeviceOn
        {
            get { return _isDeviceOn; }
            set
            {
                if (_isDeviceOn != value)
                {
                    _isDeviceOn = value;
                    OnPropertyChanged(nameof(IsDeviceOn));
                }
            }
        }

        public bool IsLoaderOn
        {
            get { return _isLoaderOn; }
            set
            {
                if (_isLoaderOn != value)
                {
                    _isLoaderOn = value;
                    OnPropertyChanged(nameof(IsLoaderOn));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
