using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PaymentGateway.Migrations
{
    public partial class Addtagsforterminal : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SpTerminalViewModel");

            migrationBuilder.CreateTable(
                name: "TagTerminals",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<string>(nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", nullable: false),
                    Description = table.Column<string>(type: "varchar(200)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagTerminals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TagTerminals_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TagTerminalConnections",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TagId = table.Column<int>(nullable: false),
                    TerminalId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagTerminalConnections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TagTerminalConnections_TagTerminals_TagId",
                        column: x => x.TagId,
                        principalTable: "TagTerminals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TagTerminalConnections_Terminal_TerminalId",
                        column: x => x.TerminalId,
                        principalTable: "Terminal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TagTerminalConnections_TagId",
                table: "TagTerminalConnections",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_TagTerminalConnections_TerminalId",
                table: "TagTerminalConnections",
                column: "TerminalId");

            migrationBuilder.CreateIndex(
                name: "IX_TagTerminals_UserId",
                table: "TagTerminals",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TagTerminalConnections");

            migrationBuilder.DropTable(
                name: "TagTerminals");

            migrationBuilder.CreateTable(
                name: "SpTerminalViewModel",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MerchantId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    SerialNumber = table.Column<string>(nullable: true),
                    Status = table.Column<sbyte>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpTerminalViewModel", x => x.Id);
                });
        }
    }
}
