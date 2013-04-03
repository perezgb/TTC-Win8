using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using NextbusNET;
using Windows.UI.Xaml;

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

    public class PredictionViewModel : ViewModelBase
    {
        private int _seconds;

        private readonly DispatcherTimer _timer;

        private string _eta;

        public PredictionViewModel(int seconds)
        {
            _seconds = seconds;
            UpdateEta();
            _timer = new DispatcherTimer {Interval = TimeSpan.FromSeconds(1)};
            _timer.Tick += (sender, o) => UpdateEta();
            _timer.Start();
        }

        private void UpdateEta()
        {
            if (_seconds > 0)
            {
                Eta = TimeSpan.FromSeconds(_seconds).ToString("mm\\:ss");
                _seconds--;
            }
        }

        public string Eta
        {
            get { return _eta; }
            private set
            {
                _eta = value;
                OnPropertyChanged("Eta");
            }
        }
    }

    public class MainViewModel : ViewModelBase
    {
        private readonly INextbusAsyncClient _client;

        private readonly DispatcherTimer _timer;

        public ObservableCollection<PredictionViewModel> Predictions { get; set; }

        public string SelectedRoute
        {
            get { return "111"; }
        }

        public MainViewModel()
        {
            _client = new NextbusAsyncClient();
            Predictions = new ObservableCollection<PredictionViewModel>();
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            _timer.Tick += (sender, o) => Load();
            _timer.Start();
        }

        public async void Load()
        {
            var predictions = await _client.GetPredictions("ttc", "5534", "111");
            Predictions.Clear();
            foreach (var prediction in predictions)
            {
                Predictions.Add(new PredictionViewModel(prediction.Seconds));
            }
        }
    }
}