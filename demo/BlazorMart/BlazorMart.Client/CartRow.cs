using BlazorMart.Server;

namespace BlazorMart.Client
{
    public class CartRow
    {
        public string EAN { get; set; }

        public int Quantity { get; set; }

        public Product Product { get; set; }

        public string Key => $"{EAN}:{Quantity}";
    }
}
