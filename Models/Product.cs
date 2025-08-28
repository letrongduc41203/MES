

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MES.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string ProductName { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string ProductCode { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string Unit { get; set; } = string.Empty;
        
        // Navigation properties
        public ICollection<ProductMaterial> ProductMaterials { get; set; } = new List<ProductMaterial>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }

    public class ProductMaterial
    {
        public int ProductId { get; set; }
        public int MaterialId { get; set; }
        public int QtyNeeded { get; set; }

        // Navigation properties
        public Product Product { get; set; } = null!;
        public Material Material { get; set; } = null!;
    }
}
