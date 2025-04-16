using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DroneVideoManager.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCameraMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CameraMetadata",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TelemetryPointId = table.Column<int>(type: "INTEGER", nullable: false),
                    ISO = table.Column<int>(type: "INTEGER", nullable: false),
                    ShutterSpeed = table.Column<string>(type: "TEXT", nullable: false),
                    Aperture = table.Column<double>(type: "REAL", nullable: false),
                    ExposureValue = table.Column<int>(type: "INTEGER", nullable: false),
                    ColorTemperature = table.Column<int>(type: "INTEGER", nullable: false),
                    ColorMode = table.Column<string>(type: "TEXT", nullable: false),
                    FocalLength = table.Column<int>(type: "INTEGER", nullable: false),
                    DigitalZoomRatio = table.Column<int>(type: "INTEGER", nullable: false),
                    DigitalZoomDelta = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CameraMetadata", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CameraMetadata_TelemetryPoints_TelemetryPointId",
                        column: x => x.TelemetryPointId,
                        principalTable: "TelemetryPoints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CameraMetadata_TelemetryPointId",
                table: "CameraMetadata",
                column: "TelemetryPointId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CameraMetadata");
        }
    }
}
