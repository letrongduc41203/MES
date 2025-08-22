

namespace MES.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public string Unit { get; set; }
        public ICollection<ProductMaterial> ProductMaterials { get; set; }
    }

    public class ProductMaterial
    {
        public int ProductId { get; set; }
        public int MaterialId { get; set; }
        public int QtyNeeded { get; set; }

        public Product Product { get; set; }
        public Material Material { get; set; }
    }
}
