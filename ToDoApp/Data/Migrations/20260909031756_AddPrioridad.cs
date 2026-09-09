using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToDoApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPrioridad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // defaultValue: 1 = TodoPrioridad.Media, no el 0 que EF scaffoldea por
            // default (CLR default del enum, no el "= TodoPrioridad.Media" del modelo
            // en C#) — ver SPRINT5.md: las tareas existentes deben quedar en Media,
            // el mismo valor neutro que ya usan las tareas nuevas.
            migrationBuilder.AddColumn<int>(
                name: "Prioridad",
                table: "TodoItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Prioridad",
                table: "TodoItems");
        }
    }
}
