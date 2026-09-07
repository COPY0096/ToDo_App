using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToDoApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSubtasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParentTaskId",
                table: "TodoItems",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TodoItems_ParentTaskId",
                table: "TodoItems",
                column: "ParentTaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_TodoItems_TodoItems_ParentTaskId",
                table: "TodoItems",
                column: "ParentTaskId",
                principalTable: "TodoItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TodoItems_TodoItems_ParentTaskId",
                table: "TodoItems");

            migrationBuilder.DropIndex(
                name: "IX_TodoItems_ParentTaskId",
                table: "TodoItems");

            migrationBuilder.DropColumn(
                name: "ParentTaskId",
                table: "TodoItems");
        }
    }
}
