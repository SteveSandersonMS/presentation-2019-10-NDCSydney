using BlazorMart.Client;
using BlazorMart.Server;
using Microsoft.AspNetCore.Components.Testing;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace BlazorMart.Test
{
    public class CartTests
    {
        private readonly TestHost host = new TestHost();
        private readonly Mock<Inventory.InventoryClient> inventoryClient = new Mock<Inventory.InventoryClient>(MockBehavior.Strict);

        public CartTests()
        {
            host.AddService(inventoryClient.Object);
            host.AddService(new Cart(inventoryClient.Object));
        }

        [Fact]
        public void InitiallyDisplaysNoItems()
        {
            var component = host.AddComponent<App>();
            Assert.Empty(component.FindAll(".cart-item"));
        }

        [Fact]
        public void CanAddItem_InitiallyShowsLoadingState()
        {
            // Arrange/Act
            var component = host.AddComponent<App>();
            CaptureProductDetailsRequest("123123123");

            // Act
            component.Find(".search input").Input("123123123");
            component.Find(".search form").Submit();

            // Assert: Initially displays loading state
            Assert.Collection(component.FindAll(".cart-item h3"),
                item => Assert.Equal("Loading...", item.InnerText));
        }

        [Fact]
        public void CanAddItem_DisplaysDataFromServerResponse()
        {
            // Arrange
            var component = host.AddComponent<App>();
            SimulateAddingItem(component, new Product { EAN = "123123123", Name = "Something cool", Price = 45678 });

            // Assert: one item row, displaying info from server
            Assert.Equal(1, component.FindAll(".cart-item").Count);
            Assert.Equal("Something cool", component.Find(".cart-item h3").InnerText);
            Assert.EndsWith("456.78", component.Find(".cart-item .price").InnerText);
        }

        [Fact]
        public void CanRemoveItem()
        {
            // When starting with one item in the cart...
            var component = host.AddComponent<App>();
            SimulateAddingItem(component, new Product { EAN = "123123123", Name = "Something cool", Price = 45678 });

            // Initially there's no "remove" overlay
            Assert.Null(component.Find(".overlay"));

            // You can click "Remove item" to display it
            component.Find(".remove-item").Click();
            Assert.NotNull(component.Find(".overlay"));
            Assert.Contains("Scan an item to remove", component.Find(".overlay").InnerText);
        
            // Then if you scan an item, it gets removed
            component.Find(".search input").Input("123123123");
            component.Find(".search form").Submit();
            Assert.Empty(component.FindAll(".cart-item"));

            // And now the "remove" overlay is gone
            Assert.Null(component.Find(".overlay"));
        }

        [Fact]
        public void CanCancelRemoveMode()
        {
            // When starting with one item in the cart, and in "remove" mode
            var component = host.AddComponent<App>();
            SimulateAddingItem(component, new Product { EAN = "123123123", Name = "Something cool", Price = 45678 });
            component.Find(".remove-item").Click();

            // You can click "cancel" to exit "remove" mode
            component.Find(".overlay button").Click();

            // And now the "remove" overlay is gone
            Assert.Null(component.Find(".overlay"));
        }

        private TaskCompletionSource<Product> CaptureProductDetailsRequest(string ean)
        {
            inventoryClient.SetupCall(
                c => c.AutocompleteAsync(It.IsAny<AutocompleteRequest>(), null, null, It.IsAny<CancellationToken>()));

            var productDetailsRequest = inventoryClient.SetupCall(c => c.ProductDetailsAsync(
                It.Is<ProductDetailsRequest>(r => r.EAN == ean),
                null,
                null,
                It.IsAny<CancellationToken>()));

            var tcs = new TaskCompletionSource<Product>();
            tcs.Task.ContinueWith(task =>
            {
                var response = new ProductDetailsResponse { Product = task.Result };
                productDetailsRequest.SetResult(response);
            });

            return tcs;
        }

        private void SimulateAddingItem(RenderedComponent<App> component, Product serverResponse)
        {
            var ean = serverResponse.EAN;
            var request = CaptureProductDetailsRequest(ean);
            component.Find(".search input").Input(ean);
            component.Find(".search form").Submit();

            // Act: Complete the response
            host.WaitForNextRender(() => request.SetResult(serverResponse));
        }
    }
}
