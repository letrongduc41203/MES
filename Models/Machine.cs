using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MES.Models
{
    public enum MachineStatus
    {
        Available,
        Busy,
        Maintenance,
        Error,
        Running,
        Idle
    }

    public class Machine
    {
        [Key]
        public int MachineId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string MachineName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string MachineType { get; set; } = string.Empty;
        
        public MachineStatus Status { get; set; } = MachineStatus.Available;

        public int? CurrentOrderId { get; set; }
        
        public DateTime? LastMaintenanceDate { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        public DateTime? UpdatedDate { get; set; }
        
        // Navigation properties
        public ICollection<OrderMachine> OrderMachines { get; set; } = new List<OrderMachine>();
        public ICollection<MachineMaintenance> MaintenanceHistory { get; set; } = new List<MachineMaintenance>();
    }

    public class OrderMachine
    {
        public int OrderId { get; set; }
        public int MachineId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        
        // Navigation properties
        public Order Order { get; set; } = null!;
        public Machine Machine { get; set; } = null!;
    }

    public class MachineMaintenance
    {
        [Key]
        public int MaintenanceId { get; set; }
        
        public int MachineId { get; set; }
        public DateTime MaintenanceDate { get; set; }
        
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        

        
        [StringLength(100)]
        public string Technician { get; set; } = string.Empty;
        
        // Navigation properties
        public Machine Machine { get; set; } = null!;
    }
} 