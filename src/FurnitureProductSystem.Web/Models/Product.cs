using System.ComponentModel.DataAnnotations;

namespace FurnitureProductSystem.Web.Models;

public sealed class Product
{
    public int Id { get; set; }

    [Required, StringLength(64)]
    public string Article { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Range(0, double.MaxValue, ErrorMessage = "Стоимость не может быть отрицательной")]
    public decimal MinPartnerCost { get; set; }

    [Display(Name = "Тип продукции")]
    public int ProductTypeId { get; set; }
    public ProductType? ProductType { get; set; }

    [Display(Name = "Mатериал")]
    public int MaterialTypeId { get; set; }
    public MaterialType? MaterialType { get; set; }

    public List<ProductWorkshop> Workshops { get; set; } = new();
}
