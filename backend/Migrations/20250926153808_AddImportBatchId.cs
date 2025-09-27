using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatGPTcorpus.Migrations
{
    /// <inheritdoc />
    public partial class AddImportBatchId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImportBatchId",
                table: "Conversations",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImportBatchId",
                table: "Conversations");
        }
    }
}
