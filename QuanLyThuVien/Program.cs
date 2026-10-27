
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using QuanLyThuVien.Models;

var builder = WebApplication.CreateBuilder(args);

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
var app = builder.Build();
// KHỞI TẠO TÀI KHOẢN QUẢN TRỊ - CHỈ DÙNG KHI PHÁT TRIỂN
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();

    var db = scope.ServiceProvider
        .GetRequiredService<QuanLyThuVienContext>();

    var hasher = scope.ServiceProvider
        .GetRequiredService<IPasswordHasher<TaiKhoan>>();

    var vaiTro = db.VaiTros
        .FirstOrDefault(x => x.TenVaiTro == "QuanTri");

    if (vaiTro == null)
        throw new Exception("Chưa có vai trò QuanTri");

    if (!db.TaiKhoans.Any(x => x.TenDangNhap == "admin"))
    {
        // Thay bằng mật khẩu mạnh chỉ bạn biết
        var matKhau = builder.Configuration["BootstrapAdminPassword"];

        if (string.IsNullOrWhiteSpace(matKhau))
            throw new Exception(
                "Chưa cấu hình BootstrapAdminPassword");

        var admin = new TaiKhoan
        {
            TenDangNhap = "admin",
            HoTen = "Quản trị hệ thống",
            Email = "admin@example.invalid",
            HoatDong = true,
            NgayTao = DateTime.Now,
            VaiTroId = vaiTro.Id,
            MatKhauHash = ""
        };

        admin.MatKhauHash =
            hasher.HashPassword(admin, matKhau);

        db.TaiKhoans.Add(admin);
        db.SaveChanges();
    }
}

// 7. Cấu hình HTTP Request Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseRouting();

// Kích hoạt Session
app.UseSession();

// Xác thực và phân quyền
app.UseAuthentication();
app.UseAuthorization();


// 9. Cấu hình tài nguyên tĩnh
app.MapStaticAssets();
//9.5
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}"
);
// 10. Cấu hình đường dẫn Controller và Action
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// 11. Chạy ứng dụng
app.Run();
