using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace QuickBiteAPI.Models
{
    /// <summary>
    /// Represents a menu item in the restaurant's food menu
    /// </summary>
    [SwaggerSchema(
        Title = "Menu Item",
        Description = "A food item available in the restaurant's menu with details like name, description, price, category, and dietary information"
    )]
    public class MenuItem
    {
        /// <summary>
        /// Unique identifier for the menu item
        /// </summary>
        /// <example>1</example>
        [SwaggerSchema(Description = "The unique identifier for the menu item")]
        public int Id { get; set; }
        
        /// <summary>
        /// Name of the menu item
        /// </summary>
        /// <example>Margherita Pizza</example>
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        [SwaggerSchema(Description = "The name of the menu item")]
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Detailed description of the menu item
        /// </summary>
        /// <example>Classic pizza with tomato sauce, mozzarella, and fresh basil</example>
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [SwaggerSchema(Description = "A detailed description of the menu item including ingredients and preparation details")]
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// Price of the menu item in decimal format
        /// </summary>
        /// <example>12.99</example>
        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        [SwaggerSchema(Description = "The price of the menu item in decimal format")]
        public decimal Price { get; set; }
        
        /// <summary>
        /// Category that the menu item belongs to (e.g., Pizza, Burger, Salad, etc.)
        /// </summary>
        /// <example>Pizza</example>
        [Required(ErrorMessage = "Category is required")]
        [StringLength(50, ErrorMessage = "Category cannot exceed 50 characters")]
        [SwaggerSchema(Description = "The category that the menu item belongs to (e.g., Pizza, Burger, Salad, Dessert, etc.)")]
        public string Category { get; set; } = string.Empty;
        
        /// <summary>
        /// Dietary information or tags for the menu item (e.g., Vegetarian, Vegan, Gluten-Free, etc.)
        /// </summary>
        /// <example>Vegetarian</example>
        [StringLength(100, ErrorMessage = "Dietary tag cannot exceed 100 characters")]
        [SwaggerSchema(Description = "Dietary information or tags for the menu item (e.g., Vegetarian, Vegan, Gluten-Free, Non-Vegetarian, etc.)")]
        public string DietaryTag { get; set; } = string.Empty;
    }
}
