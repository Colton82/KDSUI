using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KDSUI.Models
{
    /// <summary>
    /// Represents an order
    /// </summary>
    public class OrderModel
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public DateTime Timestamp { get; set; }
        public int Users_id { get; set; }

        public string? Station = InitializeOrder();

        /// <summary>
        /// Returns a string representation of the order
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"Order {Id} - {CustomerName} - {Station}";
        }

        public static string InitializeOrder()
        {
            try
            {
                return LayoutManager.Stations[0].ToString();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"No stations present");
                return "Error";
            }
        }
    }
}
