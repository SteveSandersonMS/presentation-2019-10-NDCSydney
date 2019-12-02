using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using BlazorMart.Server.Models;
using Grpc.Core;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BlazorMart.Server
{
    public class InventoryService : Inventory.InventoryBase
    {
        private readonly IMongoCollection<Product> _products;

        public InventoryService(IBlazorMartDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _products = database.GetCollection<Product>(settings.InventoryCollectionName);
        }

        public override async Task<AutocompleteReply> Autocomplete(AutocompleteRequest request, ServerCallContext context)
        {
            var result = new AutocompleteReply();

            if (!string.IsNullOrEmpty(request.SearchQuery))
            {
                // TODO: Santize - don't pass through arbitrary regexes
                var filter = Builders<Product>.Filter.Regex(p => p.Name, new BsonRegularExpression("^" + request.SearchQuery, "i"));
                var matches = await _products.Find(filter).Limit(10)
                    .Project(p => new AutocompleteItem { EAN = p.EAN, Name = p.Name })
                    .ToListAsync();
                result.Items.AddRange(matches);
            }

            return result;
        }

        public override async Task<ProductDetailsResponse> ProductDetails(ProductDetailsRequest request, ServerCallContext context)
        {
            await Task.Delay(500); // Look busy

            // TODO: Use index here
            var filter = Builders<Product>.Filter.Eq(p => p.EAN, request.EAN);
            var product = await _products.Find(filter).SingleAsync();
            return new ProductDetailsResponse { Product = product };
        }
    }

    partial class Product
    {
        public ObjectId Id { get; set; } // Satisfy Mongo driver
    }
}
