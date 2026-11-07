namespace QuanLyThuVien.Services;

// Quét thông báo cho mọi độc giả định kỳ, kể cả khi họ không mở website
public class ThongBaoBackgroundService : BackgroundService
{
    private static readonly TimeSpan ChuKy = TimeSpan.FromMinutes(10);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ThongBaoBackgroundService> _logger;

    public ThongBaoBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<ThongBaoBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Chờ ứng dụng khởi động xong rồi mới quét lần đầu
        try { await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken); }
        catch (OperationCanceledException) { return; }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var dichVu = scope.ServiceProvider.GetRequiredService<ThongBaoService>();

                var soMoi = await dichVu.TaoThongBaoAsync();
                if (soMoi > 0)
                    _logger.LogInformation("Đã tạo {SoMoi} thông báo mới", soMoi);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Ví dụ chưa tạo bảng ThongBao hoặc mất kết nối DB: ghi log và thử lại ở chu kỳ sau
                _logger.LogError(ex, "Quét thông báo định kỳ bị lỗi");
            }

            try { await Task.Delay(ChuKy, stoppingToken); }
            catch (OperationCanceledException) { break; }
        }
    }
}
