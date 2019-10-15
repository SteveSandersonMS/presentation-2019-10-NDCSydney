using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Grpc.Core;

namespace BlazorMart.Server
{
    public class InventoryService : Inventory.InventoryBase
    {
        private static Product[] _products = JsonSerializer.Deserialize<Product[]>(
            File.ReadAllText("products.json"));

        public override Task<AutocompleteReply> Autocomplete(AutocompleteRequest request, ServerCallContext context)
        {
            var result = new AutocompleteReply();

            if (!string.IsNullOrEmpty(request.SearchQuery))
            {
                var matches = _products
                    .Where(p => p.Name.StartsWith(request.SearchQuery, StringComparison.CurrentCultureIgnoreCase))
                    .Select(p => new AutocompleteItem { EAN = p.EAN, Name = p.Name })
                    .Take(10); // Limit to this many results
                result.Items.AddRange(matches);
            }

            return Task.FromResult(result);
        }

        public override async Task<ProductDetailsResponse> ProductDetails(ProductDetailsRequest request, ServerCallContext context)
        {
            await Task.Delay(500); // Look busy
            var product = _products.FirstOrDefault(p => p.EAN == request.EAN);
            return new ProductDetailsResponse { Product = product };
        }
    }
}
