
using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyThuVien.Migrations
{
    public partial class ThemHoTroVaDangKyEmail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DangKyNhanTin",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(
                        type: "nvarchar(100)",
                        maxLength: 100,
                        nullable: false),
                    NgayDangKy = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: false,
                        defaultValueSql: "(sysdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DangKyNhanTin", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DangKyNhanTin_Email",
                table: "DangKyNhanTin",
                column: "Email",
                unique: true);

            migrationBuilder.CreateTable(
                name: "YeuCauHoTro",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HoTen = table.Column<string>(
                        type: "nvarchar(100)",
                        maxLength: 100,
                        nullable: false),
                    Email = table.Column<string>(
                        type: "nvarchar(100)",
                        maxLength: 100,
                        nullable: false),
                    ChuDe = table.Column<string>(
                        type: "nvarchar(100)",
                        maxLength: 100,
                        nullable: false),
                    LoiNhan = table.Column<string>(
                        type: "nvarchar(1000)",
                        maxLength: 1000,
                        nullable: false),
                    NgayGui = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: false,
                        defaultValueSql: "(sysdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YeuCauHoTro", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "DangKyNhanTin");
            migrationBuilder.DropTable(name: "YeuCauHoTro");
        }
    }
}
