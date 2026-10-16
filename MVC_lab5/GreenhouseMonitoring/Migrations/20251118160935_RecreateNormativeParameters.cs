using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GreenhouseMonitoring.Migrations
{
    public partial class RecreateNormativeParameters : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NormativeParameters",
                columns: table => new
                {
                    ParameterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CropID = table.Column<int>(type: "int", nullable: false),
                    Parameter = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    MinValue = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    MaxValue = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    GrowthStage = table.Column<string>(type: "nvarchar(50)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NormativeParameters", x => x.ParameterID);
                    table.ForeignKey(
                        name: "FK_NormativeParameters_Crops_CropID",
                        column: x => x.CropID,
                        principalTable: "Crops",
                        principalColumn: "CropID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NormativeParameters_CropID",
                table: "NormativeParameters",
                column: "CropID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "NormativeParameters");
        }
    }
}