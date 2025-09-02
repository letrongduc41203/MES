using System.ComponentModel.DataAnnotations;

namespace MES.Models
{
    public class Employee
    {
        [Key]
        public int EmployeeId { get; set; }

        [Required]
        [StringLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active"; // Active / Inactive

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; }

        // Navigation
        public ICollection<OrderEmployee> OrderEmployees { get; set; } = new List<OrderEmployee>();
    }

    public class OrderEmployee
    {
        public int OrderId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime AssignedDate { get; set; }
        public string Role { get; set; } = string.Empty;

        // Navigation
        public Order Order { get; set; } = null!;
        public Employee Employee { get; set; } = null!;
    }
}
