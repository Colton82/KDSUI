using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KDSUI.Pages
{
    /// <summary>
    /// Interaction logic for Dashboard.xaml
    /// </summary>
    public partial class Dashboard : Page
    {
        public Dashboard()
        {
            InitializeComponent();
            LoadListBox();
        }

        private void EditLayout_Click(object sender, RoutedEventArgs e)
        {
            EditLayout editLayout = new EditLayout();
            Application.Current.MainWindow.Content = editLayout;
        }

        /// <summary>
        /// Asynchronously loads the list box with the stations
        /// </summary>
        /// <returns></returns>
        public async Task LoadListBox()
        {
            await LayoutManager.GetStationsAsync();
            StationsList.ItemsSource = LayoutManager.Stations;
        }
    }
}
