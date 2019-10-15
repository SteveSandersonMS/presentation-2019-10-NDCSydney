using Microsoft.AspNetCore.Components.Builder;
using Microsoft.Extensions.DependencyInjection;
using BlazorMart.Server;
using System.Net.Http;
using System;

namespace BlazorMart.Client
{
    public class Startup
    {
        public const string BackendUrl = "https://localhost:5001";

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped(serviceProvider =>
            {
                var httpClient = new HttpClient { BaseAddress = new Uri(BackendUrl) };
                return new Inventory.InventoryClient(new GrpcWebCallInvoker(httpClient));
            });

            services.AddScoped<Cart>();
        }

        public void Configure(IComponentsApplicationBuilder app)
        {
            app.AddComponent<App>("app");
        }
    }
}
