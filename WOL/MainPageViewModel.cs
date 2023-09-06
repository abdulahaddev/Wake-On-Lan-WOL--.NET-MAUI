using System.ComponentModel;

namespace WOL
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        private bool _isDeviceOn;
        private bool _isLoaderOn;
        private bool _isVisibleStatusLabel = true;
        private string _currentStatus = string.Empty;

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

        public string CurrentStatus
        {
            get { return _currentStatus; }
            set
            {
                if (_currentStatus != value)
                {
                    _currentStatus = value;
                    OnPropertyChanged(nameof(CurrentStatus));
                }
            }
        }

        public bool IsVisibleStatusLabel
        {
            get { return _isVisibleStatusLabel; }
            set
            {
                if (_isVisibleStatusLabel != value)
                {
                    _isVisibleStatusLabel = value;
                    OnPropertyChanged(nameof(IsVisibleStatusLabel));
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
