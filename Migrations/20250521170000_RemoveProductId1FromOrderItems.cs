using Microsoft.EntityFrameworkCore.Migrations;

namespace VEXA.Migrations
{
    public partial class RemoveProductId1FromOrderItems : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Products_ProductId1",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_ProductId1",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "ProductId1",
                table: "OrderItems");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProductId1",
                table: "OrderItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ProductId1",
                table: "OrderItems",
                column: "ProductId1");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Products_ProductId1",
                table: "OrderItems",
                column: "ProductId1",
                principalTable: "Products",
                principalColumn: "Id");
        }
    }
} 