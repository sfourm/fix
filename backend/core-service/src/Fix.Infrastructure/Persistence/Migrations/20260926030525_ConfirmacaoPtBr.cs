using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fix.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ConfirmacaoPtBr : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "roles",
                keyColumn: "id",
                keyValue: new Guid("cb7df096-8268-3ca4-808d-7ac2e51a96da"),
                column: "description",
                value: "Registrar e reconciliar confirmações (middle office)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "roles",
                keyColumn: "id",
                keyValue: new Guid("cb7df096-8268-3ca4-808d-7ac2e51a96da"),
                column: "description",
                value: "Registrar e reconciliar confirmations (middle office)");
        }
    }
}
