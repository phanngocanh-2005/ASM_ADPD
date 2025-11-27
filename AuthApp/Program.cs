using Microsoft.EntityFrameworkCore;
using AuthApp.Data;
using Microsoft.AspNetCore.Authentication.Cookies; // ⭐ CẦN THÊM DÒNG NÀY ⭐

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
        )
);

// ⭐ BỔ SUNG BƯỚC 1: ĐĂNG KÝ DỊCH VỤ AUTHENTICATION ⭐
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "AuthCookie";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
        // Tùy chọn: Đặt trang đăng nhập mặc định
        // options.LoginPath = "/Login/Index"; 
    });


builder.Services.AddSession(
    option => option.IdleTimeout = TimeSpan.FromMinutes(5)
    );

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
app.UseRouting();

// ⭐ BỔ SUNG BƯỚC 2: THÊM MIDDLEWARE AUTHENTICATION (Phải đặt trước UseAuthorization) ⭐
app.UseAuthentication();

app.UseAuthorization(); // Giữ nguyên vị trí sau UseAuthentication

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();