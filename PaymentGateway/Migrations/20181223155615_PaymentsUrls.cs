using Microsoft.EntityFrameworkCore.Migrations;

namespace PaymentGateway.Migrations
{
    public partial class PaymentsUrls : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CallbackUrl",
                table: "Payment",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancelUrl",
                table: "Payment",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompleatesUrl",
                table: "Payment",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CallbackUrl",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "CancelUrl",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "CompleatesUrl",
                table: "Payment");
        }
    }
}
