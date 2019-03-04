using Microsoft.EntityFrameworkCore.Migrations;

namespace PaymentGateway.Migrations
{
    public partial class ConvertToStable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BrokerGraftWallet",
                table: "Payment",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ConvertToStableBlockNumber",
                table: "Payment",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ConvertToStableTxId",
                table: "Payment",
                nullable: true);

            migrationBuilder.AddColumn<sbyte>(
                name: "ConvertToStableTxStatus",
                table: "Payment",
                nullable: false,
                defaultValue: (sbyte)0);

            migrationBuilder.AddColumn<string>(
                name: "ConvertToStableTxStatusDescription",
                table: "Payment",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BrokerGraftWallet",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "ConvertToStableBlockNumber",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "ConvertToStableTxId",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "ConvertToStableTxStatus",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "ConvertToStableTxStatusDescription",
                table: "Payment");
        }
    }
}
