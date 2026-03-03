using System.ComponentModel.DataAnnotations;
using FurnitureProductSystem.Web.Data;
using FurnitureProductSystem.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FurnitureProductSystem.Web.Pages.Products;

public sealed class EditModel : PageModel
{
    private readonly AppDbContext _db;

    public EditModel(AppDbContext db)
    {
        _db = db;
    }

    

    private void SetAlert(string type, string title, string message)
    {
        TempData["AlertType"] = type;
        TempData["AlertTitle"] = title;
        TempData["AlertMessage"] = message;
    }
[BindProperty]
    public InputModel Input { get; set; } = new();

    public bool IsEdit => Input.Id is not null;
    public string? ErrorText { get; private set; }

    public SelectList ProductTypeOptions { get; private set; } = default!;
    public SelectList MaterialTypeOptions { get; private set; } = default!;

    public sealed class InputModel
    {
        public int? Id { get; set; }

        [Required, StringLength(64)]
        [Display(Name = "Артикул")]
        public string Article { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Тип продукта")]
        public int ProductTypeId { get; set; }

        [Required, StringLength(200)]
        [Display(Name = "Наименование")]
        public string Name { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "Стоимость не может быть отрицательной")]
        [Display(Name = "Минимальная стоимость для партнера")]
        public decimal MinPartnerCost { get; set; }

        [Required]
        [Display(Name = "Основной материал")]
        public int MaterialTypeId { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        await LoadSelectsAsync();

        if (id is null)
        {
            return Page();
        }

        var p = await _db.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id.Value);
        if (p is null)
        {
            SetAlert("error", "Ошибка", "Продукт не найден. Возможно, он был удалён.");
            return RedirectToPage("/Products/Index");
        }

        Input = new InputModel
        {
            Id = p.Id,
            Article = p.Article,
            ProductTypeId = p.ProductTypeId,
            Name = p.Name,
            MinPartnerCost = p.MinPartnerCost,
            MaterialTypeId = p.MaterialTypeId
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadSelectsAsync();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            if (Input.Id is null)
            {
                var p = new Product
                {
                    Article = Input.Article.Trim(),
                    ProductTypeId = Input.ProductTypeId,
                    Name = Input.Name.Trim(),
                    MinPartnerCost = Input.MinPartnerCost,
                    MaterialTypeId = Input.MaterialTypeId
                };

                _db.Products.Add(p);
            }
            else
            {
                var p = await _db.Products.FirstOrDefaultAsync(x => x.Id == Input.Id.Value);
                if (p is null)
                {
                    SetAlert("error", "Ошибка", "Продукт не найден. Возможно, он был удалён.");
                    return RedirectToPage("/Products/Index");
                }

                p.Article = Input.Article.Trim();
                p.ProductTypeId = Input.ProductTypeId;
                p.Name = Input.Name.Trim();
                p.MinPartnerCost = Input.MinPartnerCost;
                p.MaterialTypeId = Input.MaterialTypeId;
            }

            await _db.SaveChangesAsync();
            SetAlert("info", "Готово", Input.Id is null ? "Продукт добавлен." : "Изменения сохранены.");
            return RedirectToPage("/Products/Index");
        }
        catch (DbUpdateException ex)
        {
            // Typical exam-friendly message: tell user what happened and what to do.
            SetAlert("error", "Ошибка сохранения", "Не удалось сохранить данные. Проверьте поля и повторите попытку. Детали: " + ex.GetBaseException().Message);
            return Page();
        }
    }

    private async Task LoadSelectsAsync()
    {
        var types = await _db.ProductTypes.AsNoTracking().OrderBy(x => x.Name).ToListAsync();
        var mats = await _db.MaterialTypes.AsNoTracking().OrderBy(x => x.Name).ToListAsync();

        ProductTypeOptions = new SelectList(types, nameof(ProductType.Id), nameof(ProductType.Name));
        MaterialTypeOptions = new SelectList(mats, nameof(MaterialType.Id), nameof(MaterialType.Name));

        // Default to first options on create
        if (Input.Id is null)
        {
            if (Input.ProductTypeId == 0 && types.Count > 0) Input.ProductTypeId = types[0].Id;
            if (Input.MaterialTypeId == 0 && mats.Count > 0) Input.MaterialTypeId = mats[0].Id;
        }
    }
}
