// Models/RegisterViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace GreenhouseMonitoring.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Введите email")]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Введите пароль")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = "";

        public string? Role { get; set; }
    }
}