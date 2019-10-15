using BlazorMart.Server;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlazorMart.Client
{
    public class Cart
    {
        private readonly Inventory.InventoryClient _inventoryClient;
        private readonly List<CartRow> _rows = new List<CartRow>();

        public Cart(Inventory.InventoryClient inventoryClient)
        {
            _inventoryClient = inventoryClient;
        }

        public IReadOnlyList<CartRow> Rows => _rows;

        public bool HasAnyProducts => _rows.Any(r => r.Product != null);

        public decimal GrandTotal => Rows
            .Where(r => r.Product != null)
            .Sum(r => r.Quantity * r.Product.Price / 100m);

        public async Task AddItemAsync(string ean)
        {
            var existingRow = Rows.SingleOrDefault(r => r.EAN == ean);
            if (existingRow != null)
            {
                // We already have a row for this EAN
                // Just increment the quantity
                existingRow.Quantity++;
            }
            else
            {
                // This is a new EAN
                // First add the row, before the product data is loaded
                var row = new CartRow { EAN = ean, Quantity = 1 };
                _rows.Add(row);

                // Now fetch
                // Simple retry loop
                for (var i = 0; i < 20; i++)
                {
                    try
                    {
                        row.Product = await FetchProductData(ean);
                        break;
                    }
                    catch (HttpRequestException)
                    {
                        await Task.Delay(5000);
                    }
                }

                if (row.Product == null)
                {
                    // Can't find this product; remove row from cart
                    _rows.Remove(row);
                }
            }
        }

        public void RemoveItem(string ean)
        {
            var existingRow = Rows.SingleOrDefault(r => r.EAN == ean);
            if (existingRow != null)
            {
                existingRow.Quantity--;
                if (existingRow.Quantity <= 0)
                {
                    _rows.Remove(existingRow);
                }
            }
        }

        private async Task<Product> FetchProductData(string ean)
        {
            var request = new ProductDetailsRequest { EAN = ean };
            var response = await _inventoryClient.ProductDetailsAsync(request);
            return response.Product;
        }
    }
}
