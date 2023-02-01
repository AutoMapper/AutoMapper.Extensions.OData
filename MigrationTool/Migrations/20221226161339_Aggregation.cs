using Microsoft.EntityFrameworkCore.Migrations;

namespace MigrationTool.Migrations
{
    public partial class Aggregation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // As described in OData Extension for "Data Aggregation Version 4.0/2.2 Example Data Model":
            // https://docs.oasis-open.org/odata/odata-data-aggregation-ext/v4.0/cs02/odata-data-aggregation-ext-v4.0-cs02.html#_Toc435016564

            migrationBuilder.CreateTable(
                name: "TblCategory",
                columns: table => new
                {
                    FldId = table.Column<string>(nullable: false),
                    FldName = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblCategory", x => x.FldId);
                });

            migrationBuilder.CreateTable(
                name: "TblCurrency",
                columns: table => new
                {
                    FldCode = table.Column<string>(nullable: false),
                    FldName = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblCurrency", x => x.FldCode);
                });

            migrationBuilder.CreateTable(
                name: "TblCustomer",
                columns: table => new
                {
                    FldId = table.Column<string>(nullable: false),
                    FldName = table.Column<string>(nullable: false),
                    FldCountry = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblCustomer", x => x.FldId);
                });

            migrationBuilder.CreateTable(
                name: "TblSalesOrganization",
                columns: table => new
                {
                    FldId = table.Column<string>(nullable: false),
                    FldName = table.Column<string>(nullable: false),
                    FldSuperordinateId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblSalesOrganization", x => x.FldId);
                    table.ForeignKey(
                        name: "FK_TblSalesOrganization_TblSalesOrganization_FldSuperordinateId",
                        column: x => x.FldSuperordinateId,
                        principalTable: "TblSalesOrganization",
                        principalColumn: "FldId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TblTime",
                columns: table => new
                {
                    FldDate = table.Column<string>(nullable: false),
                    FldMonth = table.Column<string>(nullable: false),
                    FldQuarter = table.Column<string>(nullable: false),
                    FldYear = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblTime", x => x.FldDate);
                });

            migrationBuilder.CreateTable(
                name: "TblProduct",
                columns: table => new
                {
                    FldId = table.Column<string>(nullable: false),
                    FldCategoryId = table.Column<string>(nullable: false),
                    FldName = table.Column<string>(nullable: false),
                    FldColor = table.Column<string>(nullable: false),
                    FldTaxRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblProduct", x => x.FldId);
                    table.ForeignKey(
                        name: "FK_TblProduct_TblCategory_FldCategoryId",
                        column: x => x.FldCategoryId,
                        principalTable: "TblCategory",
                        principalColumn: "FldId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TblSales",
                columns: table => new
                {
                    FldId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FldCustomerId = table.Column<string>(nullable: false),
                    FldTimeId = table.Column<string>(nullable: false),
                    FldProductId = table.Column<string>(nullable: false),
                    FldSalesOrganizationId = table.Column<string>(nullable: false),
                    FldCurrencyCode = table.Column<string>(nullable: false),
                    FldAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblSales", x => x.FldId);
                    table.ForeignKey(
                        name: "FK_TblSales_TblCurrency_FldCurrencyCode",
                        column: x => x.FldCurrencyCode,
                        principalTable: "TblCurrency",
                        principalColumn: "FldCode",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TblSales_TblCustomer_FldCustomerId",
                        column: x => x.FldCustomerId,
                        principalTable: "TblCustomer",
                        principalColumn: "FldId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TblSales_TblProduct_FldProductId",
                        column: x => x.FldProductId,
                        principalTable: "TblProduct",
                        principalColumn: "FldId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TblSales_TblSalesOrganization_FldSalesOrganizationId",
                        column: x => x.FldSalesOrganizationId,
                        principalTable: "TblSalesOrganization",
                        principalColumn: "FldId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TblSales_TblTime_FldTimeId",
                        column: x => x.FldTimeId,
                        principalTable: "TblTime",
                        principalColumn: "FldDate",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TblProduct_FldCategoryId",
                table: "TblProduct",
                column: "FldCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_TblSales_FldCurrencyCode",
                table: "TblSales",
                column: "FldCurrencyCode");

            migrationBuilder.CreateIndex(
                name: "IX_TblSales_FldCustomerId",
                table: "TblSales",
                column: "FldCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_TblSales_FldProductId",
                table: "TblSales",
                column: "FldProductId");

            migrationBuilder.CreateIndex(
                name: "IX_TblSales_FldSalesOrganizationId",
                table: "TblSales",
                column: "FldSalesOrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_TblSales_FldTimeId",
                table: "TblSales",
                column: "FldTimeId");

            migrationBuilder.CreateIndex(
                name: "IX_TblSalesOrganization_FldSuperordinateId",
                table: "TblSalesOrganization",
                column: "FldSuperordinateId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TblSales");

            migrationBuilder.DropTable(
                name: "TblCurrency");

            migrationBuilder.DropTable(
                name: "TblCustomer");

            migrationBuilder.DropTable(
                name: "TblProduct");

            migrationBuilder.DropTable(
                name: "TblSalesOrganization");

            migrationBuilder.DropTable(
                name: "TblTime");

            migrationBuilder.DropTable(
                name: "TblCategory");
        }
    }
}
