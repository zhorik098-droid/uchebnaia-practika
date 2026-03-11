using System.ComponentModel.DataAnnotations;

namespace FurnitureProductSystem.Web.Models;

public sealed class ProductType
{
    public int Id { get; set; }

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    // расчет сырья
    public decimal Coefficient { get; set; }

    public List<Product> Products { get; set; } = new();
}
