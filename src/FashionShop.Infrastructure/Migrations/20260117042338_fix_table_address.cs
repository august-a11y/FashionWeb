using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FashionShop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class fix_table_address : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullName",
                table: "Address");

            migrationBuilder.RenameColumn(
                name: "PhoneNumber",
                table: "Address",
                newName: "FullNameAddress");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FullNameAddress",
                table: "Address",
                newName: "PhoneNumber");

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "Address",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
