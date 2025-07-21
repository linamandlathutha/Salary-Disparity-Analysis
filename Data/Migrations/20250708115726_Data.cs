using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salary_Disparity_Analysis.Data.Migrations
{
    /// <inheritdoc />
    public partial class Data : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MockDatas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ip_address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    industry = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    gender = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    location = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    years_experience = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    job_tittle = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MockDatas", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MockDatas");
        }
    }
}
