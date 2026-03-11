using FurnitureProductSystem.Web.Data;
using FurnitureProductSystem.Web.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("AppDb")));

builder.Services.AddScoped<ProductTimeService>();
builder.Services.AddScoped<RawMaterialCalculator>();

var app = builder.Build();

// Убедитесь, что папка базы данных существует
Directory.CreateDirectory(Path.Combine(app.Environment.ContentRootPath, "App_Data"));

// Автоматическое создание и заполнение базы данных
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    DbInitializer.Seed(db, app.Environment.ContentRootPath);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();

app.Run();