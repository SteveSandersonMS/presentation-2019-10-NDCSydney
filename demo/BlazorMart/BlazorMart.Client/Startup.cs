using Microsoft.AspNetCore.Components.Builder;
using Microsoft.Extensions.DependencyInjection;
using BlazorMart.Server;
using System.Net.Http;
using System;

namespace BlazorMart.Client
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped(serviceProvider =>
            {
                var httpClient = serviceProvider.GetRequiredService<HttpClient>();
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
