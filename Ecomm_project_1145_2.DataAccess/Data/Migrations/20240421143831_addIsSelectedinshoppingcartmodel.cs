using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecomm_project_1145_2.DataAccess.Migrations
{
    public partial class addIsSelectedinshoppingcartmodel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSelected",
                table: "ShoppingCarts",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSelected",
                table: "ShoppingCarts");
        }
    }
}
