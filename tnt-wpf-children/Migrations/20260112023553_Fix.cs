using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tnt_wpf_children.Migrations
{
    /// <inheritdoc />
    public partial class Fix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Face",
                table: "Relatives");

            migrationBuilder.AddColumn<byte[]>(
                name: "FaceEmbedding",
                table: "Relatives",
                type: "BLOB",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FaceEmbedding",
                table: "Relatives");

            migrationBuilder.AddColumn<byte[]>(
                name: "Face",
                table: "Relatives",
                type: "BLOB",
                nullable: false,
                defaultValue: new byte[0]);
        }
    }
}
