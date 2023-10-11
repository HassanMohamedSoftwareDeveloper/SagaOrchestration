using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SagaOrchestrator.Migrations
{
    /// <inheritdoc />
    public partial class AddStateField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "State",
                schema: "ORDERS",
                table: "OrderStates",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "State",
                schema: "ORDERS",
                table: "OrderStates");
        }
    }
}
