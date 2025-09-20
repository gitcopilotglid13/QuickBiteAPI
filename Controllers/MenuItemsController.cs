using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickBiteAPI.Data;
using QuickBiteAPI.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace QuickBiteAPI.Controllers
{
    /// <summary>
    /// Controller for managing restaurant menu items
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [SwaggerTag("Operations for managing restaurant menu items including CRUD operations")]
    public class MenuItemsController : ControllerBase
    {
        private readonly QuickBiteDbContext _context;

        /// <summary>
        /// Initializes a new instance of the MenuItemsController
        /// </summary>
        /// <param name="context">The database context for menu items</param>
        public MenuItemsController(QuickBiteDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all menu items from the restaurant
        /// </summary>
        /// <returns>A list of all menu items</returns>
        /// <response code="200">Returns the list of menu items</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet]
        [SwaggerOperation(
            Summary = "Get all menu items",
            Description = "Retrieves a complete list of all menu items available in the restaurant",
            OperationId = "GetMenuItems",
            Tags = new[] { "Menu Items" }
        )]
        [SwaggerResponse(200, "Successfully retrieved all menu items", typeof(IEnumerable<MenuItem>))]
        [SwaggerResponse(500, "Internal server error")]
        public async Task<ActionResult<IEnumerable<MenuItem>>> GetMenuItems()
        {
            return await _context.MenuItems.ToListAsync();
        }

        /// <summary>
        /// Retrieves a specific menu item by its ID
        /// </summary>
        /// <param name="id">The unique identifier of the menu item</param>
        /// <returns>The requested menu item</returns>
        /// <response code="200">Returns the requested menu item</response>
        /// <response code="404">If the menu item with the specified ID is not found</response>
        /// <response code="400">If the provided ID is invalid</response>
        [HttpGet("{id}")]
        [SwaggerOperation(
            Summary = "Get menu item by ID",
            Description = "Retrieves a specific menu item using its unique identifier",
            OperationId = "GetMenuItem",
            Tags = new[] { "Menu Items" }
        )]
        [SwaggerResponse(200, "Successfully retrieved the menu item", typeof(MenuItem))]
        [SwaggerResponse(404, "Menu item not found")]
        [SwaggerResponse(400, "Invalid ID provided")]
        public async Task<ActionResult<MenuItem>> GetMenuItem(int id)
        {
            var menuItem = await _context.MenuItems.FindAsync(id);

            if (menuItem == null)
            {
                return NotFound();
            }

            return menuItem;
        }

        /// <summary>
        /// Creates a new menu item
        /// </summary>
        /// <param name="menuItem">The menu item to be created</param>
        /// <returns>The newly created menu item</returns>
        /// <response code="201">Returns the newly created menu item</response>
        /// <response code="400">If the menu item data is invalid</response>
        /// <response code="500">If there was an error creating the menu item</response>
        [HttpPost]
        [SwaggerOperation(
            Summary = "Create a new menu item",
            Description = "Adds a new menu item to the restaurant's menu",
            OperationId = "CreateMenuItem",
            Tags = new[] { "Menu Items" }
        )]
        [SwaggerResponse(201, "Menu item created successfully", typeof(MenuItem))]
        [SwaggerResponse(400, "Invalid menu item data provided")]
        [SwaggerResponse(500, "Error creating menu item")]
        public async Task<ActionResult<MenuItem>> PostMenuItem(MenuItem menuItem)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.MenuItems.Add(menuItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMenuItem", new { id = menuItem.Id }, menuItem);
        }

        /// <summary>
        /// Updates an existing menu item
        /// </summary>
        /// <param name="id">The unique identifier of the menu item to update</param>
        /// <param name="menuItem">The updated menu item data</param>
        /// <returns>No content if successful</returns>
        /// <response code="204">Menu item updated successfully</response>
        /// <response code="400">If the ID doesn't match or data is invalid</response>
        /// <response code="404">If the menu item is not found</response>
        /// <response code="500">If there was an error updating the menu item</response>
        [HttpPut("{id}")]
        [SwaggerOperation(
            Summary = "Update an existing menu item",
            Description = "Updates an existing menu item with new information",
            OperationId = "UpdateMenuItem",
            Tags = new[] { "Menu Items" }
        )]
        [SwaggerResponse(204, "Menu item updated successfully")]
        [SwaggerResponse(400, "Invalid data or ID mismatch")]
        [SwaggerResponse(404, "Menu item not found")]
        [SwaggerResponse(500, "Error updating menu item")]
        public async Task<IActionResult> PutMenuItem(int id, MenuItem menuItem)
        {
            if (id != menuItem.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Entry(menuItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MenuItemExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        /// <summary>
        /// Deletes a menu item
        /// </summary>
        /// <param name="id">The unique identifier of the menu item to delete</param>
        /// <returns>No content if successful</returns>
        /// <response code="204">Menu item deleted successfully</response>
        /// <response code="404">If the menu item is not found</response>
        /// <response code="500">If there was an error deleting the menu item</response>
        [HttpDelete("{id}")]
        [SwaggerOperation(
            Summary = "Delete a menu item",
            Description = "Removes a menu item from the restaurant's menu",
            OperationId = "DeleteMenuItem",
            Tags = new[] { "Menu Items" }
        )]
        [SwaggerResponse(204, "Menu item deleted successfully")]
        [SwaggerResponse(404, "Menu item not found")]
        [SwaggerResponse(500, "Error deleting menu item")]
        public async Task<IActionResult> DeleteMenuItem(int id)
        {
            var menuItem = await _context.MenuItems.FindAsync(id);
            if (menuItem == null)
            {
                return NotFound();
            }

            _context.MenuItems.Remove(menuItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Search menu items by name using secure parameterized queries
        /// </summary>
        /// <param name="name">The name to search for</param>
        /// <returns>List of menu items matching the search criteria</returns>
        /// <response code="200">Returns the list of matching menu items</response>
        /// <response code="400">If the search parameter is invalid</response>
        [HttpGet("search/{name}")]
        [SwaggerOperation(
            Summary = "Search menu items by name",
            Description = "Safely searches for menu items by name using secure parameterized queries to prevent SQL injection attacks",
            OperationId = "SearchMenuItems",
            Tags = new[] { "Menu Items" }
        )]
        [SwaggerResponse(200, "Successfully retrieved matching menu items", typeof(IEnumerable<MenuItem>))]
        [SwaggerResponse(400, "Invalid search parameter")]
        public async Task<ActionResult<IEnumerable<MenuItem>>> SearchMenuItems(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest("Search name cannot be empty");
            }

            // ✅ SECURE: Using Entity Framework LINQ - automatically parameterized
            var menuItems = await _context.MenuItems
                .Where(m => m.Name.Contains(name))
                .ToListAsync();

            return Ok(menuItems);
        }

        /// <summary>
        /// Filter menu items by category using secure parameterized queries
        /// </summary>
        /// <param name="category">The category to filter by</param>
        /// <returns>List of menu items in the specified category</returns>
        /// <response code="200">Returns the list of menu items in the category</response>
        /// <response code="400">If the category parameter is invalid</response>
        [HttpGet("category/{category}")]
        [SwaggerOperation(
            Summary = "Filter menu items by category",
            Description = "Safely filters menu items by category using secure parameterized queries to prevent SQL injection attacks",
            OperationId = "FilterMenuItemsByCategory",
            Tags = new[] { "Menu Items" }
        )]
        [SwaggerResponse(200, "Successfully retrieved menu items in the category", typeof(IEnumerable<MenuItem>))]
        [SwaggerResponse(400, "Invalid category parameter")]
        public async Task<ActionResult<IEnumerable<MenuItem>>> FilterByCategory(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                return BadRequest("Category cannot be empty");
            }

            // ✅ SECURE: Using Entity Framework LINQ - automatically parameterized
            var menuItems = await _context.MenuItems
                .Where(m => m.Category.ToLower() == category.ToLower())
                .ToListAsync();

            return Ok(menuItems);
        }

        /// <summary>
        /// Filter menu items by dietary tag using secure parameterized queries
        /// </summary>
        /// <param name="dietaryTag">The dietary tag to filter by</param>
        /// <returns>List of menu items with the specified dietary tag</returns>
        /// <response code="200">Returns the list of menu items with the dietary tag</response>
        /// <response code="400">If the dietary tag parameter is invalid</response>
        [HttpGet("dietary/{dietaryTag}")]
        [SwaggerOperation(
            Summary = "Filter menu items by dietary tag",
            Description = "Safely filters menu items by dietary tag using secure parameterized queries to prevent SQL injection attacks",
            OperationId = "FilterMenuItemsByDietaryTag",
            Tags = new[] { "Menu Items" }
        )]
        [SwaggerResponse(200, "Successfully retrieved menu items with the dietary tag", typeof(IEnumerable<MenuItem>))]
        [SwaggerResponse(400, "Invalid dietary tag parameter")]
        public async Task<ActionResult<IEnumerable<MenuItem>>> FilterByDietaryTag(string dietaryTag)
        {
            if (string.IsNullOrWhiteSpace(dietaryTag))
            {
                return BadRequest("Dietary tag cannot be empty");
            }

            // ✅ SECURE: Using Entity Framework LINQ - automatically parameterized
            var menuItems = await _context.MenuItems
                .Where(m => m.DietaryTag.ToLower().Contains(dietaryTag.ToLower()))
                .ToListAsync();

            return Ok(menuItems);
        }

        /// <summary>
        /// Get menu items within a price range using secure parameterized queries
        /// </summary>
        /// <param name="minPrice">Minimum price</param>
        /// <param name="maxPrice">Maximum price</param>
        /// <returns>List of menu items within the price range</returns>
        /// <response code="200">Returns the list of menu items within the price range</response>
        /// <response code="400">If the price parameters are invalid</response>
        [HttpGet("price-range")]
        [SwaggerOperation(
            Summary = "Get menu items within price range",
            Description = "Safely retrieves menu items within a specified price range using secure parameterized queries",
            OperationId = "GetMenuItemsByPriceRange",
            Tags = new[] { "Menu Items" }
        )]
        [SwaggerResponse(200, "Successfully retrieved menu items within price range", typeof(IEnumerable<MenuItem>))]
        [SwaggerResponse(400, "Invalid price parameters")]
        public async Task<ActionResult<IEnumerable<MenuItem>>> GetByPriceRange(
            [FromQuery] decimal minPrice, 
            [FromQuery] decimal maxPrice)
        {
            if (minPrice < 0 || maxPrice < 0)
            {
                return BadRequest("Price values cannot be negative");
            }

            if (minPrice > maxPrice)
            {
                return BadRequest("Minimum price cannot be greater than maximum price");
            }

            // ✅ SECURE: Using Entity Framework LINQ - automatically parameterized
            var menuItems = await _context.MenuItems
                .Where(m => m.Price >= minPrice && m.Price <= maxPrice)
                .ToListAsync();
            
            // Order on client side due to SQLite decimal ordering limitation
            menuItems = menuItems.OrderBy(m => m.Price).ToList();

            return Ok(menuItems);
        }

        /// <summary>
        /// Health check endpoint for monitoring and load balancers
        /// </summary>
        /// <returns>Health status of the API</returns>
        /// <response code="200">API is healthy and ready to serve requests</response>
        [HttpGet("health")]
        [SwaggerOperation(
            Summary = "Health check endpoint",
            Description = "Returns the health status of the API for monitoring and load balancers",
            OperationId = "HealthCheck",
            Tags = new[] { "Health" }
        )]
        [SwaggerResponse(200, "API is healthy", typeof(object))]
        public IActionResult HealthCheck()
        {
            return Ok(new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Service = "QuickBite API",
                Version = "1.0.0",
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"
            });
        }

        private bool MenuItemExists(int id)
        {
            return _context.MenuItems.Any(e => e.Id == id);
        }
    }
}
