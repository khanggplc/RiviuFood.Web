using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RiviuFood.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddAreaToRestaurant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Area",
                table: "Restaurants",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Area",
                table: "Restaurants");
        }
    }
}
