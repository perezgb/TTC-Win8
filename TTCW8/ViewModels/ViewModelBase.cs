using System.Collections.ObjectModel;
using System.ComponentModel;

namespace TTCW8.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class MainViewModel : ViewModelBase
    {
        public ObservableCollection<string> Predictions { get; set; }

        public MainViewModel()
        {
            Predictions = new ObservableCollection<string>();
        }
    }
}