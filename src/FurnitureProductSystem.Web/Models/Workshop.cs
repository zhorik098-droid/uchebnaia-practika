using System.ComponentModel.DataAnnotations;

namespace FurnitureProductSystem.Web.Models;

public sealed class Workshop
{
    public int Id { get; set; }

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int PeopleCount { get; set; }

    public List<ProductWorkshop> Products { get; set; } = new();
}
