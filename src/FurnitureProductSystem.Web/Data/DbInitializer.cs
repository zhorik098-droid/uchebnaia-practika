using ClosedXML.Excel;
using FurnitureProductSystem.Web.Models;

namespace FurnitureProductSystem.Web.Data;

public static class DbInitializer
{
    public static void Seed(AppDbContext db, string contentRootPath)
    {
        // If DB already has data – do nothing.
        if (db.ProductTypes.Any() || db.MaterialTypes.Any() || db.Products.Any())
        {
            return;
        }

        // Try to import from provided Excel resources (files with "import")
        var importDir = Path.Combine(contentRootPath, "Data", "Import");
        var ptPath = Path.Combine(importDir, "Product_type_import.xlsx");
        var mtPath = Path.Combine(importDir, "Material_type_import.xlsx");
        var wPath  = Path.Combine(importDir, "Workshops_import.xlsx");
        var pPath  = Path.Combine(importDir, "Products_import.xlsx");
        var pwPath = Path.Combine(importDir, "Product_workshops_import.xlsx");

        if (File.Exists(ptPath) && File.Exists(mtPath) && File.Exists(wPath) && File.Exists(pPath) && File.Exists(pwPath))
        {
            ImportFromExcel(db, ptPath, mtPath, wPath, pPath, pwPath);
            return;
        }

        // Fallback demo seed (in case Excel files are missing)
        SeedDemo(db);
    }

    private static void ImportFromExcel(
        AppDbContext db,
        string productTypePath,
        string materialTypePath,
        string workshopsPath,
        string productsPath,
        string productWorkshopsPath)
    {
        using var ptWb = new XLWorkbook(productTypePath);
        var ptWs = ptWb.Worksheet(1);
        foreach (var row in ptWs.RowsUsed().Skip(1))
        {
            var name = row.Cell(1).GetString().Trim();
            var coef = row.Cell(2).GetValue<decimal>();
            if (!string.IsNullOrWhiteSpace(name))
                db.ProductTypes.Add(new ProductType { Name = name, Coefficient = coef });
        }
        db.SaveChanges();

        using var mtWb = new XLWorkbook(materialTypePath);
        var mtWs = mtWb.Worksheet(1);
        foreach (var row in mtWs.RowsUsed().Skip(1))
        {
            var name = row.Cell(1).GetString().Trim();
            var loss = row.Cell(2).GetValue<decimal>();
            if (!string.IsNullOrWhiteSpace(name))
                db.MaterialTypes.Add(new MaterialType { Name = name, LossPercent = loss });
        }
        db.SaveChanges();

        using var wWb = new XLWorkbook(workshopsPath);
        var wWs = wWb.Worksheet(1);
        foreach (var row in wWs.RowsUsed().Skip(1))
        {
            var name = row.Cell(1).GetString().Trim();
            var people = row.Cell(3).GetValue<int>();
            if (!string.IsNullOrWhiteSpace(name))
                db.Workshops.Add(new Workshop { Name = name, PeopleCount = people });
        }
        db.SaveChanges();

        using var pWb = new XLWorkbook(productsPath);
        var pWs = pWb.Worksheet(1);
        foreach (var row in pWs.RowsUsed().Skip(1))
        {
            var ptName = row.Cell(1).GetString().Trim();
            var name = row.Cell(2).GetString().Trim();
            var article = row.Cell(3).GetValue<long>().ToString();
            var cost = row.Cell(4).GetValue<decimal>();
            var mtName = row.Cell(5).GetString().Trim();

            var pt = db.ProductTypes.FirstOrDefault(x => x.Name == ptName);
            var mt = db.MaterialTypes.FirstOrDefault(x => x.Name == mtName);
            if (pt is null || mt is null || string.IsNullOrWhiteSpace(name))
                continue;

            db.Products.Add(new Product
            {
                Article = article,
                Name = name,
                MinPartnerCost = cost,
                ProductTypeId = pt.Id,
                MaterialTypeId = mt.Id
            });
        }
        db.SaveChanges();

        using var pwWb = new XLWorkbook(productWorkshopsPath);
        var pwWs = pwWb.Worksheet(1);
        foreach (var row in pwWs.RowsUsed().Skip(1))
        {
            var productName = row.Cell(1).GetString().Trim();
            var workshopName = row.Cell(2).GetString().Trim();
            var hours = row.Cell(3).GetValue<decimal>();

            var product = db.Products.FirstOrDefault(x => x.Name == productName);
            var workshop = db.Workshops.FirstOrDefault(x => x.Name == workshopName);
            if (product is null || workshop is null)
                continue;

            var minutes = (int)Math.Round((double)(hours * 60m), MidpointRounding.AwayFromZero);
            if (minutes < 0) minutes = 0;

            db.ProductWorkshops.Add(new ProductWorkshop
            {
                ProductId = product.Id,
                WorkshopId = workshop.Id,
                Minutes = minutes
            });
        }
        db.SaveChanges();
    }

    private static void SeedDemo(AppDbContext db)
    {
        var chairType = new ProductType { Name = "Стул", Coefficient = 1.10m };
        var tableType = new ProductType { Name = "Стол", Coefficient = 1.35m };
        var wardrobeType = new ProductType { Name = "Шкаф", Coefficient = 1.75m };

        var oak = new MaterialType { Name = "Дуб", LossPercent = 0.03m };
        var pine = new MaterialType { Name = "Сосна", LossPercent = 0.05m };
        var mdf = new MaterialType { Name = "МДФ", LossPercent = 0.02m };

        db.ProductTypes.AddRange(chairType, tableType, wardrobeType);
        db.MaterialTypes.AddRange(oak, pine, mdf);

        var w1 = new Workshop { Name = "Заготовительный", PeopleCount = 6 };
        var w2 = new Workshop { Name = "Сборочный", PeopleCount = 4 };
        var w3 = new Workshop { Name = "Отделочный", PeopleCount = 3 };
        db.Workshops.AddRange(w1, w2, w3);
        db.SaveChanges();

        var p1 = new Product { Article = "A-1001", Name = "Стул кухонный", MinPartnerCost = 2499.99m, ProductTypeId = chairType.Id, MaterialTypeId = pine.Id };
        var p2 = new Product { Article = "B-2001", Name = "Стол обеденный", MinPartnerCost = 13999.00m, ProductTypeId = tableType.Id, MaterialTypeId = oak.Id };
        db.Products.AddRange(p1, p2);
        db.SaveChanges();

        db.ProductWorkshops.AddRange(
            new ProductWorkshop { ProductId = p1.Id, WorkshopId = w1.Id, Minutes = 25 },
            new ProductWorkshop { ProductId = p1.Id, WorkshopId = w2.Id, Minutes = 35 },
            new ProductWorkshop { ProductId = p1.Id, WorkshopId = w3.Id, Minutes = 20 },
            new ProductWorkshop { ProductId = p2.Id, WorkshopId = w1.Id, Minutes = 45 },
            new ProductWorkshop { ProductId = p2.Id, WorkshopId = w2.Id, Minutes = 60 },
            new ProductWorkshop { ProductId = p2.Id, WorkshopId = w3.Id, Minutes = 40 }
        );
        db.SaveChanges();
    }
}
