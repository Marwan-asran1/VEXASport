using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VEXA.Migrations
{
    /// <inheritdoc />
    public partial class ConvertUserGenderToEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // First, add a temporary column
            migrationBuilder.AddColumn<int>(
                name: "GenderTemp",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Convert existing data
            migrationBuilder.Sql(@"
                UPDATE Users 
                SET GenderTemp = CASE 
                    WHEN Gender = 'Male' THEN 1
                    WHEN Gender = 'Female' THEN 2
                    WHEN Gender = 'Other' THEN 3
                    WHEN Gender = 'Men' THEN 1
                    WHEN Gender = 'Women' THEN 2
                    WHEN Gender = 'Kids' THEN 3
                    ELSE 0
                END");

            // Drop the old column
            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Users");

            // Rename the temporary column
            migrationBuilder.RenameColumn(
                name: "GenderTemp",
                table: "Users",
                newName: "Gender");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Add a temporary string column
            migrationBuilder.AddColumn<string>(
                name: "GenderTemp",
                table: "Users",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            // Convert data back
            migrationBuilder.Sql(@"
                UPDATE Users 
                SET GenderTemp = CASE 
                    WHEN Gender = 1 THEN 'Male'
                    WHEN Gender = 2 THEN 'Female'
                    WHEN Gender = 3 THEN 'Other'
                    ELSE 'Unspecified'
                END");

            // Drop the int column
            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Users");

            // Rename the temporary column
            migrationBuilder.RenameColumn(
                name: "GenderTemp",
                table: "Users",
                newName: "Gender");
        }
    }
}
