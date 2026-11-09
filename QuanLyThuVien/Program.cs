
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien.Models;
using QuanLyThuVien.Services;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<QuanLyThuVien.ViewModels.SachCoverService>();

// 1. Đăng ký dịch vụ MVC
builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// 2. Lấy chuỗi kết nối từ appsettings.json
var connectionString = builder.Configuration
    .GetConnectionString("QuanLyThuVienConnectionString");

// 3. Đăng ký DbContext kết nối SQL Server
builder.Services.AddDbContext<QuanLyThuVienContext>(options =>
    options.UseSqlServer(connectionString));

// 4. Cấu hình đăng nhập bằng Cookie
builder.Services
    .AddAuthentication(
        CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";

        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;

        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy =
            CookieSecurePolicy.Always;
        options.Cookie.SameSite =
            SameSiteMode.Lax;
    });

// 5. Đăng ký dịch vụ mã hóa và kiểm tra mật khẩu
builder.Services.AddScoped<
    IPasswordHasher<TaiKhoan>,
    PasswordHasher<TaiKhoan>>();

// 6. Tạo ứng dụng sau khi đăng ký tất cả dịch vụ
builder.Services.AddScoped<
    QuanLyThuVien.Services.DashboardThongKeService>();
builder.Services.AddScoped<QuanLyThuVien.Services.SachDienTuAdminService>();

builder.Services.AddScoped<SachCoverService>();
// dki dich vu thong bao
builder.Services.AddMemoryCache();
builder.Services.AddScoped<ThongBaoService>();
builder.Services.AddHostedService<ThongBaoBackgroundService>();



var app = builder.Build();

// 7. Cấu hình HTTP Request Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}


app.UseHttpsRedirection();

// Phục vụ ảnh trong wwwroot
app.UseStaticFiles();

app.UseRouting();

// Kích hoạt Session
app.UseSession();

// Xác thực và phân quyền
app.UseAuthentication();
app.UseAuthorization();


// 9. Cấu hình tài nguyên tĩnh
app.MapStaticAssets();

app.MapAreaControllerRoute(
    name: "admin",
    areaName: "Admin",
    pattern: "Admin/{controller=Dashboard}/{action=Index}/{id?}");

app.MapAreaControllerRoute(
    name: "thuthu",
    areaName: "ThuThu",
    pattern: "ThuThu/{controller=Home}/{action=Index}/{id?}");

app.MapAreaControllerRoute(
    name: "docgia",
    areaName: "DocGia",
    pattern: "DocGia/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// 10. Cấu hình đường dẫn Controller và Action
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// 11. Chạy ứng dụng
app.Run();
