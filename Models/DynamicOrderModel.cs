using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Timers;

namespace KDSUI.Models
{
    public class DynamicOrderModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // Unique identifier for the order
        public int Id { get; set; }

        // Name of the customer who placed the order
        public string CustomerName { get; set; }

        // The station this order currently belongs to
        public string Station { get; set; }

        // Timestamp history for the order, tracking when it entered each station
        public string Timestamp { get; set; }

        private string _elapsedTime;

        // Human-readable string showing time since the order was created
        public string ElapsedTime
        {
            get => _elapsedTime;
            set
            {
                _elapsedTime = value;
                OnPropertyChanged();
            }
        }

        // Collection of items included in the order
        public ObservableCollection<OrderItem> Items { get; set; } = new ObservableCollection<OrderItem>();

        private System.Timers.Timer _timer;

        // Constructor initializes a repeating timer to calculate elapsed time
        public DynamicOrderModel()
        {
            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += UpdateElapsedTime;
            _timer.Start();
        }

        // Recalculates elapsed time every second by comparing current time to the order's start time
        private void UpdateElapsedTime(object sender, ElapsedEventArgs e)
        {
            DateTime? startTime = ExtractFirstTimestamp(Timestamp);

            if (startTime.HasValue)
            {
                var elapsed = DateTime.Now - startTime.Value;
                ElapsedTime = $"{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";
            }
        }

        // Extracts the very first valid DateTime from the timestamp string
        private DateTime? ExtractFirstTimestamp(string timestamp)
        {
            if (string.IsNullOrEmpty(timestamp))
                return null;

            var parts = timestamp
                .Split('|')
                .Select(p => p.Trim())
                .Where(p => DateTime.TryParse(p, out _))
                .ToList();

            return parts.Count > 0 && DateTime.TryParse(parts[0], out DateTime parsedTime)
                ? parsedTime
                : (DateTime?)null;
        }

        // Notifies the UI of changes to property values
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class OrderItem : INotifyPropertyChanged
    {
        private string _name;
        private ObservableCollection<ItemProperty> _properties;

        // Name of the item (e.g., "Burger", "Salad")
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        // Additional properties or modifiers for the item (e.g., "No onions", "Extra cheese")
        public ObservableCollection<ItemProperty> Properties
        {
            get => _properties;
            set
            {
                _properties = value;
                OnPropertyChanged();
            }
        }

        // Initializes the item with a name and optional list of properties
        public OrderItem(string name, List<ItemProperty> properties = null)
        {
            Name = name;
            Properties = new ObservableCollection<ItemProperty>(properties ?? new List<ItemProperty>());
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // Notifies the UI when a property changes
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ItemProperty : INotifyPropertyChanged
    {
        private string _key;
        private string _value;

        // The label or category of the property (e.g., "Temperature", "Sauce")
        public string Key
        {
            get => _key;
            set
            {
                _key = value;
                OnPropertyChanged();
            }
        }

        // The specific value for the key (e.g., "Hot", "Ranch")
        public string Value
        {
            get => _value;
            set
            {
                _value = value;
                OnPropertyChanged();
            }
        }

        // Initializes the property with a key-value pair
        public ItemProperty(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // Notifies the UI when a property changes
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
