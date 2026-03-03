using FurnitureProductSystem.Web.Data;

namespace FurnitureProductSystem.Web.Services;

public sealed class RawMaterialCalculator
{
    private readonly AppDbContext _db;

    public RawMaterialCalculator(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Module 4 requirement.
    /// Accepts: product type id, material type id, quantity (ints), product params (positive doubles).
    /// Returns: required raw material amount (int, rounded up) considering coefficient and loss percent.
    /// Returns -1 for invalid ids or invalid input values.
    /// </summary>
    public int CalculateRawMaterial(int productTypeId, int materialTypeId, int quantity, double param1, double param2)
    {
        if (productTypeId <= 0 || materialTypeId <= 0 || quantity <= 0)
            return -1;

        if (double.IsNaN(param1) || double.IsNaN(param2) || param1 <= 0 || param2 <= 0)
            return -1;

        var pt = _db.ProductTypes.FirstOrDefault(x => x.Id == productTypeId);
        var mt = _db.MaterialTypes.FirstOrDefault(x => x.Id == materialTypeId);
        if (pt is null || mt is null)
            return -1;

        var coef = (double)pt.Coefficient;
        var loss = (double)mt.LossPercent;

        if (coef <= 0 || loss < 0)
            return -1;

        // raw per item = param1 * param2 * coef
        var perItem = param1 * param2 * coef;

        // total with losses
        var total = perItem * quantity * (1.0 + loss);

        if (double.IsInfinity(total) || total < 0)
            return -1;

        // integer amount, round up (no shortages)
        var result = (int)Math.Ceiling(total);
        return result < 0 ? -1 : result;
    }
}
