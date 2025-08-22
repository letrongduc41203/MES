

namespace MES.Models
{
    public class Material
    {
        public int MaterialId { get; set; }
        public string MaterialName { get; set; }
        public string Unit { get; set; }
        public double StockQuantity { get; set; }
        public DateTime LastUpdated { get; set; }
    }

}
