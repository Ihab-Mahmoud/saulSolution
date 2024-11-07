using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Saul.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class FixShoppingcardTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OrderTotal",
                table: "ShoppingCards",
                newName: "Count");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Count",
                table: "ShoppingCards",
                newName: "OrderTotal");
        }
    }
}
