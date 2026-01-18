using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tnt_wpf_children.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusForAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Status",
                table: "Admins",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Admins");
        }
    }
}
