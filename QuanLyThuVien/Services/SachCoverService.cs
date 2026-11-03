
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace QuanLyThuVien.Services
{
    public class SachCoverService
    {
        private readonly IWebHostEnvironment _env;

        private const long MaxBytes = 2 * 1024 * 1024;

        public SachCoverService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> SaveAsync(IFormFile file)
        {
            if (file.Length == 0 || file.Length > MaxBytes)
            {
                throw new InvalidOperationException(
                    "Ảnh phải có dung lượng tối đa 2 MB.");
            }

            string ext = Path.GetExtension(file.FileName)
                .ToLowerInvariant();

            if (ext != ".jpg" && ext != ".jpeg" &&
                ext != ".png" && ext != ".webp")
            {
                throw new InvalidOperationException(
                    "Chỉ cho phép ảnh JPG, PNG hoặc WebP.");
            }

            byte[] header = new byte[12];
            int read = 0;

            await using (var input = file.OpenReadStream())
            {
                while (read < header.Length)
                {
                    int n = await input.ReadAsync(
                        header.AsMemory(read, header.Length - read));

                    if (n == 0) break;
                    read += n;
                }
            }

            bool jpg = read >= 3 &&
                header[0] == 255 &&
                header[1] == 216 &&
                header[2] == 255;

            bool png = read >= 8 &&
                header[0] == 137 &&
                header[1] == 80 &&
                header[2] == 78 &&
                header[3] == 71 &&
                header[4] == 13 &&
                header[5] == 10 &&
                header[6] == 26 &&
                header[7] == 10;

            bool webp = read >= 12 &&
                System.Text.Encoding.ASCII.GetString(
                    header, 0, 4) == "RIFF" &&
                System.Text.Encoding.ASCII.GetString(
                    header, 8, 4) == "WEBP";

            bool valid = ext switch
            {
                ".jpg" or ".jpeg" =>
                    jpg && file.ContentType == "image/jpeg",
                ".png" =>
                    png && file.ContentType == "image/png",
                ".webp" =>
                    webp && file.ContentType == "image/webp",
                _ => false
            };

            if (!valid)
            {
                throw new InvalidOperationException(
                    "Nội dung file không đúng định dạng ảnh.");
            }

            string root = _env.WebRootPath ??
                Path.Combine(_env.ContentRootPath, "wwwroot");

            string folder = Path.Combine(root, "uploads", "sach");

            Directory.CreateDirectory(folder);

            string name = Guid.NewGuid().ToString("N") + ext;
            string path = Path.Combine(folder, name);

            try
            {
                await using var input = file.OpenReadStream();

                await using var output = new FileStream(
                    path,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    81920,
                    useAsync: true);

                await input.CopyToAsync(output);
            }
            catch
            {
                if (File.Exists(path))
                    File.Delete(path);

                throw;
            }

            return "/uploads/sach/" + name;
        }

        public void DeleteIfLocal(string? url)
        {
            const string prefix = "/uploads/sach/";

            if (string.IsNullOrWhiteSpace(url) ||
                !url.StartsWith(prefix, StringComparison.Ordinal))
                return;

            string name = url[prefix.Length..];

            if (Path.GetFileName(name) != name)
                return;

            if (!Guid.TryParseExact(
                Path.GetFileNameWithoutExtension(name), "N", out _))
                return;

            string ext = Path.GetExtension(name).ToLowerInvariant();

            if (ext is not (".jpg" or ".jpeg" or ".png" or ".webp"))
                return;

            string root = _env.WebRootPath ??
                Path.Combine(_env.ContentRootPath, "wwwroot");

            string path = Path.Combine(
                root, "uploads", "sach", name);

            if (File.Exists(path))
                File.Delete(path);
        }
    }
}
