using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace appReservas.Areas.Admin.Models
{
    public class CreateOwnerVM
    {
        [Required, EmailAddress] public string Email { get; set; } = default!;
        [Required, DataType(DataType.Password), StringLength(100, MinimumLength = 6)] public string Password { get; set; } = default!;
        [Required, DataType(DataType.Password), Compare(nameof(Password))] public string ConfirmPassword { get; set; } = default!;
        public int? RestauranteId { get; set; }
        public List<SelectListItem> Restaurantes { get; set; } = new();
    }
}
