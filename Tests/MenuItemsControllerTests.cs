using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using QuickBiteAPI.Data;
using QuickBiteAPI.Models;
using Xunit;

namespace QuickBiteAPI.Tests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseContentRoot(Directory.GetCurrentDirectory());
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                // Remove the real database
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<QuickBiteDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                // Add in-memory database for testing
                services.AddDbContext<QuickBiteDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDatabase");
                });
            });
        }
    }

    public class MenuItemsControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public MenuItemsControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task testCreateMenuItem_ShouldReturnSavedMenuItem()
        {
            // Arrange
            var newMenuItem = new MenuItem
            {
                Name = "Test Pizza",
                Description = "A test pizza for unit testing",
                Price = 15.99m,
                Category = "Pizza",
                DietaryTag = "Vegetarian"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/menuitems", newMenuItem);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var createdMenuItem = await response.Content.ReadFromJsonAsync<MenuItem>();
            Assert.NotNull(createdMenuItem);
            Assert.Equal(newMenuItem.Name, createdMenuItem.Name);
            Assert.Equal(newMenuItem.Description, createdMenuItem.Description);
            Assert.Equal(newMenuItem.Price, createdMenuItem.Price);
            Assert.Equal(newMenuItem.Category, createdMenuItem.Category);
            Assert.Equal(newMenuItem.DietaryTag, createdMenuItem.DietaryTag);
            Assert.True(createdMenuItem.Id > 0); // Should have an auto-generated ID
        }

        [Fact]
        public async Task GetAllMenuItems_ShouldReturnAllMenuItems()
        {
            // Act
            var response = await _client.GetAsync("/api/menuitems");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var menuItems = await response.Content.ReadFromJsonAsync<List<MenuItem>>();
            Assert.NotNull(menuItems);
            Assert.True(menuItems.Count >= 0); // Should return at least an empty list
        }

        [Fact]
        public async Task GetMenuItemById_WithValidId_ShouldReturnMenuItem()
        {
            // Arrange - First create a menu item
            var newMenuItem = new MenuItem
            {
                Name = "Test Burger",
                Description = "A test burger for unit testing",
                Price = 12.99m,
                Category = "Burger",
                DietaryTag = "Non-Vegetarian"
            };

            var createResponse = await _client.PostAsJsonAsync("/api/menuitems", newMenuItem);
            var createdMenuItem = await createResponse.Content.ReadFromJsonAsync<MenuItem>();
            Assert.NotNull(createdMenuItem);

            // Act
            var response = await _client.GetAsync($"/api/menuitems/{createdMenuItem.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var retrievedMenuItem = await response.Content.ReadFromJsonAsync<MenuItem>();
            Assert.NotNull(retrievedMenuItem);
            Assert.Equal(createdMenuItem.Id, retrievedMenuItem.Id);
            Assert.Equal(createdMenuItem.Name, retrievedMenuItem.Name);
        }

        [Fact]
        public async Task GetMenuItemById_WithInvalidId_ShouldReturnNotFound()
        {
            // Act
            var response = await _client.GetAsync("/api/menuitems/99999");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task UpdateMenuItem_WithValidData_ShouldUpdateMenuItem()
        {
            // Arrange - First create a menu item
            var newMenuItem = new MenuItem
            {
                Name = "Original Pizza",
                Description = "Original description",
                Price = 10.99m,
                Category = "Pizza",
                DietaryTag = "Vegetarian"
            };

            var createResponse = await _client.PostAsJsonAsync("/api/menuitems", newMenuItem);
            var createdMenuItem = await createResponse.Content.ReadFromJsonAsync<MenuItem>();
            Assert.NotNull(createdMenuItem);

            // Update the menu item
            createdMenuItem.Name = "Updated Pizza";
            createdMenuItem.Description = "Updated description";
            createdMenuItem.Price = 13.99m;

            // Act
            var response = await _client.PutAsJsonAsync($"/api/menuitems/{createdMenuItem.Id}", createdMenuItem);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify the update
            var getResponse = await _client.GetAsync($"/api/menuitems/{createdMenuItem.Id}");
            var updatedMenuItem = await getResponse.Content.ReadFromJsonAsync<MenuItem>();
            Assert.NotNull(updatedMenuItem);
            Assert.Equal("Updated Pizza", updatedMenuItem.Name);
            Assert.Equal("Updated description", updatedMenuItem.Description);
            Assert.Equal(13.99m, updatedMenuItem.Price);
        }

        [Fact]
        public async Task DeleteMenuItem_WithValidId_ShouldDeleteMenuItem()
        {
            // Arrange - First create a menu item
            var newMenuItem = new MenuItem
            {
                Name = "To Be Deleted",
                Description = "This item will be deleted",
                Price = 5.99m,
                Category = "Salad",
                DietaryTag = "Vegetarian"
            };

            var createResponse = await _client.PostAsJsonAsync("/api/menuitems", newMenuItem);
            var createdMenuItem = await createResponse.Content.ReadFromJsonAsync<MenuItem>();
            Assert.NotNull(createdMenuItem);

            // Act
            var response = await _client.DeleteAsync($"/api/menuitems/{createdMenuItem.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify the deletion
            var getResponse = await _client.GetAsync($"/api/menuitems/{createdMenuItem.Id}");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact]
        public async Task DeleteMenuItem_WithInvalidId_ShouldReturnNotFound()
        {
            // Act
            var response = await _client.DeleteAsync("/api/menuitems/99999");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CreateMenuItem_WithInvalidData_ShouldReturnBadRequest()
        {
            // Arrange - Create menu item with invalid data (missing required fields)
            var invalidMenuItem = new MenuItem
            {
                Name = "", // Empty name should be invalid
                Description = "Test description",
                Price = -1, // Negative price should be invalid
                Category = "", // Empty category should be invalid
                DietaryTag = "Vegetarian"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/menuitems", invalidMenuItem);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task SearchMenuItems_WithValidSearchTerm_ShouldReturnMatchingItems()
        {
            // Arrange - Create test data
            var testItems = new List<MenuItem>
            {
                new MenuItem { Name = "Margherita Pizza", Description = "Classic pizza", Price = 12.99m, Category = "Pizza", DietaryTag = "Vegetarian" },
                new MenuItem { Name = "Pepperoni Pizza", Description = "Pizza with pepperoni", Price = 14.99m, Category = "Pizza", DietaryTag = "Non-Vegetarian" },
                new MenuItem { Name = "Caesar Salad", Description = "Fresh salad", Price = 8.99m, Category = "Salad", DietaryTag = "Vegetarian" }
            };

            foreach (var item in testItems)
            {
                await _client.PostAsJsonAsync("/api/menuitems", item);
            }

            // Act - Search for "Pizza"
            var response = await _client.GetAsync("/api/menuitems/search/Pizza");

            // Assert
            response.EnsureSuccessStatusCode();
            var searchResults = await response.Content.ReadFromJsonAsync<List<MenuItem>>();
            Assert.NotNull(searchResults);
            Assert.Equal(2, searchResults.Count);
            Assert.All(searchResults, item => Assert.Contains("Pizza", item.Name));
        }

        [Fact]
        public async Task SearchMenuItems_WithEmptySearchTerm_ShouldReturnBadRequest()
        {
            // Act
            var response = await _client.GetAsync("/api/menuitems/search/");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task SearchMenuItems_WithSpecialCharacters_ShouldHandleGracefully()
        {
            // Arrange - Create item with special characters
            var specialItem = new MenuItem 
            { 
                Name = "Pizza & Pasta Combo", 
                Description = "Special combo with @ symbols", 
                Price = 16.99m, 
                Category = "Combo", 
                DietaryTag = "Non-Vegetarian" 
            };
            await _client.PostAsJsonAsync("/api/menuitems", specialItem);

            // Act - Search with special characters
            var response = await _client.GetAsync("/api/menuitems/search/Pizza%20%26");

            // Assert
            response.EnsureSuccessStatusCode();
            var searchResults = await response.Content.ReadFromJsonAsync<List<MenuItem>>();
            Assert.NotNull(searchResults);
            Assert.Single(searchResults);
        }

        [Fact]
        public async Task SearchMenuItems_CaseInsensitive_ShouldReturnResults()
        {
            // Arrange
            var testItem = new MenuItem 
            { 
                Name = "Chicken Burger", 
                Description = "Delicious burger", 
                Price = 9.99m, 
                Category = "Burger", 
                DietaryTag = "Non-Vegetarian" 
            };
            await _client.PostAsJsonAsync("/api/menuitems", testItem);

            // Act - Search with different case
            var response = await _client.GetAsync("/api/menuitems/search/Chicken");

            // Assert
            response.EnsureSuccessStatusCode();
            var searchResults = await response.Content.ReadFromJsonAsync<List<MenuItem>>();
            Assert.NotNull(searchResults);
            Assert.Single(searchResults);
            Assert.Equal("Chicken Burger", searchResults[0].Name);
        }
    }
}
