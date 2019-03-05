using Microsoft.EntityFrameworkCore.Migrations;

namespace PaymentGateway.Migrations
{
    public partial class Payment_StatusMessage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StatusMessage",
                table: "Payment",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StatusMessage",
                table: "Payment");
        }
    }
}
