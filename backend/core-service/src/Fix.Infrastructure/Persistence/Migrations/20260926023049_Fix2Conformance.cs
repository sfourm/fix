using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fix.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Fix2Conformance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "limits_buyback_deadline_business_days",
                table: "policies",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "limits_buyback_trigger_pct",
                table: "policies",
                type: "numeric(6,2)",
                precision: 6,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "limits_confirmation_deadline_business_days",
                table: "policies",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "limits_contingency12months_pct",
                table: "policies",
                type: "numeric(6,2)",
                precision: 6,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "limits_contingency1month_pct",
                table: "policies",
                type: "numeric(6,2)",
                precision: 6,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "limits_contingency24months_pct",
                table: "policies",
                type: "numeric(6,2)",
                precision: 6,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "limits_contingency36months_pct",
                table: "policies",
                type: "numeric(6,2)",
                precision: 6,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "limits_contingency6months_pct",
                table: "policies",
                type: "numeric(6,2)",
                precision: 6,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "limits_deviation_report_hours",
                table: "policies",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "limits_mix_shift_max_pp",
                table: "policies",
                type: "numeric(6,2)",
                precision: 6,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "limits_pricing_cold_percentile",
                table: "policies",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "limits_pricing_hot_percentile",
                table: "policies",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "limits_registration_deadline_days",
                table: "policies",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "limits_stress_days",
                table: "policies",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "limits_stress_sigmas",
                table: "policies",
                type: "numeric(6,2)",
                precision: 6,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<Guid>(
                name: "mandate_id",
                table: "orders",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "compliance_reason",
                table: "orders",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "compliance_status",
                table: "orders",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "confirmation_by",
                table: "orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "covered_sale",
                table: "orders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "deviation_note",
                table: "orders",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "exceeds_mandate",
                table: "orders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "linked_after_execution",
                table: "orders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "number",
                table: "orders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "number",
                table: "mandates",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "number",
                table: "counterparties",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Dados existentes (antes dos índices únicos):
            // 1) numeração sequencial por organização na ordem de criação (ids são ordenados no tempo) — MD-01, HX-0001, CP-01;
            // 2) boletas já registradas ficam "dentro" (todas nasceram num mandato ativo e dentro do saldo);
            // 3) parâmetros novos das políticas recebem os padrões do modelo FIX2, não zero.
            foreach (var table in new[] { "orders", "mandates", "counterparties" })
            {
                migrationBuilder.Sql($"""
                    UPDATE {table} t SET number = s.n
                    FROM (SELECT id, row_number() OVER (PARTITION BY organization_id ORDER BY id) AS n FROM {table}) s
                    WHERE t.id = s.id;
                    """);
            }

            migrationBuilder.Sql("""
                UPDATE orders SET compliance_status = 'Within', compliance_reason = 'dentro do mandato (registrada antes do enquadramento da boleta)'
                WHERE compliance_status = '';
                """);

            migrationBuilder.Sql("""
                UPDATE policies SET
                    limits_contingency1month_pct = 2,
                    limits_contingency6months_pct = 5,
                    limits_contingency12months_pct = 10,
                    limits_contingency24months_pct = 20,
                    limits_contingency36months_pct = 40,
                    limits_buyback_trigger_pct = 110,
                    limits_buyback_deadline_business_days = 10,
                    limits_stress_sigmas = 3,
                    limits_stress_days = 10,
                    limits_pricing_hot_percentile = 70,
                    limits_pricing_cold_percentile = 30,
                    limits_mix_shift_max_pp = 5,
                    limits_confirmation_deadline_business_days = 2,
                    limits_registration_deadline_days = 0,
                    limits_deviation_report_hours = 24;
                """);

            migrationBuilder.CreateIndex(
                name: "ix_orders_organization_id_number",
                table: "orders",
                columns: new[] { "organization_id", "number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_mandates_organization_id_number",
                table: "mandates",
                columns: new[] { "organization_id", "number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_counterparties_organization_id_number",
                table: "counterparties",
                columns: new[] { "organization_id", "number" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_orders_organization_id_number",
                table: "orders");

            migrationBuilder.DropIndex(
                name: "ix_mandates_organization_id_number",
                table: "mandates");

            migrationBuilder.DropIndex(
                name: "ix_counterparties_organization_id_number",
                table: "counterparties");

            migrationBuilder.DropColumn(
                name: "limits_buyback_deadline_business_days",
                table: "policies");

            migrationBuilder.DropColumn(
                name: "limits_buyback_trigger_pct",
                table: "policies");

            migrationBuilder.DropColumn(
                name: "limits_confirmation_deadline_business_days",
                table: "policies");

            migrationBuilder.DropColumn(
                name: "limits_contingency12months_pct",
                table: "policies");

            migrationBuilder.DropColumn(
                name: "limits_contingency1month_pct",
                table: "policies");

            migrationBuilder.DropColumn(
                name: "limits_contingency24months_pct",
                table: "policies");

            migrationBuilder.DropColumn(
                name: "limits_contingency36months_pct",
                table: "policies");

            migrationBuilder.DropColumn(
                name: "limits_contingency6months_pct",
                table: "policies");

            migrationBuilder.DropColumn(
                name: "limits_deviation_report_hours",
                table: "policies");

            migrationBuilder.DropColumn(
                name: "limits_mix_shift_max_pp",
                table: "policies");

            migrationBuilder.DropColumn(
                name: "limits_pricing_cold_percentile",
                table: "policies");

            migrationBuilder.DropColumn(
                name: "limits_pricing_hot_percentile",
                table: "policies");

            migrationBuilder.DropColumn(
                name: "limits_registration_deadline_days",
                table: "policies");

            migrationBuilder.DropColumn(
                name: "limits_stress_days",
                table: "policies");

            migrationBuilder.DropColumn(
                name: "limits_stress_sigmas",
                table: "policies");

            migrationBuilder.DropColumn(
                name: "compliance_reason",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "compliance_status",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "confirmation_by",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "covered_sale",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "deviation_note",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "exceeds_mandate",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "linked_after_execution",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "number",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "number",
                table: "mandates");

            migrationBuilder.DropColumn(
                name: "number",
                table: "counterparties");

            migrationBuilder.AlterColumn<Guid>(
                name: "mandate_id",
                table: "orders",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
