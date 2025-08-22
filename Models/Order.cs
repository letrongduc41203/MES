

namespace MES.Models
{
    public enum OrderStatus
    {
        Pending,
        Processing,
        Completed
    }

    public class Order
    {
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public Product Product { get; set; }
        public ICollection<OrderMaterial> OrderMaterials { get; set; }
    }

    public class OrderMaterial
    {
        public int OrderId { get; set; }
        public int MaterialId { get; set; }
        public int QtyUsed { get; set; }
        public int ProcessedQuantity { get; set; }

        public Order Order { get; set; }
        public Material Material { get; set; }
    }

    public class OrderRow
    {
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Status { get; set; }
        public int Quantity { get; set; }
    }
}
