using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LibraryManagement.Migrations
{
    /// <inheritdoc />
    public partial class UpdateInitial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Series",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Series",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Series",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Name", "ParentId" },
                values: new object[] { "DAM MY", 1 });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Name", "ParentId" },
                values: new object[] { "NGON TINH", 2 });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Name", "ParentId" },
                values: new object[] { "VAN HOC VIET NAM", 3 });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Name", "ParentId" },
                values: new object[] { "VAN HOC CHAU A", 4 });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Name", "ParentId" },
                values: new object[] { "VAN HOC NUOC NGOAI", 5 });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Name", "ParentId" },
                values: new object[] { "SACH NGOAI VAN", 6 });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Name", "ParentId" },
                values: new object[] { "TRUYEN TRANH", 7 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Name", "ParentId" },
                values: new object[] { "Văn học", null });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Name", "ParentId" },
                values: new object[] { "Kỹ năng", null });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Name", "ParentId" },
                values: new object[] { "Công nghệ", null });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Name", "ParentId" },
                values: new object[] { "Văn học Việt Nam", 1 });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Name", "ParentId" },
                values: new object[] { "Văn học nước ngoài", 1 });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Name", "ParentId" },
                values: new object[] { "Tiểu thuyết trinh thám", 5 });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Name", "ParentId" },
                values: new object[] { "Ngôn tình", 5 });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Description", "Name", "ParentId" },
                values: new object[,]
                {
                    { 8, null, "Khoa học viễn tưởng", 5 },
                    { 9, null, "Phát triển bản thân", 2 },
                    { 10, null, "Kinh doanh", 2 }
                });

            migrationBuilder.InsertData(
                table: "Series",
                columns: new[] { "Id", "Author", "Description", "Name", "Publisher", "TotalVolumes" },
                values: new object[,]
                {
                    { 1, "Arthur Conan Doyle", null, "Sherlock Holmes", null, 4 },
                    { 2, "J.K. Rowling", null, "Harry Potter", null, 7 },
                    { 3, "Frank Herbert", null, "Dune", null, 6 }
                });
        }
    }
}
