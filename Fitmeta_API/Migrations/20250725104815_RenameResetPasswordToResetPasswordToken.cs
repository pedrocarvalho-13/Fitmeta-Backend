using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fitmeta_API.Migrations
{
    /// <inheritdoc />
    public partial class RenameResetPasswordToResetPasswordToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ResetPassword",
                table: "Usuarios",
                newName: "ResetPasswordToken");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ResetPasswordToken",
                table: "Usuarios",
                newName: "ResetPassword");
        }
    }
}
