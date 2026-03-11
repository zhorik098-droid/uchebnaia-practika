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
    /// Принимаются: идентификатор типа продукта, идентификатор типа материала, количество (в целых числах), параметры продукта (с положительным удвоением).
    /// Возвращается: требуемое количество сырья (в целых числах, округленное в большую сторону) с учетом коэффициента и процента потерь.
    /// Возвращает значение -1 для недопустимых идентификаторов или неверных входных значений.
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

        // сырье для каждого изделия = параметр 1 * параметр 2 * коэффициент полезного действия
        var perItem = param1 * param2 * coef;

        // итого с убытками
        var total = perItem * quantity * (1.0 + loss);

        if (double.IsInfinity(total) || total < 0)
            return -1;

        // целая сумма, округленная в большую сторону
        var result = (int)Math.Ceiling(total);
        return result < 0 ? -1 : result;
    }
}
