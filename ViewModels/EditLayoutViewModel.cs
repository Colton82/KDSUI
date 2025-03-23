using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using System.Windows;
using KDSUI.Windows;
using KDSUI.Pages;
using System.Linq;

namespace KDSUI.ViewModels
{
    /// <summary>
    /// ViewModel for the Edit Layout page.
    /// Manages the list of kitchen stations and supports adding, removing, reordering, and saving stations.
    /// </summary>
    public class EditLayoutViewModel : INotifyPropertyChanged
    {
        private string _draggedStation;

        // Prevents station layout editing if any station has active orders
        public bool CanEditStations => !LayoutManager.Stations.Any(station => OrderManager.GetOrdersForStation(station).Count > 0);

        // Collection of station names bound to the layout editor UI
        public ObservableCollection<string> Stations { get; } = new ObservableCollection<string>();

        // Commands used for binding actions in the view
        public ICommand AddStationCommand { get; }
        public ICommand RemoveStationCommand { get; }
        public ICommand SaveLayoutCommand { get; }
        public ICommand DragStationCommand { get; }
        public ICommand DropStationCommand { get; }
        public ICommand GoBackCommand { get; }

        // Constructor sets up command bindings and loads the initial list of stations
        public EditLayoutViewModel()
        {
            AddStationCommand = new RelayCommand(AddStation);
            RemoveStationCommand = new RelayCommand<string>(RemoveStation);
            SaveLayoutCommand = new RelayCommand(SaveLayout);
            GoBackCommand = new RelayCommand(GoBack);
            DragStationCommand = new RelayCommand<string>(StartDrag);
            DropStationCommand = new RelayCommand<int>(Drop);

            LoadStations();
        }

        // Loads the current layout from LayoutManager and syncs it into the ViewModel
        private async void LoadStations()
        {
            await LayoutManager.GetStationsAsync();

            Stations.Clear();
            foreach (var station in LayoutManager.Stations)
            {
                Stations.Add(station);
            }

            OnPropertyChanged(nameof(Stations));
        }

        // Opens a dialog to input a new station name and adds it to the list
        private void AddStation()
        {
            var addStationWindow = new AddStationWindow
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            bool? result = addStationWindow.ShowDialog();

            if (result == true && !string.IsNullOrWhiteSpace(addStationWindow.StationName))
            {
                Stations.Add(addStationWindow.StationName);
                SaveLayout();
            }
        }

        // Removes a station from the list and updates the saved layout
        private void RemoveStation(string stationName)
        {
            if (!string.IsNullOrEmpty(stationName) && Stations.Contains(stationName))
            {
                Stations.Remove(stationName);
                SaveLayout();
            }
        }

        // Saves the current layout to the LayoutManager and persists it to the backend
        private void SaveLayout()
        {
            LayoutManager.Stations = new ObservableCollection<string>(Stations.ToList());
            LayoutManager.SaveStationsAsync();
        }

        // Navigates back to the main dashboard screen
        private void GoBack()
        {
            Application.Current.MainWindow.Content = new Dashboard();
        }

        // Stores the name of the station currently being dragged
        public void StartDrag(string stationName)
        {
            _draggedStation = stationName;
        }

        // Reorders stations based on drag-and-drop target index
        public void Drop(int targetIndex)
        {
            if (!CanEditStations)
            {
                MessageBox.Show("Cannot edit stations while orders are in progress.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!string.IsNullOrEmpty(_draggedStation) && targetIndex >= 0 && targetIndex < Stations.Count)
            {
                int oldIndex = Stations.IndexOf(_draggedStation);

                if (oldIndex >= 0 && oldIndex != targetIndex)
                {
                    Stations.Move(oldIndex, targetIndex);
                    SaveLayout();
                }
            }
        }

        // Notifies the UI when a property has changed
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
