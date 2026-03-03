using System.ComponentModel.DataAnnotations;

namespace FurnitureProductSystem.Web.Models;

public sealed class MaterialType
{
    public int Id { get; set; }

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    // Used in Module 4 method (raw material calculation)
    public decimal LossPercent { get; set; }

    public List<Product> Products { get; set; } = new();
}
