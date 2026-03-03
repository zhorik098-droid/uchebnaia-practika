using FurnitureProductSystem.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FurnitureProductSystem.Web.Pages.Workshops;

public sealed class IndexModel : PageModel
{
    private readonly AppDbContext _db;

    public IndexModel(AppDbContext db)
    {
        _db = db;
    }

    public string ProductTitle { get; private set; } = string.Empty;

    public List<Row> Rows { get; private set; } = new();

    [BindProperty]
    public string NewWorkshopName { get; set; } = string.Empty;

    [BindProperty]
    public int NewPeopleCount { get; set; }

    [BindProperty]
    public int NewMinutes { get; set; }

    public sealed class Row
    {
        public int ProductWorkshopId { get; init; }
        public string WorkshopName { get; init; } = string.Empty;
        public int PeopleCount { get; init; }
        public int Minutes { get; init; }
    }

    public async Task<IActionResult> OnGetAsync(int productId)
    {
        return await LoadAsync(productId);
    }

    public async Task<IActionResult> OnPostAddAsync(int productId)
    {
        var productExists = await _db.Products.AnyAsync(x => x.Id == productId);
        if (!productExists)
        {
            TempData["AlertType"] = "error";
            TempData["AlertTitle"] = "Ошибка";
            TempData["AlertMessage"] = "Продукт не найден.";
            return RedirectToPage("/Products/Index");
        }

        NewWorkshopName = (NewWorkshopName ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(NewWorkshopName))
        {
            TempData["AlertType"] = "warning";
            TempData["AlertTitle"] = "Проверь данные";
            TempData["AlertMessage"] = "Название цеха не может быть пустым.";
            return await LoadAsync(productId);
        }

        if (NewPeopleCount < 0 || NewMinutes < 0)
        {
            TempData["AlertType"] = "warning";
            TempData["AlertTitle"] = "Проверь данные";
            TempData["AlertMessage"] = "Количество людей и время должны быть 0 или больше.";
            return await LoadAsync(productId);
        }

        // Если такой цех уже есть (по имени) – используем его, чтобы не плодить дубликаты.
        var existingWorkshop = await _db.Workshops.FirstOrDefaultAsync(x => x.Name == NewWorkshopName);
        if (existingWorkshop is null)
        {
            existingWorkshop = new Models.Workshop
            {
                Name = NewWorkshopName,
                PeopleCount = NewPeopleCount
            };
            _db.Workshops.Add(existingWorkshop);
            await _db.SaveChangesAsync();
        }
        else
        {
            // Обновим PeopleCount, если пользователь ввёл значение.
            existingWorkshop.PeopleCount = NewPeopleCount;
            await _db.SaveChangesAsync();
        }

        var linkExists = await _db.ProductWorkshops.AnyAsync(x => x.ProductId == productId && x.WorkshopId == existingWorkshop.Id);
        if (linkExists)
        {
            TempData["AlertType"] = "info";
            TempData["AlertTitle"] = "Уже добавлено";
            TempData["AlertMessage"] = "Этот цех уже привязан к продукту.";
            return RedirectToPage(new { productId });
        }

        _db.ProductWorkshops.Add(new Models.ProductWorkshop
        {
            ProductId = productId,
            WorkshopId = existingWorkshop.Id,
            Minutes = NewMinutes
        });

        await _db.SaveChangesAsync();

        TempData["AlertType"] = "info";
        TempData["AlertTitle"] = "Готово";
        TempData["AlertMessage"] = "Цех добавлен.";
        return RedirectToPage(new { productId });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int productId, int productWorkshopId)
    {
        var link = await _db.ProductWorkshops.FirstOrDefaultAsync(x => x.Id == productWorkshopId && x.ProductId == productId);
        if (link is null)
        {
            TempData["AlertType"] = "warning";
            TempData["AlertTitle"] = "Не найдено";
            TempData["AlertMessage"] = "Связь продукт–цех не найдена.";
            return RedirectToPage(new { productId });
        }

        _db.ProductWorkshops.Remove(link);
        await _db.SaveChangesAsync();

        TempData["AlertType"] = "info";
        TempData["AlertTitle"] = "Удалено";
        TempData["AlertMessage"] = "Цех отвязан от продукта.";
        return RedirectToPage(new { productId });
    }

    private async Task<IActionResult> LoadAsync(int productId)
    {
        var product = await _db.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == productId);
        if (product is null)
        {
            return RedirectToPage("/Products/Index");
        }

        ProductTitle = $"{product.Article} – {product.Name}";

        Rows = await _db.ProductWorkshops
            .AsNoTracking()
            .Where(x => x.ProductId == productId)
            .Include(x => x.Workshop)
            .OrderBy(x => x.Workshop!.Name)
            .Select(x => new Row
            {
                ProductWorkshopId = x.Id,
                WorkshopName = x.Workshop!.Name,
                PeopleCount = x.Workshop!.PeopleCount,
                Minutes = x.Minutes
            })
            .ToListAsync();

        return Page();
    }
}
