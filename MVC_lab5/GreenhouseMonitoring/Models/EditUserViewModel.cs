// Models/EditUserViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace GreenhouseMonitoring.Models
{
    public class EditUserViewModel
    {
        public string Id { get; set; } = "";

        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        public string? Role { get; set; }
    }
}