namespace Products.DTOs
{
    public class ProductDTO
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public decimal Price { get; set; }
    }
}
