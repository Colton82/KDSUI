using KDSUI.ViewModels;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for EditOrders.xaml
    /// </summary>
    public partial class EditOrders : Page
    {
        public EditOrders()
        {
            InitializeComponent();
            // Set the data context of the page to the EditOrdersViewModel
            this.DataContext = new EditOrdersViewModel();
        }
    }
}
