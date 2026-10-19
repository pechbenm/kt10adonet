using KT10.Data;
using KT10.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using KT10.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase("UsersDb"));
builder.Services.AddSingleton<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseWhen(ctx => ctx.Request.Path.StartsWithSegments("/api"), api =>
    api.UseExceptionHandler(errorApp => errorApp.Run(context =>
        Results.Problem(title: "¬нутренн€€ ошибка сервера",
                        statusCode: StatusCodes.Status500InternalServerError)
               .ExecuteAsync(context))));

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var users = scope.ServiceProvider.GetRequiredService<IUserService>();
    await users.CreateAsync("Ivan", "ivan@example.com", "Password1");
    await users.CreateAsync("Anna", "anna@example.com", "Password2");
}

app.Run();