

using System.ComponentModel.DataAnnotations;

namespace MES.Models
{
    public class Material
    {
        [Key]
        public int MaterialId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string MaterialName { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string Unit { get; set; } = string.Empty;
        
        public double StockQuantity { get; set; }
        
        public DateTime LastUpdated { get; set; } = DateTime.Now;
        
        // Navigation properties
        public ICollection<OrderMaterial> OrderMaterials { get; set; } = new List<OrderMaterial>();
        public ICollection<ProductMaterial> ProductMaterials { get; set; } = new List<ProductMaterial>();
    }
}
