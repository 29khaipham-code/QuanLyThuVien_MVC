using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace QuanLyThuVien.Models;

public partial class QuanLyThuVienContext : DbContext
{
    public QuanLyThuVienContext()
    {
    }

    public QuanLyThuVienContext(DbContextOptions<QuanLyThuVienContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BanSaoSach> BanSaoSaches { get; set; }

    public virtual DbSet<ChiTietMuon> ChiTietMuons { get; set; }

    public virtual DbSet<DatTruoc> DatTruocs { get; set; }

    public virtual DbSet<DocGium> DocGia { get; set; }

    public virtual DbSet<LoaiDocGium> LoaiDocGia { get; set; }

    public virtual DbSet<LuotDocSachDienTu> LuotDocSachDienTus { get; set; }

    public virtual DbSet<NhaXuatBan> NhaXuatBans { get; set; }

    public virtual DbSet<NhatKyTruyCap> NhatKyTruyCaps { get; set; }

    public virtual DbSet<Phat> Phats { get; set; }

    public virtual DbSet<PhieuMuon> PhieuMuons { get; set; }

    public virtual DbSet<Sach> Saches { get; set; }

    public virtual DbSet<TacGium> TacGia { get; set; }

    public virtual DbSet<TaiKhoan> TaiKhoans { get; set; }

    public virtual DbSet<TepSachDienTu> TepSachDienTus { get; set; }

    public virtual DbSet<ThanhLy> ThanhLies { get; set; }

    public virtual DbSet<TheLoai> TheLoais { get; set; }

    public virtual DbSet<VaiTro> VaiTros { get; set; }

    public virtual DbSet<VwSoLuongSach> VwSoLuongSaches { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:QuanLyThuVienConnectionString");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Vietnamese_CI_AS");

        modelBuilder.Entity<BanSaoSach>(entity =>
        {
            entity.ToTable("BanSaoSach");

            entity.HasIndex(e => new { e.SachId, e.TrangThai }, "IX_BanSaoSach_SachId_TrangThai");

            entity.HasIndex(e => e.MaVach, "UQ_BanSaoSach_MaVach").IsUnique();

            entity.Property(e => e.GiaTri).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.MaVach)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TinhTrang)
                .HasMaxLength(30)
                .HasDefaultValue("Tot", "DF_BanSao_TinhTrang");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(30)
                .HasDefaultValue("SanSang", "DF_BanSao_TrangThai");
            entity.Property(e => e.ViTri).HasMaxLength(100);

            entity.HasOne(d => d.Sach).WithMany(p => p.BanSaoSaches)
                .HasForeignKey(d => d.SachId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BanSaoSach_Sach");
        });

