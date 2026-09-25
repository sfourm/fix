using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fix.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class OrganizationChart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "parent_group_id",
                table: "organization_groups",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_organization_groups_parent_group_id",
                table: "organization_groups",
                column: "parent_group_id");

            // Grupos que já existiam passam a ficar abaixo da raiz (grupo default) da própria organização.
            migrationBuilder.Sql("""
                UPDATE organization_groups g
                SET parent_group_id = root.id
                FROM organization_groups root
                WHERE root.organization_id = g.organization_id
                  AND root.is_default
                  AND NOT g.is_default
                  AND g.parent_group_id IS NULL;
                """);

            migrationBuilder.AddForeignKey(
                name: "fk_organization_groups_organization_groups_parent_group_id",
                table: "organization_groups",
                column: "parent_group_id",
                principalTable: "organization_groups",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_organization_groups_organization_groups_parent_group_id",
                table: "organization_groups");

            migrationBuilder.DropIndex(
                name: "ix_organization_groups_parent_group_id",
                table: "organization_groups");

            migrationBuilder.DropColumn(
                name: "parent_group_id",
                table: "organization_groups");
        }
    }
}
