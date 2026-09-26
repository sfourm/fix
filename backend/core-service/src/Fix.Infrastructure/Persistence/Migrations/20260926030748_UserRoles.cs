using System;
using Fix.Domain.AggregateRoots.Roles;
using Fix.Domain.AggregateRoots.Rules;
using Fix.Domain.Common;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fix.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UserRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Cargos das organizações (não os de sistema, que o seed abaixo trata): guarda quem tinha view_users e
            // edit_organization antes de a regra view_users sair (a exclusão remove os vínculos em cascata).
            var systemRules = string.Join(", ", SystemRules.All.Select(r => $"'{SystemRules.Id(r.Code)}'"));
            migrationBuilder.Sql($"""
                CREATE TEMP TABLE _had_view_users AS
                    SELECT rule_id FROM rule_roles WHERE role_id = '{DeterministicGuid.From("role:view_users")}' AND rule_id NOT IN ({systemRules});
                CREATE TEMP TABLE _had_edit_organization AS
                    SELECT rule_id FROM rule_roles WHERE role_id = '{SystemRoles.Id(RoleCodes.EditOrganization)}' AND rule_id NOT IN ({systemRules});
                """);

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("4648f9ec-3017-6dc1-4823-16543e317148"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("7861f371-802a-085a-a4a2-90733c4c00b8"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("f40127fb-71e1-ce69-b318-018de9ca7684"));

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "id",
                keyValue: new Guid("0cb12fbb-961c-66af-bad6-52e04cc75baf"));

            migrationBuilder.UpdateData(
                table: "roles",
                keyColumn: "id",
                keyValue: new Guid("5a4ac9a6-1c8e-167b-9182-80cbf2db6de8"),
                column: "description",
                value: "Editar o setup da companhia");

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "id", "author_created", "author_updated", "code", "created_at", "description", "updated_at" },
                values: new object[,]
                {
                    { new Guid("1b307cd1-9a78-7280-78ed-917e0aeeb11c"), null, null, "view_user", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Visualizar membros, grupos e organograma", null },
                    { new Guid("2d9f0008-f3bb-0824-92dc-f59d8b091b48"), null, null, "delete_user", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Remover membros e excluir grupos", null },
                    { new Guid("74db766c-34b4-eb64-b49b-1f745a25cdfb"), null, null, "create_user", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Adicionar membros e criar grupos", null },
                    { new Guid("b90f6c34-2b62-3e52-8f6c-a6b2d5ccae8a"), null, null, "update_user", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Editar membros, grupos, organograma e cargos", null }
                });

            migrationBuilder.InsertData(
                table: "rule_roles",
                columns: new[] { "id", "author_created", "author_updated", "created_at", "role_id", "rule_id", "updated_at" },
                values: new object[,]
                {
                    { new Guid("1540aded-03f9-52a0-8851-25683871aa49"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("2d9f0008-f3bb-0824-92dc-f59d8b091b48"), new Guid("eca4b1bc-3f11-091d-7220-6a4ac2047557"), null },
                    { new Guid("1aabdd68-37f6-ee27-23e6-ce97537f5a41"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("b90f6c34-2b62-3e52-8f6c-a6b2d5ccae8a"), new Guid("cef275a0-cb75-62d9-0526-6f384c73ed83"), null },
                    { new Guid("7907f575-caf5-233b-de6f-be81e086cc70"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("2d9f0008-f3bb-0824-92dc-f59d8b091b48"), new Guid("cef275a0-cb75-62d9-0526-6f384c73ed83"), null },
                    { new Guid("7c7da31c-2313-3821-803e-f233df53abbb"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("1b307cd1-9a78-7280-78ed-917e0aeeb11c"), new Guid("cef275a0-cb75-62d9-0526-6f384c73ed83"), null },
                    { new Guid("7e227d48-0964-adb2-e7fc-42c66b791756"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("1b307cd1-9a78-7280-78ed-917e0aeeb11c"), new Guid("92acc37f-4dd4-c818-1aca-1b5993a088cd"), null },
                    { new Guid("9d78e5c6-5d6d-d531-0332-c618bdab6244"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("74db766c-34b4-eb64-b49b-1f745a25cdfb"), new Guid("cef275a0-cb75-62d9-0526-6f384c73ed83"), null },
                    { new Guid("b7d160a4-b340-bfcf-a23b-2dd52128332e"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("1b307cd1-9a78-7280-78ed-917e0aeeb11c"), new Guid("eca4b1bc-3f11-091d-7220-6a4ac2047557"), null },
                    { new Guid("c14ab1fc-ec54-058c-b332-7871d2f9c193"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("74db766c-34b4-eb64-b49b-1f745a25cdfb"), new Guid("eca4b1bc-3f11-091d-7220-6a4ac2047557"), null },
                    { new Guid("fc845878-436a-e825-1536-5d8a12834956"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("b90f6c34-2b62-3e52-8f6c-a6b2d5ccae8a"), new Guid("eca4b1bc-3f11-091d-7220-6a4ac2047557"), null }
                });

            // Quem via membros continua vendo (view_user); quem editava a organização continua gerindo usuários.
            string Grant(string temp, string role) => $"""
                INSERT INTO rule_roles (id, rule_id, role_id, created_at)
                SELECT gen_random_uuid(), rule_id, '{SystemRoles.Id(role)}', now() FROM {temp}
                ON CONFLICT (rule_id, role_id) DO NOTHING;
                """;
            migrationBuilder.Sql(Grant("_had_view_users", RoleCodes.ViewUser));
            foreach (var role in new[] { RoleCodes.ViewUser, RoleCodes.CreateUser, RoleCodes.UpdateUser, RoleCodes.DeleteUser })
            {
                migrationBuilder.Sql(Grant("_had_edit_organization", role));
            }

            migrationBuilder.Sql("DROP TABLE _had_view_users; DROP TABLE _had_edit_organization;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("1540aded-03f9-52a0-8851-25683871aa49"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("1aabdd68-37f6-ee27-23e6-ce97537f5a41"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("7907f575-caf5-233b-de6f-be81e086cc70"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("7c7da31c-2313-3821-803e-f233df53abbb"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("7e227d48-0964-adb2-e7fc-42c66b791756"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("9d78e5c6-5d6d-d531-0332-c618bdab6244"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("b7d160a4-b340-bfcf-a23b-2dd52128332e"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("c14ab1fc-ec54-058c-b332-7871d2f9c193"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("fc845878-436a-e825-1536-5d8a12834956"));

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "id",
                keyValue: new Guid("1b307cd1-9a78-7280-78ed-917e0aeeb11c"));

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "id",
                keyValue: new Guid("2d9f0008-f3bb-0824-92dc-f59d8b091b48"));

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "id",
                keyValue: new Guid("74db766c-34b4-eb64-b49b-1f745a25cdfb"));

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "id",
                keyValue: new Guid("b90f6c34-2b62-3e52-8f6c-a6b2d5ccae8a"));

            migrationBuilder.UpdateData(
                table: "roles",
                keyColumn: "id",
                keyValue: new Guid("5a4ac9a6-1c8e-167b-9182-80cbf2db6de8"),
                column: "description",
                value: "Editar o setup da companhia, membros e grupos");

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "id", "author_created", "author_updated", "code", "created_at", "description", "updated_at" },
                values: new object[] { new Guid("0cb12fbb-961c-66af-bad6-52e04cc75baf"), null, null, "view_users", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Visualizar membros e grupos", null });

            migrationBuilder.InsertData(
                table: "rule_roles",
                columns: new[] { "id", "author_created", "author_updated", "created_at", "role_id", "rule_id", "updated_at" },
                values: new object[,]
                {
                    { new Guid("4648f9ec-3017-6dc1-4823-16543e317148"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("0cb12fbb-961c-66af-bad6-52e04cc75baf"), new Guid("cef275a0-cb75-62d9-0526-6f384c73ed83"), null },
                    { new Guid("7861f371-802a-085a-a4a2-90733c4c00b8"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("0cb12fbb-961c-66af-bad6-52e04cc75baf"), new Guid("92acc37f-4dd4-c818-1aca-1b5993a088cd"), null },
                    { new Guid("f40127fb-71e1-ce69-b318-018de9ca7684"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("0cb12fbb-961c-66af-bad6-52e04cc75baf"), new Guid("eca4b1bc-3f11-091d-7220-6a4ac2047557"), null }
                });
        }
    }
}