        modelBuilder.Entity<ChiTietMuon>(entity =>
        {
            entity.ToTable("ChiTietMuon");

            entity.HasIndex(e => new { e.PhieuMuonId, e.BanSaoId }, "UQ_ChiTietMuon_Phieu_BanSao").IsUnique();

            entity.HasIndex(e => new { e.BanSaoId, e.NgayTra }, "UX_ChiTietMuon_BanSao_NgayTra").IsUnique();

            entity.Property(e => e.GhiChu).HasMaxLength(500);
            entity.Property(e => e.TinhTrangSau).HasMaxLength(255);
            entity.Property(e => e.TinhTrangTruoc).HasMaxLength(255);

            entity.HasOne(d => d.BanSao).WithMany(p => p.ChiTietMuons)
                .HasForeignKey(d => d.BanSaoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChiTietMuon_BanSao");

            entity.HasOne(d => d.PhieuMuon).WithMany(p => p.ChiTietMuons)
                .HasForeignKey(d => d.PhieuMuonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChiTietMuon_PhieuMuon");
        });

        modelBuilder.Entity<DatTruoc>(entity =>
        {
            entity.ToTable("DatTruoc");

            entity.HasIndex(e => e.BanSaoGiuId, "IX_DatTruoc_BanSaoGiuId");

            entity.HasIndex(e => new { e.SachId, e.NgayDat }, "IX_DatTruoc_Sach_HangDoi");

            entity.HasIndex(e => new { e.DocGiaId, e.SachId }, "UX_DatTruoc_ConHieuLuc")
                .IsUnique()
                .HasFilter("([TrangThai] IN (N'DangCho', N'DaGiu'))");

            entity.Property(e => e.NgayDat).HasDefaultValueSql("(sysdatetime())", "DF_DatTruoc_NgayDat");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(30)
                .HasDefaultValue("DangCho", "DF_DatTruoc_TrangThai");

            entity.HasOne(d => d.BanSaoGiu).WithMany(p => p.DatTruocs)
                .HasForeignKey(d => d.BanSaoGiuId)
                .HasConstraintName("FK_DatTruoc_BanSao");

            entity.HasOne(d => d.DocGia).WithMany(p => p.DatTruocs)
                .HasForeignKey(d => d.DocGiaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DatTruoc_DocGia");

            entity.HasOne(d => d.Sach).WithMany(p => p.DatTruocs)
                .HasForeignKey(d => d.SachId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DatTruoc_Sach");
        });

        modelBuilder.Entity<DocGium>(entity =>
        {
            entity.HasIndex(e => e.LoaiDocGiaId, "IX_DocGia_LoaiDocGiaId");

            entity.HasIndex(e => e.MaDocGia, "UQ_DocGia_MaDocGia").IsUnique();

            entity.HasIndex(e => e.TaiKhoanId, "UX_DocGia_TaiKhoanId")
                .IsUnique()
                .HasFilter("([TaiKhoanId] IS NOT NULL)");

            entity.Property(e => e.DiaChi).HasMaxLength(255);
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.GioiTinh).HasMaxLength(10);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.MaDocGia)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.NgayDangKy).HasDefaultValueSql("(CONVERT([date],sysdatetime()))", "DF_DocGia_NgayDangKy");
            entity.Property(e => e.SoDienThoai)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(30)
                .HasDefaultValue("HoatDong", "DF_DocGia_TrangThai");

            entity.HasOne(d => d.LoaiDocGia).WithMany(p => p.DocGia)
                .HasForeignKey(d => d.LoaiDocGiaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DocGia_LoaiDocGia");

            entity.HasOne(d => d.TaiKhoan).WithOne(p => p.DocGium)
                .HasForeignKey<DocGium>(d => d.TaiKhoanId)
                .HasConstraintName("FK_DocGia_TaiKhoan");
        });

        modelBuilder.Entity<LoaiDocGium>(entity =>
        {
            entity.HasIndex(e => e.TenLoai, "UQ_LoaiDocGia_TenLoai").IsUnique();

            entity.Property(e => e.MoTa).HasMaxLength(255);
            entity.Property(e => e.TenLoai).HasMaxLength(100);
        });

        modelBuilder.Entity<LuotDocSachDienTu>(entity =>
        {
            entity.ToTable("LuotDocSachDienTu");

            entity.HasIndex(e => e.TepSachId, "IX_LuotDoc_TepSachId");

            entity.HasIndex(e => new { e.DocGiaId, e.TepSachId }, "UX_LuotDoc_HieuLuc")
                .IsUnique()
                .HasFilter("([TrangThai]=N'HieuLuc')");

            entity.Property(e => e.NgayBatDau).HasDefaultValueSql("(sysdatetime())", "DF_LuotDoc_NgayBatDau");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(30)
                .HasDefaultValue("HieuLuc", "DF_LuotDoc_TrangThai");

            entity.HasOne(d => d.DocGia).WithMany(p => p.LuotDocSachDienTus)
                .HasForeignKey(d => d.DocGiaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LuotDoc_DocGia");

            entity.HasOne(d => d.TepSach).WithMany(p => p.LuotDocSachDienTus)
                .HasForeignKey(d => d.TepSachId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LuotDoc_TepSach");
        });

        modelBuilder.Entity<NhaXuatBan>(entity =>
        {
            entity.ToTable("NhaXuatBan");

            entity.Property(e => e.DiaChi).HasMaxLength(255);
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.SoDienThoai)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TenNhaXuatBan).HasMaxLength(150);
            entity.Property(e => e.Website)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<NhatKyTruyCap>(entity =>
        {
            entity.ToTable("NhatKyTruyCap");

            entity.HasIndex(e => new { e.LuotDocId, e.ThoiDiem }, "IX_NhatKy_LuotDocId_ThoiDiem");

            entity.Property(e => e.DiaChiIp)
                .HasMaxLength(45)
                .IsUnicode(false)
                .HasColumnName("DiaChiIP");
            entity.Property(e => e.HanhDong).HasMaxLength(30);
            entity.Property(e => e.ThoiDiem).HasDefaultValueSql("(sysdatetime())", "DF_NhatKy_ThoiDiem");

            entity.HasOne(d => d.LuotDoc).WithMany(p => p.NhatKyTruyCaps)
                .HasForeignKey(d => d.LuotDocId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_NhatKy_LuotDoc");
        });

        modelBuilder.Entity<Phat>(entity =>
        {
            entity.ToTable("Phat");

            entity.HasIndex(e => new { e.ChiTietMuonId, e.LoaiPhat }, "UX_Phat_ChiTiet_Loai").IsUnique();

            entity.Property(e => e.LoaiPhat).HasMaxLength(50);
            entity.Property(e => e.LyDo).HasMaxLength(500);
            entity.Property(e => e.NgayTao).HasDefaultValueSql("(sysdatetime())", "DF_Phat_NgayTao");
            entity.Property(e => e.SoTien).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.ChiTietMuon).WithMany(p => p.Phats)
                .HasForeignKey(d => d.ChiTietMuonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Phat_ChiTietMuon");
        });

        modelBuilder.Entity<PhieuMuon>(entity =>
        {
            entity.ToTable("PhieuMuon");

            entity.HasIndex(e => e.DocGiaId, "IX_PhieuMuon_DocGiaId");

            entity.HasIndex(e => e.NhanVienLapId, "IX_PhieuMuon_NhanVienLapId");

            entity.HasIndex(e => e.MaPhieu, "UQ_PhieuMuon_MaPhieu").IsUnique();

            entity.Property(e => e.GhiChu).HasMaxLength(500);
            entity.Property(e => e.MaPhieu)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.NgayMuon).HasDefaultValueSql("(CONVERT([date],sysdatetime()))", "DF_PhieuMuon_NgayMuon");
            entity.Property(e => e.NgayTao).HasDefaultValueSql("(sysdatetime())", "DF_PhieuMuon_NgayTao");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(30)
                .HasDefaultValue("DangMuon", "DF_PhieuMuon_TrangThai");

            entity.HasOne(d => d.DocGia).WithMany(p => p.PhieuMuons)
                .HasForeignKey(d => d.DocGiaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PhieuMuon_DocGia");

            entity.HasOne(d => d.NhanVienLap).WithMany(p => p.PhieuMuons)
                .HasForeignKey(d => d.NhanVienLapId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PhieuMuon_NhanVien");
        });

        modelBuilder.Entity<Sach>(entity =>
        {
            entity.ToTable("Sach");

            entity.HasIndex(e => e.NhaXuatBanId, "IX_Sach_NhaXuatBanId");

            entity.HasIndex(e => e.TheLoaiId, "IX_Sach_TheLoaiId");

            entity.HasIndex(e => e.Isbn, "UX_Sach_ISBN")
                .IsUnique()
                .HasFilter("([ISBN] IS NOT NULL)");

            entity.Property(e => e.AnhBiaUrl)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Isbn)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("ISBN");
            entity.Property(e => e.NgayTao).HasDefaultValueSql("(sysdatetime())", "DF_Sach_NgayTao");
            entity.Property(e => e.TieuDe).HasMaxLength(255);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(30)
                .HasDefaultValue("HoatDong", "DF_Sach_TrangThai");

            entity.HasOne(d => d.NhaXuatBan).WithMany(p => p.Saches)
                .HasForeignKey(d => d.NhaXuatBanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Sach_NhaXuatBan");

            entity.HasOne(d => d.TheLoai).WithMany(p => p.Saches)
                .HasForeignKey(d => d.TheLoaiId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Sach_TheLoai");

            entity.HasMany(d => d.TacGia).WithMany(p => p.Saches)
                .UsingEntity<Dictionary<string, object>>(
                    "SachTacGium",
                    r => r.HasOne<TacGium>().WithMany()
                        .HasForeignKey("TacGiaId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_SachTacGia_TacGia"),
                    l => l.HasOne<Sach>().WithMany()
                        .HasForeignKey("SachId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_SachTacGia_Sach"),
                    j =>
                    {
                        j.HasKey("SachId", "TacGiaId");
                        j.ToTable("SachTacGia");
                        j.HasIndex(new[] { "TacGiaId" }, "IX_SachTacGia_TacGiaId");
                    });
        });

        modelBuilder.Entity<TacGium>(entity =>
        {
            entity.Property(e => e.QuocTich).HasMaxLength(100);
            entity.Property(e => e.TenTacGia).HasMaxLength(150);
        });

        modelBuilder.Entity<TaiKhoan>(entity =>
        {
            entity.ToTable("TaiKhoan");

            entity.HasIndex(e => e.VaiTroId, "IX_TaiKhoan_VaiTroId");

            entity.HasIndex(e => e.Email, "UQ_TaiKhoan_Email").IsUnique();

            entity.HasIndex(e => e.TenDangNhap, "UQ_TaiKhoan_TenDangNhap").IsUnique();

            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.HoatDong).HasDefaultValue(true, "DF_TaiKhoan_HoatDong");
            entity.Property(e => e.MatKhauHash)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.NgayTao).HasDefaultValueSql("(sysdatetime())", "DF_TaiKhoan_NgayTao");
            entity.Property(e => e.SoDienThoai)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TenDangNhap)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.VaiTro).WithMany(p => p.TaiKhoans)
                .HasForeignKey(d => d.VaiTroId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TaiKhoan_VaiTro");
        });

        modelBuilder.Entity<TepSachDienTu>(entity =>
        {
            entity.ToTable("TepSachDienTu");

            entity.HasIndex(e => e.SachId, "IX_TepSach_SachId");

            entity.Property(e => e.CheDoTruyCap)
                .HasMaxLength(30)
                .HasDefaultValue("DocTrucTuyen", "DF_TepSach_CheDo");
            entity.Property(e => e.DinhDang)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.DuongDanTep).HasMaxLength(500);
            entity.Property(e => e.NgayTaiLen).HasDefaultValueSql("(sysdatetime())", "DF_TepSach_NgayTaiLen");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(30)
                .HasDefaultValue("HoatDong", "DF_TepSach_TrangThai");

            entity.HasOne(d => d.Sach).WithMany(p => p.TepSachDienTus)
                .HasForeignKey(d => d.SachId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TepSach_Sach");
        });

        modelBuilder.Entity<ThanhLy>(entity =>
        {
            entity.ToTable("ThanhLy");

            entity.HasIndex(e => e.NhanVienId, "IX_ThanhLy_NhanVienId");

            entity.HasIndex(e => e.BanSaoId, "UQ_ThanhLy_BanSaoId").IsUnique();

            entity.Property(e => e.LyDo).HasMaxLength(500);
            entity.Property(e => e.NgayThanhLy).HasDefaultValueSql("(CONVERT([date],sysdatetime()))", "DF_ThanhLy_Ngay");

            entity.HasOne(d => d.BanSao).WithOne(p => p.ThanhLy)
                .HasForeignKey<ThanhLy>(d => d.BanSaoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ThanhLy_BanSao");

            entity.HasOne(d => d.NhanVien).WithMany(p => p.ThanhLies)
                .HasForeignKey(d => d.NhanVienId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ThanhLy_NhanVien");
        });

        modelBuilder.Entity<TheLoai>(entity =>
        {
            entity.ToTable("TheLoai");

            entity.HasIndex(e => e.TenTheLoai, "UQ_TheLoai_Ten").IsUnique();

            entity.Property(e => e.MoTa).HasMaxLength(255);
            entity.Property(e => e.TenTheLoai).HasMaxLength(100);
        });

        modelBuilder.Entity<VaiTro>(entity =>
        {
            entity.ToTable("VaiTro");

            entity.HasIndex(e => e.TenVaiTro, "UQ_VaiTro_TenVaiTro").IsUnique();

            entity.Property(e => e.MoTa).HasMaxLength(255);
            entity.Property(e => e.TenVaiTro).HasMaxLength(50);
        });

        modelBuilder.Entity<VwSoLuongSach>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_SoLuongSach");

            entity.Property(e => e.TieuDe).HasMaxLength(255);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
