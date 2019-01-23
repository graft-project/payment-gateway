using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PaymentGateway.Migrations
{
    public partial class RemoveAppParam : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppParam");

            migrationBuilder.DropTable(
                name: "StimulusTransaction");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppParam",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", nullable: false),
                    Value = table.Column<string>(type: "varchar(1000)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppParam", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StimulusTransaction",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Amount = table.Column<decimal>(nullable: false),
                    BlockNumber = table.Column<int>(nullable: false),
                    Error = table.Column<string>(nullable: true),
                    PayWalletAddress = table.Column<string>(type: "varchar(200)", nullable: true),
                    RecvWalletAddress = table.Column<string>(type: "varchar(200)", nullable: true),
                    Status = table.Column<sbyte>(nullable: false),
                    TransactionDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StimulusTransaction", x => x.Id);
                });
        }
    }
}
