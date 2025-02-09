using KDSUI.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace KDSUI.UserControls
{
    /// <summary>
    /// Order cards are used to display order information, and are used in the StationView
    /// </summary>
    public partial class OrderCard : UserControl
    {
        public OrderModel Order => DataContext as OrderModel;

        public OrderCard()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Bump the order to the next station
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BumpOrder_Click(object sender, RoutedEventArgs e)
        {
            if (Order == null) return;

            var currentStationIndex = LayoutManager.Stations.IndexOf(Order.Station);
            if (currentStationIndex < LayoutManager.Stations.Count - 1)
            {
                // Move order to the next station
                string nextStation = LayoutManager.Stations[currentStationIndex + 1];
                OrderManager.UpdateOrderStation(Order, nextStation);
            }
            else
            {
                // Order is completed, remove from system
                OrderManager.RemoveOrder(Order);
            }
        }
    }

}
