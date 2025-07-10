using System.ComponentModel.DataAnnotations;

namespace JwtAuthDemo.Model.Entity
{
    public class AppUser
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(10)]
        public string? UserName { get; set; }

        [Required]
        public string? Passwordhash { get; set; }

        [Required]
        [MaxLength(20)]
        public string? Email { get; set; }
    }
}
