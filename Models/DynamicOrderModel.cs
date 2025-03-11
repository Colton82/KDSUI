using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KDSUI.Models
{
    /// <summary>
    /// This model represents an order and can be created dynamically allowing for a variable number of items and properties
    /// </summary>
    public class DynamicOrderModel
    {
        public required int Id { get; set; }
        public required string CustomerName { get; set; }
        public string Station { get; set; }
        public string Timestamp { get; set; }
        public Dictionary<string, object> Items { get; set; }

        public override string ToString()
        {
            var itemsFormatted = string.Join("\n  ",
                Items.Select(item => $"{item.Key}:\n{FormatItem(item.Value, 6)}"));

            string result = $"Order ID: {Id}\nCustomer: {CustomerName}\nStation: {Station}\nItems:\n  {itemsFormatted}\n Timestamp: {Timestamp}";

            // Ensure any accidental brackets are removed before returning the string
            result.Replace("{", "").Replace("}", "");
            return result;
        }

        /// <summary>
        /// Formats dictionary values into an indented, readable string without brackets.
        /// </summary>
        private static string FormatItem(object item, int indentLevel = 6)
        {
            string indent = new string(' ', indentLevel); // Indentation for readability

            if (item is Dictionary<string, object> subProperties)
            {
                // Format properties correctly without brackets
                return string.Join("\n", subProperties
                    .Where(prop => prop.Value != null && !string.IsNullOrWhiteSpace(prop.Value.ToString())) // Remove empty values
                    .Select(prop => $"{indent}{prop.Key} : {FormatItem(prop.Value, indentLevel + 2)}")); // Recursive call for deeper nesting
            }

            return $"{indent}{item}"; // For simple values (like a string or number)
        }
    }
}
