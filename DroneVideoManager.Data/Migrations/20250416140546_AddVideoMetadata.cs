using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DroneVideoManager.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVideoMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VideoMetadata",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VideoFileId = table.Column<int>(type: "INTEGER", nullable: false),
                    BitRate = table.Column<long>(type: "INTEGER", nullable: false),
                    ColorSpace = table.Column<string>(type: "TEXT", nullable: false),
                    ColorDepth = table.Column<int>(type: "INTEGER", nullable: false),
                    VideoCodec = table.Column<string>(type: "TEXT", nullable: false),
                    IsVariableFrameRate = table.Column<bool>(type: "INTEGER", nullable: false),
                    AudioCodec = table.Column<string>(type: "TEXT", nullable: false),
                    AudioChannels = table.Column<int>(type: "INTEGER", nullable: false),
                    AudioSampleRate = table.Column<int>(type: "INTEGER", nullable: false),
                    AudioBitRate = table.Column<long>(type: "INTEGER", nullable: false),
                    CameraModel = table.Column<string>(type: "TEXT", nullable: false),
                    CameraSettings = table.Column<string>(type: "TEXT", nullable: false),
                    Category = table.Column<string>(type: "TEXT", nullable: false),
                    ExactCreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RecordingMode = table.Column<string>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoMetadata", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VideoMetadata_VideoFiles_VideoFileId",
                        column: x => x.VideoFileId,
                        principalTable: "VideoFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VideoMetadata_VideoFileId",
                table: "VideoMetadata",
                column: "VideoFileId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VideoMetadata");
        }
    }
}
