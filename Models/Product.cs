namespace FoodY.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; }   
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public ICollection<ProductImage> Images { get; set; }
    }
}
