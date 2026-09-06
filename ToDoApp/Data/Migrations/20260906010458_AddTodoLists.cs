using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToDoApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTodoLists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCreacion",
                table: "TodoItems",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            // defaultValue: 1, no 0 (default de EF) — así las tareas del Sprint 1 que ya
            // existan en la base quedan asignadas a la lista predeterminada "Mis Tareas"
            // (sembrada más abajo con Id=1), en vez de apuntar a una lista inexistente.
            migrationBuilder.AddColumn<int>(
                name: "TodoListId",
                table: "TodoItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "TodoLists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    Orden = table.Column<int>(type: "INTEGER", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EsPredeterminada = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TodoLists", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "TodoLists",
                columns: new[] { "Id", "EsPredeterminada", "FechaCreacion", "Nombre", "Orden" },
                values: new object[] { 1, true, new DateTime(2026, 9, 5, 0, 0, 0, 0, DateTimeKind.Local), "Mis Tareas", 0 });

            migrationBuilder.CreateIndex(
                name: "IX_TodoItems_TodoListId",
                table: "TodoItems",
                column: "TodoListId");

            migrationBuilder.AddForeignKey(
                name: "FK_TodoItems_TodoLists_TodoListId",
                table: "TodoItems",
                column: "TodoListId",
                principalTable: "TodoLists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TodoItems_TodoLists_TodoListId",
                table: "TodoItems");

            migrationBuilder.DropTable(
                name: "TodoLists");

            migrationBuilder.DropIndex(
                name: "IX_TodoItems_TodoListId",
                table: "TodoItems");

            migrationBuilder.DropColumn(
                name: "FechaCreacion",
                table: "TodoItems");

            migrationBuilder.DropColumn(
                name: "TodoListId",
                table: "TodoItems");
        }
    }
}
