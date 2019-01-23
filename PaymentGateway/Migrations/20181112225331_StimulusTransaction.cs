using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PaymentGateway.Migrations
{
    public partial class StimulusTransaction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StimulusTransaction",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    BlockNumber = table.Column<int>(nullable: false),
                    Amount = table.Column<decimal>(nullable: false),
                    TransactionDate = table.Column<DateTime>(nullable: false),
                    Status = table.Column<sbyte>(nullable: false),
                    PayWalletAddress = table.Column<string>(type: "varchar(200)", nullable: true),
                    RecvWalletAddress = table.Column<string>(type: "varchar(200)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StimulusTransaction", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StimulusTransaction");
        }
    }
}
