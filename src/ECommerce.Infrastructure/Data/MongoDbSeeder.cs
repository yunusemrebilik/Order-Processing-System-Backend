using ECommerce.Application.Common.Interfaces;
using ECommerce.Domain.Entities;

namespace ECommerce.Infrastructure.Data;

/// <summary>
/// Seeds MongoDB with sample products for development.
/// Products demonstrate the flexible schema advantage of MongoDB
/// (different attributes per category).
/// </summary>
public static class MongoDbSeeder
{
    public static async Task SeedAsync(IProductRepository productRepository)
    {
        var (existing, _) = await productRepository.GetAllAsync(pageSize: 1);
        if (existing.Count > 0)
            return; // Already seeded

        var products = new List<Product>
        {
            // Electronics
            new()
            {
                Name = "iPhone 15 Pro Max",
                Description = "Apple's flagship smartphone with A17 Pro chip and titanium design.",
                Category = "Electronics",
                Price = 1199.99m,
                ImageUrl = "https://placeholder.co/600x400?text=iPhone+15+Pro",
                Attributes = new Dictionary<string, object>
                {
                    { "Brand", "Apple" },
                    { "RAM", "8GB" },
                    { "Storage", "256GB" },
                    { "Screen", "6.7 inch OLED" },
                    { "Chip", "A17 Pro" },
                    { "Color", "Natural Titanium" }
                }
            },
            new()
            {
                Name = "Samsung Galaxy S24 Ultra",
                Description = "Samsung's premium smartphone with AI features and S Pen support.",
                Category = "Electronics",
                Price = 1299.99m,
                ImageUrl = "https://placeholder.co/600x400?text=Galaxy+S24",
                Attributes = new Dictionary<string, object>
                {
                    { "Brand", "Samsung" },
                    { "RAM", "12GB" },
                    { "Storage", "512GB" },
                    { "Screen", "6.8 inch Dynamic AMOLED" },
                    { "Chip", "Snapdragon 8 Gen 3" },
                    { "Color", "Titanium Gray" }
                }
            },
            new()
            {
                Name = "Sony WH-1000XM5 Headphones",
                Description = "Industry-leading noise cancelling over-ear headphones.",
                Category = "Electronics",
                Price = 349.99m,
                ImageUrl = "https://placeholder.co/600x400?text=Sony+XM5",
                Attributes = new Dictionary<string, object>
                {
                    { "Brand", "Sony" },
                    { "Type", "Over-Ear" },
                    { "Connectivity", "Bluetooth 5.2" },
                    { "Battery Life", "30 hours" },
                    { "Noise Cancelling", true }
                }
            },

            // Clothing
            new()
            {
                Name = "Nike Air Max 90",
                Description = "Classic Nike sneakers with visible Air cushioning.",
                Category = "Clothing",
                Price = 129.99m,
                ImageUrl = "https://placeholder.co/600x400?text=Air+Max+90",
                Attributes = new Dictionary<string, object>
                {
                    { "Brand", "Nike" },
                    { "Size", "42" },
                    { "Color", "White/Black" },
                    { "Material", "Leather/Mesh" },
                    { "Gender", "Unisex" }
                }
            },
            new()
            {
                Name = "Levi's 501 Original Fit Jeans",
                Description = "The original jean, iconic since 1873. Straight fit through the leg.",
                Category = "Clothing",
                Price = 79.99m,
                ImageUrl = "https://placeholder.co/600x400?text=Levi+501",
                Attributes = new Dictionary<string, object>
                {
                    { "Brand", "Levi's" },
                    { "Size", "32x32" },
                    { "Color", "Medium Indigo" },
                    { "Fit", "Original" },
                    { "Material", "100% Cotton Denim" }
                }
            },

            // Books
            new()
            {
                Name = "Clean Architecture by Robert C. Martin",
                Description = "A Craftsman's Guide to Software Structure and Design.",
                Category = "Books",
                Price = 34.99m,
                ImageUrl = "https://placeholder.co/600x400?text=Clean+Architecture",
                Attributes = new Dictionary<string, object>
                {
                    { "Author", "Robert C. Martin" },
                    { "Publisher", "Prentice Hall" },
                    { "Pages", 432 },
                    { "ISBN", "978-0134494166" },
                    { "Format", "Paperback" },
                    { "Language", "English" }
                }
            },
            new()
            {
                Name = "Designing Data-Intensive Applications",
                Description = "The Big Ideas Behind Reliable, Scalable, and Maintainable Systems by Martin Kleppmann.",
                Category = "Books",
                Price = 44.99m,
                ImageUrl = "https://placeholder.co/600x400?text=DDIA",
                Attributes = new Dictionary<string, object>
                {
                    { "Author", "Martin Kleppmann" },
                    { "Publisher", "O'Reilly" },
                    { "Pages", 616 },
                    { "ISBN", "978-1449373320" },
                    { "Format", "Paperback" },
                    { "Language", "English" }
                }
            },

            // Home & Kitchen
            new()
            {
                Name = "Dyson V15 Detect Absolute",
                Description = "Intelligent cordless vacuum with laser dust detection.",
                Category = "Home & Kitchen",
                Price = 749.99m,
                ImageUrl = "https://placeholder.co/600x400?text=Dyson+V15",
                Attributes = new Dictionary<string, object>
                {
                    { "Brand", "Dyson" },
                    { "Type", "Cordless Stick" },
                    { "Battery Life", "60 minutes" },
                    { "Weight", "3.1 kg" },
                    { "Laser Detection", true }
                }
            }
        };

        foreach (var product in products)
        {
            await productRepository.CreateAsync(product);
        }
    }
}
