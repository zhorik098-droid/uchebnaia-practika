using FurnitureProductSystem.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace FurnitureProductSystem.Web.Services;

public sealed class ProductTimeService
{
    private readonly AppDbContext _db;

    public ProductTimeService(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Возвращает общее время изготовления продукции
    /// как сумму времени по всем связанным цехам.
    /// </summary>
    public async Task<int> GetTotalMinutesAsync(int productId)
    {
        var minutes = await _db.ProductWorkshops
            .Where(x => x.ProductId == productId)
            .SumAsync(x => (int?)x.Minutes);

        return minutes ?? 0;
    }
}
