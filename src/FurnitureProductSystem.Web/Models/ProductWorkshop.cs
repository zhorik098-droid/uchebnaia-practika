using System.ComponentModel.DataAnnotations;

namespace FurnitureProductSystem.Web.Models;

public sealed class ProductWorkshop
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int WorkshopId { get; set; }
    public Workshop? Workshop { get; set; }

    [Range(0, int.MaxValue)]
    public int Minutes { get; set; }
}
