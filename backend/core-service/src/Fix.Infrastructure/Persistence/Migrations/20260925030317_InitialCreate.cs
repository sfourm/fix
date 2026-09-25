using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fix.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "identity_roles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identity_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "organizations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    slug = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    author_created = table.Column<Guid>(type: "uuid", nullable: true),
                    author_updated = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    budget_cash_cost = table.Column<decimal>(type: "numeric(12,4)", precision: 12, scale: 4, nullable: true),
                    budget_economic_floor = table.Column<decimal>(type: "numeric(12,4)", precision: 12, scale: 4, nullable: true),
                    budget_equivalent_price = table.Column<decimal>(type: "numeric(12,4)", precision: 12, scale: 4, nullable: true),
                    budget_target_margin_pct = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: true),
                    financials_cash = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    financials_credit_lines = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    financials_ebitda = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    financials_monthly_fixed_cost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    financials_net_debt = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    financials_reference_date = table.Column<DateOnly>(type: "date", nullable: true),
                    financials_usd_debt = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    industrial_milling_capacity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    industrial_mix_guidance_pct = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: true),
                    industrial_mix_max_pct = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: true),
                    industrial_mix_min_pct = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: true),
                    profile_active_crop = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    profile_corporate_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    profile_crop_year_start_month = table.Column<int>(type: "integer", nullable: false),
                    profile_group = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    profile_headquarters = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    profile_sector = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    profile_tax_id = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_organizations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    author_created = table.Column<Guid>(type: "uuid", nullable: true),
                    author_updated = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rules",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    author_created = table.Column<Guid>(type: "uuid", nullable: true),
                    author_updated = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rules", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "timelines",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: true),
                    entity_type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    action = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    snapshot = table.Column<string>(type: "jsonb", nullable: false),
                    author_id = table.Column<Guid>(type: "uuid", nullable: true),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_timelines", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    full_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: true),
                    security_stamp = table.Column<string>(type: "text", nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true),
                    phone_number = table.Column<string>(type: "text", nullable: true),
                    phone_number_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    two_factor_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    lockout_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    lockout_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    access_failed_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "identity_role_claims",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: true),
                    claim_value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identity_role_claims", x => x.id);
                    table.ForeignKey(
                        name: "fk_identity_role_claims_identity_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "identity_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "counterparties",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    document = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    country = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    is_homologated = table.Column<bool>(type: "boolean", nullable: false),
                    notional_limit_usd = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    mtm_limit_usd = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    author_created = table.Column<Guid>(type: "uuid", nullable: true),
                    author_updated = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_counterparties", x => x.id);
                    table.ForeignKey(
                        name: "fk_counterparties_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "organization_commodities",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    commodity = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    capacity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    unit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    price_reference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    sells = table.Column<bool>(type: "boolean", nullable: false),
                    author_created = table.Column<Guid>(type: "uuid", nullable: true),
                    author_updated = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_organization_commodities", x => x.id);
                    table.ForeignKey(
                        name: "fk_organization_commodities_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "organization_groups",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    is_default = table.Column<bool>(type: "boolean", nullable: false),
                    author_created = table.Column<Guid>(type: "uuid", nullable: true),
                    author_updated = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_organization_groups", x => x.id);
                    table.ForeignKey(
                        name: "fk_organization_groups_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "policies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    approval_record = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    approved_on = table.Column<DateOnly>(type: "date", nullable: true),
                    author_created = table.Column<Guid>(type: "uuid", nullable: true),
                    author_updated = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    limits_absolute_ceiling_pct = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    limits_covered_call_max_pct = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    limits_financial_concentration_max_pct = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    limits_freight_ceiling_pct = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    limits_fx_fixed_max_pct = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    limits_fx_fixed_min_pct = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    limits_fx_unfixed_max_pct = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    limits_hedge_horizon_years = table.Column<int>(type: "integer", nullable: false),
                    limits_logistics_deadline_months = table.Column<int>(type: "integer", nullable: false),
                    limits_margin_cash_max_pct = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    limits_physical_concentration_max_pct = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    valid_to = table.Column<DateOnly>(type: "date", nullable: true),
                    valid_from = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_policies", x => x.id);
                    table.ForeignKey(
                        name: "fk_policies_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "rule_roles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    rule_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    author_created = table.Column<Guid>(type: "uuid", nullable: true),
                    author_updated = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rule_roles", x => x.id);
                    table.ForeignKey(
                        name: "fk_rule_roles_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_rule_roles_rules_rule_id",
                        column: x => x.rule_id,
                        principalTable: "rules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "identity_user_claims",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: true),
                    claim_value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identity_user_claims", x => x.id);
                    table.ForeignKey(
                        name: "fk_identity_user_claims_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "identity_user_logins",
                columns: table => new
                {
                    login_provider = table.Column<string>(type: "text", nullable: false),
                    provider_key = table.Column<string>(type: "text", nullable: false),
                    provider_display_name = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identity_user_logins", x => new { x.login_provider, x.provider_key });
                    table.ForeignKey(
                        name: "fk_identity_user_logins_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "identity_user_roles",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identity_user_roles", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "fk_identity_user_roles_identity_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "identity_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_identity_user_roles_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "identity_user_tokens",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    login_provider = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identity_user_tokens", x => new { x.user_id, x.login_provider, x.name });
                    table.ForeignKey(
                        name: "fk_identity_user_tokens_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "organization_members",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    desk = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    author_created = table.Column<Guid>(type: "uuid", nullable: true),
                    author_updated = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_organization_members", x => x.id);
                    table.ForeignKey(
                        name: "fk_organization_members_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_organization_members_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "policy_axes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    policy_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    factor = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    statement = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    limit_description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    approver = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    restrictions = table.Column<string[]>(type: "text[]", nullable: false),
                    author_created = table.Column<Guid>(type: "uuid", nullable: true),
                    author_updated = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_policy_axes", x => x.id);
                    table.ForeignKey(
                        name: "fk_policy_axes_policies_policy_id",
                        column: x => x.policy_id,
                        principalTable: "policies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "policy_coverage_bands",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    policy_id = table.Column<Guid>(type: "uuid", nullable: false),
                    horizon = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    crop = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    min_pct = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    max_pct = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    note = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    author_created = table.Column<Guid>(type: "uuid", nullable: true),
                    author_updated = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_policy_coverage_bands", x => x.id);
                    table.ForeignKey(
                        name: "fk_policy_coverage_bands_policies_policy_id",
                        column: x => x.policy_id,
                        principalTable: "policies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "policy_instruments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    policy_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    permission = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    condition = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    author_created = table.Column<Guid>(type: "uuid", nullable: true),
                    author_updated = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_policy_instruments", x => x.id);
                    table.ForeignKey(
                        name: "fk_policy_instruments_policies_policy_id",
                        column: x => x.policy_id,
                        principalTable: "policies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "policy_versions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    policy_id = table.Column<Guid>(type: "uuid", nullable: false),
                    version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    note = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    author_created = table.Column<Guid>(type: "uuid", nullable: true),
                    author_updated = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_policy_versions", x => x.id);
                    table.ForeignKey(
                        name: "fk_policy_versions_policies_policy_id",
                        column: x => x.policy_id,
                        principalTable: "policies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "organization_group_members",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    member_id = table.Column<Guid>(type: "uuid", nullable: false),
                    author_created = table.Column<Guid>(type: "uuid", nullable: true),
                    author_updated = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_organization_group_members", x => x.id);
                    table.ForeignKey(
                        name: "fk_organization_group_members_organization_groups_group_id",
                        column: x => x.group_id,
                        principalTable: "organization_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_organization_group_members_organization_members_member_id",
                        column: x => x.member_id,
                        principalTable: "organization_members",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "organization_rules",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    rule_id = table.Column<Guid>(type: "uuid", nullable: false),
                    member_id = table.Column<Guid>(type: "uuid", nullable: true),
                    group_id = table.Column<Guid>(type: "uuid", nullable: true),
                    author_created = table.Column<Guid>(type: "uuid", nullable: true),
                    author_updated = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_organization_rules", x => x.id);
                    table.CheckConstraint("ck_organization_rules_single_target", "(member_id IS NULL) <> (group_id IS NULL)");
                    table.ForeignKey(
                        name: "fk_organization_rules_organization_groups_group_id",
                        column: x => x.group_id,
                        principalTable: "organization_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_organization_rules_organization_members_member_id",
                        column: x => x.member_id,
                        principalTable: "organization_members",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_organization_rules_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_organization_rules_rules_rule_id",
                        column: x => x.rule_id,
                        principalTable: "rules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "mandates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    policy_id = table.Column<Guid>(type: "uuid", nullable: false),
                    axis_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    criteria = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    commodity = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    tenor = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    quantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    quantity_unit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    window_start = table.Column<DateOnly>(type: "date", nullable: true),
                    window_end = table.Column<DateOnly>(type: "date", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    issued_by = table.Column<Guid>(type: "uuid", nullable: false),
                    decided_by = table.Column<Guid>(type: "uuid", nullable: true),
                    decided_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    decision_note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    author_created = table.Column<Guid>(type: "uuid", nullable: true),
                    author_updated = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    compliance_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    compliance_status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    price_at_market = table.Column<bool>(type: "boolean", nullable: false),
                    price_max = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    price_min = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    price_target = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    price_unit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mandates", x => x.id);
                    table.ForeignKey(
                        name: "fk_mandates_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_mandates_policies_policy_id",
                        column: x => x.policy_id,
                        principalTable: "policies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_mandates_policy_axis_axis_id",
                        column: x => x.axis_id,
                        principalTable: "policy_axes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    mandate_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    direction = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    counterparty_id = table.Column<Guid>(type: "uuid", nullable: false),
                    commodity = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    tenor = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    lots = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    notional_usd = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    price = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    price_unit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    option_kind = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    premium = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    trade_date = table.Column<DateOnly>(type: "date", nullable: false),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    approval = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    requested_by = table.Column<Guid>(type: "uuid", nullable: false),
                    decided_by = table.Column<Guid>(type: "uuid", nullable: true),
                    decided_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    decision_note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    confirmation = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    confirmed_on = table.Column<DateOnly>(type: "date", nullable: true),
                    confirmation_note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    author_created = table.Column<Guid>(type: "uuid", nullable: true),
                    author_updated = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_orders", x => x.id);
                    table.ForeignKey(
                        name: "fk_orders_counterparties_counterparty_id",
                        column: x => x.counterparty_id,
                        principalTable: "counterparties",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_orders_mandates_mandate_id",
                        column: x => x.mandate_id,
                        principalTable: "mandates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_orders_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "id", "author_created", "author_updated", "code", "created_at", "description", "updated_at" },
                values: new object[,]
                {
                    { new Guid("089360bf-26c1-cb8d-3730-b563e1d30452"), null, null, "view_mandate", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Visualizar mandatos", null },
                    { new Guid("0cb12fbb-961c-66af-bad6-52e04cc75baf"), null, null, "view_users", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Visualizar membros e grupos", null },
                    { new Guid("0e16a590-19da-5ccb-5c5e-002d6a1873e6"), null, null, "view_order", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Visualizar boletas", null },
                    { new Guid("4af1692d-b2ee-21c8-fb83-9fdcd08a7b3c"), null, null, "view_counterparties", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Visualizar contrapartes", null },
                    { new Guid("58f13f47-3286-967c-b3dd-f007105e62ef"), null, null, "update_order", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Editar boletas", null },
                    { new Guid("5a4ac9a6-1c8e-167b-9182-80cbf2db6de8"), null, null, "edit_organization", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Editar o setup da companhia, membros e grupos", null },
                    { new Guid("5c278eba-16e6-8478-54e3-4bbe261dc9cd"), null, null, "manage_counterparties", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Cadastrar, homologar e limitar contrapartes", null },
                    { new Guid("68941986-5142-5dc5-97ab-87a4dd78c29c"), null, null, "self_approve", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Alçada de emissão: operar sem passar pela fila de aprovação", null },
                    { new Guid("6d113c17-7ea6-e84c-0dea-420f5c8ae41a"), null, null, "create_order", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Registrar boletas", null },
                    { new Guid("84da2989-9a40-e6a5-968e-57cb026ddf54"), null, null, "update_mandate", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Editar e encerrar mandatos", null },
                    { new Guid("8a073b01-3457-fd52-fb35-97f11048f63f"), null, null, "view_policy", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Visualizar a política de riscos", null },
                    { new Guid("a16880ae-dbaa-9c47-15c1-73e006c2db60"), null, null, "approve_mandate", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Aprovar mandatos dentro da política", null },
                    { new Guid("a289ec56-a1c3-28c3-dfcd-38faba04e702"), null, null, "create_mandate", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Emitir mandatos", null },
                    { new Guid("a3e8139b-2a76-1335-88e4-bd3f2e0a0cc1"), null, null, "update_policy", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Editar política, eixos, bandas e instrumentos", null },
                    { new Guid("abb80d87-0568-1d25-a682-1dc3f4b9a923"), null, null, "approve_order", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Aprovar boletas", null },
                    { new Guid("b61e201c-e31e-b5f3-5b6b-734d52dd3236"), null, null, "approve_policy", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Aprovar política (ata) e abrir novas versões", null },
                    { new Guid("b7fdd907-0764-68a3-24d6-8a6015fcd025"), null, null, "delete_policy", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Excluir política em rascunho", null },
                    { new Guid("c7cdfc04-a77d-679a-06e8-d3e3f518b22c"), null, null, "delete_order", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Excluir boletas pendentes ou rejeitadas", null },
                    { new Guid("cb7df096-8268-3ca4-808d-7ac2e51a96da"), null, null, "manage_confirmation", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Registrar e reconciliar confirmations (middle office)", null },
                    { new Guid("e1479074-56fb-83e9-368b-e5a3e9b7a7cd"), null, null, "approve_exception", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Aprovar mandatos FORA da política (exceção)", null },
                    { new Guid("e649b00f-10f2-63a7-9d83-6d548d650fbb"), null, null, "create_policy", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Criar política de riscos", null },
                    { new Guid("f8947b81-ab97-7b45-36d7-367384e66e89"), null, null, "delete_mandate", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Excluir mandatos sem boletas", null }
                });

            migrationBuilder.InsertData(
                table: "rules",
                columns: new[] { "id", "author_created", "author_updated", "code", "created_at", "name", "updated_at" },
                values: new object[,]
                {
                    { new Guid("60e88433-a40c-0da3-e91a-78e398c6bf1c"), null, null, "user", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Usuário (leitura)", null },
                    { new Guid("7a01e8e4-272f-86a5-cbaa-5651bfdf24cc"), null, null, "founder", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Founder", null },
                    { new Guid("88f13fff-76a9-49b1-02d4-3a5dca0c4b03"), null, null, "middle_office", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Middle office (Controle de riscos)", null },
                    { new Guid("92acc37f-4dd4-c818-1aca-1b5993a088cd"), null, null, "administrador", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Administrador", null },
                    { new Guid("99e68c35-b651-8b6e-055c-a33759442737"), null, null, "operador", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Operador (Mesa)", null },
                    { new Guid("a78282c6-0826-0b3b-cb7c-58950284590e"), null, null, "gestor", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Gestor (Diretoria)", null },
                    { new Guid("eca4b1bc-3f11-091d-7220-6a4ac2047557"), null, null, "super_administrador", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Super administrador", null }
                });

            migrationBuilder.InsertData(
                table: "rule_roles",
                columns: new[] { "id", "author_created", "author_updated", "created_at", "role_id", "rule_id", "updated_at" },
                values: new object[,]
                {
                    { new Guid("0226157d-2de5-1445-fa59-75a9851e35c6"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("0e16a590-19da-5ccb-5c5e-002d6a1873e6"), new Guid("eca4b1bc-3f11-091d-7220-6a4ac2047557"), null },
                    { new Guid("060ef5b5-6396-b8fd-abda-f9a8b3145245"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("4af1692d-b2ee-21c8-fb83-9fdcd08a7b3c"), new Guid("99e68c35-b651-8b6e-055c-a33759442737"), null },
                    { new Guid("062bbe76-3dea-c178-6555-bb6ddd81b849"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("6d113c17-7ea6-e84c-0dea-420f5c8ae41a"), new Guid("99e68c35-b651-8b6e-055c-a33759442737"), null },
                    { new Guid("08d60ff4-ff17-a886-11a0-6cebd1ae178c"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("cb7df096-8268-3ca4-808d-7ac2e51a96da"), new Guid("7a01e8e4-272f-86a5-cbaa-5651bfdf24cc"), null },
                    { new Guid("09b7f998-4ce9-6e2f-e829-e03d7d3bcd25"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("0cb12fbb-961c-66af-bad6-52e04cc75baf"), new Guid("7a01e8e4-272f-86a5-cbaa-5651bfdf24cc"), null },
                    { new Guid("09f1f5bc-c2bf-38c6-6f59-3161ba6dbd72"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("089360bf-26c1-cb8d-3730-b563e1d30452"), new Guid("92acc37f-4dd4-c818-1aca-1b5993a088cd"), null },
                    { new Guid("0f28d31d-e28d-6896-1a9d-30e89d592161"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("b7fdd907-0764-68a3-24d6-8a6015fcd025"), new Guid("92acc37f-4dd4-c818-1aca-1b5993a088cd"), null },
                    { new Guid("109d46c5-b42b-12e4-6b19-0062e9f3cdf2"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("b7fdd907-0764-68a3-24d6-8a6015fcd025"), new Guid("7a01e8e4-272f-86a5-cbaa-5651bfdf24cc"), null },
                    { new Guid("114c7ab1-865f-87f6-7d6d-55078bf2e38d"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("58f13f47-3286-967c-b3dd-f007105e62ef"), new Guid("eca4b1bc-3f11-091d-7220-6a4ac2047557"), null },
                    { new Guid("16222baf-6167-12b5-0477-d1562522cc17"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("4af1692d-b2ee-21c8-fb83-9fdcd08a7b3c"), new Guid("eca4b1bc-3f11-091d-7220-6a4ac2047557"), null },
                    { new Guid("1ef99c34-8082-503e-e1d7-f82c4b6d59fb"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("0e16a590-19da-5ccb-5c5e-002d6a1873e6"), new Guid("a78282c6-0826-0b3b-cb7c-58950284590e"), null },
                    { new Guid("1f93cd4e-633c-8ff2-fe13-4b61e2a54d9b"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("b61e201c-e31e-b5f3-5b6b-734d52dd3236"), new Guid("7a01e8e4-272f-86a5-cbaa-5651bfdf24cc"), null },
                    { new Guid("20dcf68f-2aa5-c2dc-6897-8a1b78c70ecc"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("0cb12fbb-961c-66af-bad6-52e04cc75baf"), new Guid("a78282c6-0826-0b3b-cb7c-58950284590e"), null },
                    { new Guid("228cbd67-5e7d-dd46-114c-d9738761600e"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("84da2989-9a40-e6a5-968e-57cb026ddf54"), new Guid("a78282c6-0826-0b3b-cb7c-58950284590e"), null },
                    { new Guid("229d297f-ff4a-8a6e-3c91-ded11d09f1f3"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("089360bf-26c1-cb8d-3730-b563e1d30452"), new Guid("88f13fff-76a9-49b1-02d4-3a5dca0c4b03"), null },
                    { new Guid("261fcc97-edef-6ba8-0fcc-35c13890c158"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("a16880ae-dbaa-9c47-15c1-73e006c2db60"), new Guid("7a01e8e4-272f-86a5-cbaa-5651bfdf24cc"), null },
                    { new Guid("276a1da8-49c0-63b8-a56e-e8e76772427f"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("6d113c17-7ea6-e84c-0dea-420f5c8ae41a"), new Guid("a78282c6-0826-0b3b-cb7c-58950284590e"), null },
                    { new Guid("3668a920-7521-cc6e-5a4e-b2f3e214a5c7"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("e1479074-56fb-83e9-368b-e5a3e9b7a7cd"), new Guid("a78282c6-0826-0b3b-cb7c-58950284590e"), null },
                    { new Guid("37f3d158-a70c-d304-afe7-86539d887115"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("0e16a590-19da-5ccb-5c5e-002d6a1873e6"), new Guid("99e68c35-b651-8b6e-055c-a33759442737"), null },
                    { new Guid("3a3f0e26-66cc-5446-8996-efb8aa9a78c1"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("a16880ae-dbaa-9c47-15c1-73e006c2db60"), new Guid("a78282c6-0826-0b3b-cb7c-58950284590e"), null },
                    { new Guid("3a879884-ab30-4dff-16d4-b220df1fb5d6"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("8a073b01-3457-fd52-fb35-97f11048f63f"), new Guid("7a01e8e4-272f-86a5-cbaa-5651bfdf24cc"), null },
                    { new Guid("3b79f533-fc35-3410-0f48-8abfe6150fe8"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("84da2989-9a40-e6a5-968e-57cb026ddf54"), new Guid("7a01e8e4-272f-86a5-cbaa-5651bfdf24cc"), null },
                    { new Guid("40f9f578-e85b-17fd-0888-90a9bfc67079"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("e649b00f-10f2-63a7-9d83-6d548d650fbb"), new Guid("7a01e8e4-272f-86a5-cbaa-5651bfdf24cc"), null },
                    { new Guid("437fe351-1d3a-b1e0-517d-343bc75450f5"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("b61e201c-e31e-b5f3-5b6b-734d52dd3236"), new Guid("a78282c6-0826-0b3b-cb7c-58950284590e"), null },
                    { new Guid("4507422a-be9d-1d4c-47ce-bdaa4dc75611"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("b7fdd907-0764-68a3-24d6-8a6015fcd025"), new Guid("eca4b1bc-3f11-091d-7220-6a4ac2047557"), null },
                    { new Guid("48160b1d-1499-4b8d-dbc3-d329c901b956"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("84da2989-9a40-e6a5-968e-57cb026ddf54"), new Guid("eca4b1bc-3f11-091d-7220-6a4ac2047557"), null },
                    { new Guid("4e446593-929f-0d72-67b3-0454d687b20d"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("089360bf-26c1-cb8d-3730-b563e1d30452"), new Guid("99e68c35-b651-8b6e-055c-a33759442737"), null },
                    { new Guid("5037a7ed-ba59-3ec1-c133-257e255b0ac2"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("4af1692d-b2ee-21c8-fb83-9fdcd08a7b3c"), new Guid("92acc37f-4dd4-c818-1aca-1b5993a088cd"), null },
                    { new Guid("507b763f-8991-f79c-300e-594967dd78e9"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("f8947b81-ab97-7b45-36d7-367384e66e89"), new Guid("92acc37f-4dd4-c818-1aca-1b5993a088cd"), null },
                    { new Guid("50b62366-3ef4-fc9e-97ee-62d807d21f65"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("e1479074-56fb-83e9-368b-e5a3e9b7a7cd"), new Guid("eca4b1bc-3f11-091d-7220-6a4ac2047557"), null },
                    { new Guid("57cdcf66-dbc7-6f4a-a099-054442e18230"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("c7cdfc04-a77d-679a-06e8-d3e3f518b22c"), new Guid("eca4b1bc-3f11-091d-7220-6a4ac2047557"), null },
                    { new Guid("5abadf4a-1794-8d04-e8e2-e4d963f69815"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("5a4ac9a6-1c8e-167b-9182-80cbf2db6de8"), new Guid("eca4b1bc-3f11-091d-7220-6a4ac2047557"), null },
                    { new Guid("5af4df66-28a0-d963-b824-d8e92c82adb0"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("6d113c17-7ea6-e84c-0dea-420f5c8ae41a"), new Guid("7a01e8e4-272f-86a5-cbaa-5651bfdf24cc"), null },
                    { new Guid("5c7b65ea-7b20-4317-c8f4-0ecd9f05abdf"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("cb7df096-8268-3ca4-808d-7ac2e51a96da"), new Guid("88f13fff-76a9-49b1-02d4-3a5dca0c4b03"), null },
                    { new Guid("5c968571-c02f-b91b-25f9-9f2b5c278c79"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("a3e8139b-2a76-1335-88e4-bd3f2e0a0cc1"), new Guid("a78282c6-0826-0b3b-cb7c-58950284590e"), null },
                    { new Guid("5cc03b65-1a70-30ff-9b05-5ec156882856"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("c7cdfc04-a77d-679a-06e8-d3e3f518b22c"), new Guid("92acc37f-4dd4-c818-1aca-1b5993a088cd"), null },
                    { new Guid("5f2610b9-e549-0b4b-d531-637078987aab"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("58f13f47-3286-967c-b3dd-f007105e62ef"), new Guid("7a01e8e4-272f-86a5-cbaa-5651bfdf24cc"), null },
                    { new Guid("615b110a-43bb-dd5f-6796-b8192b25f709"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("e649b00f-10f2-63a7-9d83-6d548d650fbb"), new Guid("eca4b1bc-3f11-091d-7220-6a4ac2047557"), null },
                    { new Guid("62e8ed8e-7e76-0d5d-5f10-4003c01a6bb5"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("a289ec56-a1c3-28c3-dfcd-38faba04e702"), new Guid("eca4b1bc-3f11-091d-7220-6a4ac2047557"), null },
                    { new Guid("68c04523-c8b0-6dd1-a432-aa5ba291e886"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("a289ec56-a1c3-28c3-dfcd-38faba04e702"), new Guid("7a01e8e4-272f-86a5-cbaa-5651bfdf24cc"), null },
                    { new Guid("72a8f396-9e6d-eee0-730d-2542d1d77902"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("a3e8139b-2a76-1335-88e4-bd3f2e0a0cc1"), new Guid("eca4b1bc-3f11-091d-7220-6a4ac2047557"), null },
                    { new Guid("7861f371-802a-085a-a4a2-90733c4c00b8"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("0cb12fbb-961c-66af-bad6-52e04cc75baf"), new Guid("92acc37f-4dd4-c818-1aca-1b5993a088cd"), null },
                    { new Guid("79299849-bd6b-7b24-c915-4a60e4f9ac32"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("6d113c17-7ea6-e84c-0dea-420f5c8ae41a"), new Guid("eca4b1bc-3f11-091d-7220-6a4ac2047557"), null },
                    { new Guid("79f4263c-8243-1f82-f228-16d7b56277e0"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("68941986-5142-5dc5-97ab-87a4dd78c29c"), new Guid("92acc37f-4dd4-c818-1aca-1b5993a088cd"), null },
                    { new Guid("83308d38-7c72-4897-34be-8b7efe531c16"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("58f13f47-3286-967c-b3dd-f007105e62ef"), new Guid("a78282c6-0826-0b3b-cb7c-58950284590e"), null },
                    { new Guid("836eef9f-46f8-eba5-6107-a91597f50e64"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("a289ec56-a1c3-28c3-dfcd-38faba04e702"), new Guid("a78282c6-0826-0b3b-cb7c-58950284590e"), null },
                    { new Guid("86a11b0b-1c84-5c18-8b27-def8f4951a83"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("58f13f47-3286-967c-b3dd-f007105e62ef"), new Guid("99e68c35-b651-8b6e-055c-a33759442737"), null },
                    { new Guid("87803ea1-1076-cb86-50e2-5dbec238f432"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("8a073b01-3457-fd52-fb35-97f11048f63f"), new Guid("88f13fff-76a9-49b1-02d4-3a5dca0c4b03"), null },
                    { new Guid("8818697b-9e14-1f98-d336-783ad486c88f"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("8a073b01-3457-fd52-fb35-97f11048f63f"), new Guid("99e68c35-b651-8b6e-055c-a33759442737"), null },
                    { new Guid("8956be42-6573-d57d-bb49-30c5fbe7c9c5"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("84da2989-9a40-e6a5-968e-57cb026ddf54"), new Guid("92acc37f-4dd4-c818-1aca-1b5993a088cd"), null },
                    { new Guid("89915608-af84-13fb-3fed-4033ee492145"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("089360bf-26c1-cb8d-3730-b563e1d30452"), new Guid("a78282c6-0826-0b3b-cb7c-58950284590e"), null },
                    { new Guid("8a1d977f-acf3-db98-c396-ad29d3b0c20e"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("4af1692d-b2ee-21c8-fb83-9fdcd08a7b3c"), new Guid("a78282c6-0826-0b3b-cb7c-58950284590e"), null },
                    { new Guid("8e8af506-92fd-cd41-53d7-4a0f8830d5e0"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("5c278eba-16e6-8478-54e3-4bbe261dc9cd"), new Guid("7a01e8e4-272f-86a5-cbaa-5651bfdf24cc"), null },
                    { new Guid("920e913b-4226-d286-931e-21a5e79c014b"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("089360bf-26c1-cb8d-3730-b563e1d30452"), new Guid("7a01e8e4-272f-86a5-cbaa-5651bfdf24cc"), null },
                    { new Guid("953132af-c0b0-17e1-9cd3-85abff0e5943"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("4af1692d-b2ee-21c8-fb83-9fdcd08a7b3c"), new Guid("60e88433-a40c-0da3-e91a-78e398c6bf1c"), null },
                    { new Guid("969e13f0-5548-b36f-2ae1-2644436a6320"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("8a073b01-3457-fd52-fb35-97f11048f63f"), new Guid("eca4b1bc-3f11-091d-7220-6a4ac2047557"), null },
                    { new Guid("9990eab8-8afc-f805-6ced-8a544c7226f6"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("cb7df096-8268-3ca4-808d-7ac2e51a96da"), new Guid("92acc37f-4dd4-c818-1aca-1b5993a088cd"), null },
                    { new Guid("9c0857f8-59ae-7c56-f5c7-e783850f8d45"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("cb7df096-8268-3ca4-808d-7ac2e51a96da"), new Guid("eca4b1bc-3f11-091d-7220-6a4ac2047557"), null },
                    { new Guid("9ecb4cb6-052d-1495-250f-8e7c79674adf"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("6d113c17-7ea6-e84c-0dea-420f5c8ae41a"), new Guid("92acc37f-4dd4-c818-1aca-1b5993a088cd"), null },
                    { new Guid("9f3e819c-4014-7247-8154-e9c739738585"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("abb80d87-0568-1d25-a682-1dc3f4b9a923"), new Guid("eca4b1bc-3f11-091d-7220-6a4ac2047557"), null },
                    { new Guid("a0b5227a-f0ca-a23b-7bf7-7961f034d334"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("5a4ac9a6-1c8e-167b-9182-80cbf2db6de8"), new Guid("92acc37f-4dd4-c818-1aca-1b5993a088cd"), null },
                    { new Guid("a1fddfb9-7fc4-7053-3f48-faa54d19c57a"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("089360bf-26c1-cb8d-3730-b563e1d30452"), new Guid("eca4b1bc-3f11-091d-7220-6a4ac2047557"), null },
                    { new Guid("a36cfab6-e59a-1cf2-92ae-4a240eac6ef6"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("c7cdfc04-a77d-679a-06e8-d3e3f518b22c"), new Guid("7a01e8e4-272f-86a5-cbaa-5651bfdf24cc"), null },
                    { new Guid("a97a12d6-f036-0255-5811-7ec1c60b774d"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("e1479074-56fb-83e9-368b-e5a3e9b7a7cd"), new Guid("92acc37f-4dd4-c818-1aca-1b5993a088cd"), null },
                    { new Guid("a98983b2-b208-51de-f793-b7f9fc103b98"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("a3e8139b-2a76-1335-88e4-bd3f2e0a0cc1"), new Guid("7a01e8e4-272f-86a5-cbaa-5651bfdf24cc"), null },
                    { new Guid("ad1e99ab-477e-62c4-8f6a-c81cbb0ebe67"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("a16880ae-dbaa-9c47-15c1-73e006c2db60"), new Guid("eca4b1bc-3f11-091d-7220-6a4ac2047557"), null },
                    { new Guid("b0e3a7b3-a023-9ddf-0049-44dbf5df895b"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("4af1692d-b2ee-21c8-fb83-9fdcd08a7b3c"), new Guid("88f13fff-76a9-49b1-02d4-3a5dca0c4b03"), null },
                    { new Guid("b2d498c9-ed3d-258d-bfd1-f284c939e18d"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("b61e201c-e31e-b5f3-5b6b-734d52dd3236"), new Guid("eca4b1bc-3f11-091d-7220-6a4ac2047557"), null },
                    { new Guid("b87525d6-79c6-b296-8aa3-539e911e50dc"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("68941986-5142-5dc5-97ab-87a4dd78c29c"), new Guid("eca4b1bc-3f11-091d-7220-6a4ac2047557"), null },
                    { new Guid("b97b025c-630f-8cb2-e795-466d2b16fec8"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("a16880ae-dbaa-9c47-15c1-73e006c2db60"), new Guid("92acc37f-4dd4-c818-1aca-1b5993a088cd"), null },
                    { new Guid("b9837fb7-7d51-e2f9-f564-fa7b26244b4c"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("0e16a590-19da-5ccb-5c5e-002d6a1873e6"), new Guid("88f13fff-76a9-49b1-02d4-3a5dca0c4b03"), null },
                    { new Guid("c16b6f4a-318f-cc46-ef6b-2699187c1ccd"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("089360bf-26c1-cb8d-3730-b563e1d30452"), new Guid("60e88433-a40c-0da3-e91a-78e398c6bf1c"), null },
                    { new Guid("c4f40c6e-97be-2eb3-5497-7e6e953a75ba"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("68941986-5142-5dc5-97ab-87a4dd78c29c"), new Guid("a78282c6-0826-0b3b-cb7c-58950284590e"), null },
                    { new Guid("c54b95bc-6e44-981a-cfbf-b7ca8d51a8c6"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("5c278eba-16e6-8478-54e3-4bbe261dc9cd"), new Guid("eca4b1bc-3f11-091d-7220-6a4ac2047557"), null },
                    { new Guid("c732e801-54e0-40a2-180d-b3d9561b5acc"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("abb80d87-0568-1d25-a682-1dc3f4b9a923"), new Guid("92acc37f-4dd4-c818-1aca-1b5993a088cd"), null },
                    { new Guid("c7a2aa0e-b1af-5816-fbba-45fbbb778af9"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("0e16a590-19da-5ccb-5c5e-002d6a1873e6"), new Guid("60e88433-a40c-0da3-e91a-78e398c6bf1c"), null },
                    { new Guid("ca49de3f-4e4a-07ec-6525-77c3ca6cd7fc"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("4af1692d-b2ee-21c8-fb83-9fdcd08a7b3c"), new Guid("7a01e8e4-272f-86a5-cbaa-5651bfdf24cc"), null },
                    { new Guid("cbeb5de0-92d3-dff8-5913-e01e879554a2"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("5c278eba-16e6-8478-54e3-4bbe261dc9cd"), new Guid("92acc37f-4dd4-c818-1aca-1b5993a088cd"), null },
                    { new Guid("cc8f3e08-3491-138f-9fcc-bec2b56d09d0"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("a289ec56-a1c3-28c3-dfcd-38faba04e702"), new Guid("92acc37f-4dd4-c818-1aca-1b5993a088cd"), null },
                    { new Guid("ce904764-a638-e3ec-fd1a-ce5e6668e162"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("e649b00f-10f2-63a7-9d83-6d548d650fbb"), new Guid("92acc37f-4dd4-c818-1aca-1b5993a088cd"), null },
                    { new Guid("d1104762-8cea-c832-c035-88c83cd7c36c"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("8a073b01-3457-fd52-fb35-97f11048f63f"), new Guid("60e88433-a40c-0da3-e91a-78e398c6bf1c"), null },
                    { new Guid("d1e59df1-0a45-c953-e87c-6f98664f4fe8"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("0e16a590-19da-5ccb-5c5e-002d6a1873e6"), new Guid("92acc37f-4dd4-c818-1aca-1b5993a088cd"), null },
                    { new Guid("d2cae4f1-e660-8d2b-9d4e-4658ec71637c"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("68941986-5142-5dc5-97ab-87a4dd78c29c"), new Guid("7a01e8e4-272f-86a5-cbaa-5651bfdf24cc"), null },
                    { new Guid("d39f35ea-c835-9159-3f38-b9dc32df1e91"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("b61e201c-e31e-b5f3-5b6b-734d52dd3236"), new Guid("92acc37f-4dd4-c818-1aca-1b5993a088cd"), null },
                    { new Guid("d3cd4596-614e-4b83-24d7-c0cb2a43906c"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("a289ec56-a1c3-28c3-dfcd-38faba04e702"), new Guid("99e68c35-b651-8b6e-055c-a33759442737"), null },
                    { new Guid("dc21788c-7407-3bd9-821d-6e6fa5d52887"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("58f13f47-3286-967c-b3dd-f007105e62ef"), new Guid("92acc37f-4dd4-c818-1aca-1b5993a088cd"), null },
                    { new Guid("e46064b1-e6b2-31bb-b9e1-20ed7d38c7d0"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("5a4ac9a6-1c8e-167b-9182-80cbf2db6de8"), new Guid("7a01e8e4-272f-86a5-cbaa-5651bfdf24cc"), null },
                    { new Guid("e6e122f2-6b1b-0def-e6af-bac6c0f553c0"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("f8947b81-ab97-7b45-36d7-367384e66e89"), new Guid("7a01e8e4-272f-86a5-cbaa-5651bfdf24cc"), null },
                    { new Guid("e89de91e-75b7-212f-f438-390db16f4b8b"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("a3e8139b-2a76-1335-88e4-bd3f2e0a0cc1"), new Guid("92acc37f-4dd4-c818-1aca-1b5993a088cd"), null },
                    { new Guid("e9287493-dda4-a685-5b6b-f2e3c7d061a8"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("8a073b01-3457-fd52-fb35-97f11048f63f"), new Guid("a78282c6-0826-0b3b-cb7c-58950284590e"), null },
                    { new Guid("edf2aebe-c859-206b-11d8-afb107763c80"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("8a073b01-3457-fd52-fb35-97f11048f63f"), new Guid("92acc37f-4dd4-c818-1aca-1b5993a088cd"), null },
                    { new Guid("f40127fb-71e1-ce69-b318-018de9ca7684"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("0cb12fbb-961c-66af-bad6-52e04cc75baf"), new Guid("eca4b1bc-3f11-091d-7220-6a4ac2047557"), null },
                    { new Guid("f6e6da21-b4f6-8787-45c4-66c741a1f1c6"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("abb80d87-0568-1d25-a682-1dc3f4b9a923"), new Guid("a78282c6-0826-0b3b-cb7c-58950284590e"), null },
                    { new Guid("f799892e-dfa2-6bde-f3bc-799bf1c8f90c"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("abb80d87-0568-1d25-a682-1dc3f4b9a923"), new Guid("7a01e8e4-272f-86a5-cbaa-5651bfdf24cc"), null },
                    { new Guid("fa394ae1-bc1a-41e9-d42b-b7ab79b3e506"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("0e16a590-19da-5ccb-5c5e-002d6a1873e6"), new Guid("7a01e8e4-272f-86a5-cbaa-5651bfdf24cc"), null },
                    { new Guid("fef2729b-5cd7-9204-9d5b-5dea464d2704"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("e1479074-56fb-83e9-368b-e5a3e9b7a7cd"), new Guid("7a01e8e4-272f-86a5-cbaa-5651bfdf24cc"), null },
                    { new Guid("ffbfa3c0-b9a9-6b58-ac8f-52e0682de595"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("f8947b81-ab97-7b45-36d7-367384e66e89"), new Guid("eca4b1bc-3f11-091d-7220-6a4ac2047557"), null }
                });

            migrationBuilder.CreateIndex(
                name: "ix_counterparties_organization_id_name",
                table: "counterparties",
                columns: new[] { "organization_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_identity_role_claims_role_id",
                table: "identity_role_claims",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "identity_roles",
                column: "normalized_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_identity_user_claims_user_id",
                table: "identity_user_claims",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_identity_user_logins_user_id",
                table: "identity_user_logins",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_identity_user_roles_role_id",
                table: "identity_user_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_mandates_axis_id",
                table: "mandates",
                column: "axis_id");

            migrationBuilder.CreateIndex(
                name: "ix_mandates_organization_id_policy_id",
                table: "mandates",
                columns: new[] { "organization_id", "policy_id" });

            migrationBuilder.CreateIndex(
                name: "ix_mandates_organization_id_status",
                table: "mandates",
                columns: new[] { "organization_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_mandates_policy_id",
                table: "mandates",
                column: "policy_id");

            migrationBuilder.CreateIndex(
                name: "ix_orders_counterparty_id",
                table: "orders",
                column: "counterparty_id");

            migrationBuilder.CreateIndex(
                name: "ix_orders_mandate_id",
                table: "orders",
                column: "mandate_id");

            migrationBuilder.CreateIndex(
                name: "ix_orders_organization_id_confirmation",
                table: "orders",
                columns: new[] { "organization_id", "confirmation" });

            migrationBuilder.CreateIndex(
                name: "ix_orders_organization_id_mandate_id_approval",
                table: "orders",
                columns: new[] { "organization_id", "mandate_id", "approval" });

            migrationBuilder.CreateIndex(
                name: "ix_organization_commodities_organization_id_commodity",
                table: "organization_commodities",
                columns: new[] { "organization_id", "commodity" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_organization_group_members_group_id_member_id",
                table: "organization_group_members",
                columns: new[] { "group_id", "member_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_organization_group_members_member_id",
                table: "organization_group_members",
                column: "member_id");

            migrationBuilder.CreateIndex(
                name: "ix_organization_groups_organization_id_name",
                table: "organization_groups",
                columns: new[] { "organization_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_organization_members_organization_id_user_id",
                table: "organization_members",
                columns: new[] { "organization_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_organization_members_user_id",
                table: "organization_members",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_organization_rules_group_id",
                table: "organization_rules",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "ix_organization_rules_member_id",
                table: "organization_rules",
                column: "member_id");

            migrationBuilder.CreateIndex(
                name: "ix_organization_rules_organization_id_rule_id_group_id",
                table: "organization_rules",
                columns: new[] { "organization_id", "rule_id", "group_id" },
                unique: true,
                filter: "group_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_organization_rules_organization_id_rule_id_member_id",
                table: "organization_rules",
                columns: new[] { "organization_id", "rule_id", "member_id" },
                unique: true,
                filter: "member_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_organization_rules_rule_id",
                table: "organization_rules",
                column: "rule_id");

            migrationBuilder.CreateIndex(
                name: "ix_organizations_slug",
                table: "organizations",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_policies_organization_id_code",
                table: "policies",
                columns: new[] { "organization_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_policies_organization_id_status",
                table: "policies",
                columns: new[] { "organization_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_policy_axes_policy_id_code",
                table: "policy_axes",
                columns: new[] { "policy_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_policy_coverage_bands_policy_id_crop",
                table: "policy_coverage_bands",
                columns: new[] { "policy_id", "crop" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_policy_instruments_policy_id",
                table: "policy_instruments",
                column: "policy_id");

            migrationBuilder.CreateIndex(
                name: "ix_policy_versions_policy_id_date",
                table: "policy_versions",
                columns: new[] { "policy_id", "date" });

            migrationBuilder.CreateIndex(
                name: "ix_roles_code",
                table: "roles",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_rule_roles_role_id",
                table: "rule_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_rule_roles_rule_id_role_id",
                table: "rule_roles",
                columns: new[] { "rule_id", "role_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_rules_code",
                table: "rules",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_timelines_entity_type_entity_id",
                table: "timelines",
                columns: new[] { "entity_type", "entity_id" });

            migrationBuilder.CreateIndex(
                name: "ix_timelines_organization_id_occurred_at",
                table: "timelines",
                columns: new[] { "organization_id", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "users",
                column: "normalized_email");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "users",
                column: "normalized_user_name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "identity_role_claims");

            migrationBuilder.DropTable(
                name: "identity_user_claims");

            migrationBuilder.DropTable(
                name: "identity_user_logins");

            migrationBuilder.DropTable(
                name: "identity_user_roles");

            migrationBuilder.DropTable(
                name: "identity_user_tokens");

            migrationBuilder.DropTable(
                name: "orders");

            migrationBuilder.DropTable(
                name: "organization_commodities");

            migrationBuilder.DropTable(
                name: "organization_group_members");

            migrationBuilder.DropTable(
                name: "organization_rules");

            migrationBuilder.DropTable(
                name: "policy_coverage_bands");

            migrationBuilder.DropTable(
                name: "policy_instruments");

            migrationBuilder.DropTable(
                name: "policy_versions");

            migrationBuilder.DropTable(
                name: "rule_roles");

            migrationBuilder.DropTable(
                name: "timelines");

            migrationBuilder.DropTable(
                name: "identity_roles");

            migrationBuilder.DropTable(
                name: "counterparties");

            migrationBuilder.DropTable(
                name: "mandates");

            migrationBuilder.DropTable(
                name: "organization_groups");

            migrationBuilder.DropTable(
                name: "organization_members");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "rules");

            migrationBuilder.DropTable(
                name: "policy_axes");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "policies");

            migrationBuilder.DropTable(
                name: "organizations");
        }
    }
}
