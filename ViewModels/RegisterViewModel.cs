using System.ComponentModel.DataAnnotations;

namespace GreenSwampApp.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [StringLength(500)]
        public string Bio { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
