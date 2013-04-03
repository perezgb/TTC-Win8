using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using NextbusNET;

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
        private readonly INextbusAsyncClient _client;

        public ObservableCollection<string> Predictions { get; set; }

        public MainViewModel()
        {
            _client = new NextbusAsyncClient();
            Predictions = new ObservableCollection<string>();
        }

        public async void Load()
        {
            var predictions = await _client.GetPredictions("ttc", "5534", "111");
            Predictions.Clear();
            foreach (var prediction in predictions)
            {
                Predictions.Add(TimeSpan.FromSeconds(prediction.Seconds).ToString("mm\\:ss"));
            }
        }
    }
}