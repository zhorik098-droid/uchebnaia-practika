using FurnitureProductSystem.Web.Data;
using FurnitureProductSystem.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FurnitureProductSystem.Web.Pages.Products;

public sealed class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly ProductTimeService _time;

    public IndexModel(AppDbContext db, ProductTimeService time)
    {
        _db = db;
        _time = time;
    }

    public List<Row> Rows { get; private set; } = new();

    public sealed class Row
    {
        public int Id { get; init; }
        public string Article { get; init; } = string.Empty;
        public string ProductType { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Material { get; init; } = string.Empty;
        public decimal MinPartnerCost { get; init; }
        public int TotalMinutes { get; init; }
    }

    public async Task OnGetAsync()
    {
        var products = await _db.Products
            .AsNoTracking()
            .Include(x => x.ProductType)
            .Include(x => x.MaterialType)
            .OrderBy(x => x.Name)
            .ToListAsync();

        Rows = new List<Row>(products.Count);
        foreach (var p in products)
        {
            var total = await _time.GetTotalMinutesAsync(p.Id);
            Rows.Add(new Row
            {
                Id = p.Id,
                Article = p.Article,
                ProductType = p.ProductType?.Name ?? "–",
                Name = p.Name,
                Material = p.MaterialType?.Name ?? "–",
                MinPartnerCost = p.MinPartnerCost,
                TotalMinutes = total
            });
        }
    }
}
