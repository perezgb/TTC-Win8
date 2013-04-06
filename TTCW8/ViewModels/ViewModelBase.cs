using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using NextbusNET;
using NextbusNET.Model;
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

        private Route _selectedRoute;

        private Direction _selectedDirection;
        
        private RouteConfig _routeConfig;
        
        private Stop _selectedStop;

        public ObservableCollection<PredictionViewModel> Predictions { get; set; }

        public ObservableCollection<Route> Routes { get; set; }

        public ObservableCollection<Direction> Directions { get; set; }

        public ObservableCollection<Stop> Stops { get; set; }

        public Route SelectedRoute
        {
            get { return _selectedRoute; }
            set
            {
                _selectedRoute = value;
                OnPropertyChanged("SelectedRoute");
            }
        }

        public Direction SelectedDirection
        {
            get { return _selectedDirection; }
            set
            {
                _selectedDirection = value;
                OnPropertyChanged("SelectedDirection");
            }
        }

        public Stop SelectedStop
        {
            get { return _selectedStop; }
            set
            {
                _selectedStop = value;
                OnPropertyChanged("SelectedStop");
            }
        }

        public MainViewModel()
        {
            _client = new NextbusAsyncClient();
            Routes = new ObservableCollection<Route>();
            Directions = new ObservableCollection<Direction>();
            Stops = new ObservableCollection<Stop>();
            Predictions = new ObservableCollection<PredictionViewModel>();
            PropertyChanged += MainViewModel_PropertyChanged;
            _timer = new DispatcherTimer();
            _timer.Tick += TimerCallback;
            _timer.Interval = TimeSpan.FromSeconds(5);
            _timer.Start();
        }

        private async void MainViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedRoute")
            {
                _routeConfig = await _client.GetRouteConfig("ttc", SelectedRoute.Tag);
                Directions.Clear();
                Stops.Clear();
                Predictions.Clear();
                foreach (var direction in _routeConfig.Directions)
                {
                    Directions.Add(direction);
                }
            }
            else if (e.PropertyName == "SelectedDirection" && SelectedDirection != null)
            {
                Stops.Clear();
                foreach (var stop in SelectedDirection.Stops)
                {
                    Stops.Add(stop);
                }
            }
            else if (e.PropertyName == "SelectedStop")
            {
                UpdatePredictions();
            }
        }

        public async void Load()
        {
            var routes = await _client.GetRoutes("ttc");
            foreach (var route in routes)
            {
                Routes.Add(route);
            }
        }

        public async void UpdatePredictions()
        {
            var selectedStop = SelectedStop;
            if (selectedStop == null) return;

            var predictions = await _client.GetPredictions("ttc", SelectedStop.Tag, SelectedRoute.Tag);
            Predictions.Clear();
            foreach (var prediction in predictions.OrderBy(x => x.Seconds))
            {
                var viewModel = new PredictionViewModel(prediction.Seconds);
                Predictions.Add(viewModel);
            }
        }

        private void TimerCallback(object sender, object e)
        {
            var selectedStop = SelectedStop;
            if (selectedStop != null)
            {
                UpdatePredictions();
            }
        }
    }
}