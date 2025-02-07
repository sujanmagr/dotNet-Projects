namespace FoodY.ViewModel
{
    // ViewModels/CategoryViewModel.cs
    public class CategoryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    // ViewModels/ProductViewModel.cs
    public class ProductViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public List<string>? ImageFileNames { get; set; }
    }
    // ViewModels/SearchViewModel.cs
    public class SearchViewModel
    {
        public string Name { get; set; }
        public int? CategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
    }
}