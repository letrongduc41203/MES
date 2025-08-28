

using System.ComponentModel.DataAnnotations;

namespace MES.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; } = string.Empty;
        
        [Required]
        [StringLength(20)]
        public string Role { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
