using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecomm_project_1145_2.DataAccess.Migrations
{
    public partial class AddColumnTotalOrderSellForProduct : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotalOrderSell",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalOrderSell",
                table: "Products");
        }
    }
}
