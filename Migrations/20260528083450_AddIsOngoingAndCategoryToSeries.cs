using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddIsOngoingAndCategoryToSeries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Series",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsOngoing",
                table: "Series",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Series_CategoryId",
                table: "Series",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Series_Categories_CategoryId",
                table: "Series",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Series_Categories_CategoryId",
                table: "Series");

            migrationBuilder.DropIndex(
                name: "IX_Series_CategoryId",
                table: "Series");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Series");

            migrationBuilder.DropColumn(
                name: "IsOngoing",
                table: "Series");
        }
    }
}
