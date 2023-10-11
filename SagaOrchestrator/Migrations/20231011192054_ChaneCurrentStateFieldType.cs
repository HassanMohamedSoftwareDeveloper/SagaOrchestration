using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SagaOrchestrator.Migrations
{
    /// <inheritdoc />
    public partial class ChaneCurrentStateFieldType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "CurrentState",
                schema: "ORDERS",
                table: "OrderStates",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CurrentState",
                schema: "ORDERS",
                table: "OrderStates",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
