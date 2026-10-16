using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenhouseMonitoring.Models;

public partial class NormativeParameter
{
    [Column("ParameterID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ParameterId { get; set; } // ← Исправлено: ParameterId (не ParameterID)

    [Column("CropID")]
    //[Required(ErrorMessage = "Выберите культуру")]
    public int CropId { get; set; }

    [Required(ErrorMessage = "Укажите параметр")]
    [StringLength(50, ErrorMessage = "Параметр не может превышать 50 символов")]
    public string Parameter { get; set; } = null!;

    [Required(ErrorMessage = "Укажите минимальное значение")]
    [Range(0, 999999.99, ErrorMessage = "Минимальное значение должно быть от 0 до 999999.99")]
    public decimal MinValue { get; set; }

    [Required(ErrorMessage = "Укажите максимальное значение")]
    [Range(0, 999999.99, ErrorMessage = "Максимальное значение должно быть от 0 до 999999.99")]
    public decimal MaxValue { get; set; }

    [Required(ErrorMessage = "Укажите стадию роста")]
    [StringLength(50, ErrorMessage = "Стадия роста не может превышать 50 символов")]
    public string GrowthStage { get; set; } = null!;

    public virtual Crop? Crop { get; set; } = null!;
}