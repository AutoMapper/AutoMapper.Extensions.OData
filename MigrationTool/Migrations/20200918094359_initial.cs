using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MigrationTool.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "G_TMandator",
                columns: table => new
                {
                    pkMandatorID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    gIdentity = table.Column<Guid>(nullable: false),
                    sName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_G_TMandator", x => x.pkMandatorID);
                });

            migrationBuilder.CreateTable(
                name: "TCities",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TCities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TBuilders",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true),
                    CityId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBuilders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TBuilders_TCities_CityId",
                        column: x => x.CityId,
                        principalTable: "TCities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OB_TBuilding",
                columns: table => new
                {
                    pkBID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Identifier = table.Column<Guid>(nullable: false),
                    sLongName = table.Column<string>(nullable: true),
                    BuilderId = table.Column<int>(nullable: false),
                    fkMandatorID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OB_TBuilding", x => x.pkBID);
                    table.ForeignKey(
                        name: "FK_OB_TBuilding_TBuilders_BuilderId",
                        column: x => x.BuilderId,
                        principalTable: "TBuilders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OB_TBuilding_G_TMandator_fkMandatorID",
                        column: x => x.fkMandatorID,
                        principalTable: "G_TMandator",
                        principalColumn: "pkMandatorID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OB_TBuilding_BuilderId",
                table: "OB_TBuilding",
                column: "BuilderId");

            migrationBuilder.CreateIndex(
                name: "IX_OB_TBuilding_fkMandatorID",
                table: "OB_TBuilding",
                column: "fkMandatorID");

            migrationBuilder.CreateIndex(
                name: "IX_TBuilders_CityId",
                table: "TBuilders",
                column: "CityId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OB_TBuilding");

            migrationBuilder.DropTable(
                name: "TBuilders");

            migrationBuilder.DropTable(
                name: "G_TMandator");

            migrationBuilder.DropTable(
                name: "TCities");
        }
    }
}
