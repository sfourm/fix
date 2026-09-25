using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fix.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InternalStaffAndCustomRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_rules_code",
                table: "rules");

            migrationBuilder.AddColumn<Guid>(
                name: "organization_id",
                table: "rules",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_internal",
                table: "organizations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.InsertData(
                table: "rules",
                columns: new[] { "id", "author_created", "author_updated", "code", "created_at", "name", "organization_id", "updated_at" },
                values: new object[] { new Guid("cef275a0-cb75-62d9-0526-6f384c73ed83"), null, null, "owner", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Owner", null, null });

            migrationBuilder.InsertData(
                table: "rule_roles",
                columns: new[] { "id", "author_created", "author_updated", "created_at", "role_id", "rule_id", "updated_at" },
                values: new object[,]
                {
                    { new Guid("2b7353df-7bca-dae2-54a1-2b182086b6f9"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("c7cdfc04-a77d-679a-06e8-d3e3f518b22c"), new Guid("cef275a0-cb75-62d9-0526-6f384c73ed83"), null },
                    { new Guid("30e6f2e5-7565-7b49-9330-69227ff9c402"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("b7fdd907-0764-68a3-24d6-8a6015fcd025"), new Guid("cef275a0-cb75-62d9-0526-6f384c73ed83"), null },
                    { new Guid("3544e216-8f60-af59-9c50-d14911b0d835"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("5a4ac9a6-1c8e-167b-9182-80cbf2db6de8"), new Guid("cef275a0-cb75-62d9-0526-6f384c73ed83"), null },
                    { new Guid("379495e2-72d7-3d7f-00f6-344a1559bbdc"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("8a073b01-3457-fd52-fb35-97f11048f63f"), new Guid("cef275a0-cb75-62d9-0526-6f384c73ed83"), null },
                    { new Guid("3b8ce2ce-8330-f7d1-f846-68e148e3b8a8"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("a16880ae-dbaa-9c47-15c1-73e006c2db60"), new Guid("cef275a0-cb75-62d9-0526-6f384c73ed83"), null },
                    { new Guid("4548f229-7c88-298a-0199-11e5fa8c01af"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("a3e8139b-2a76-1335-88e4-bd3f2e0a0cc1"), new Guid("cef275a0-cb75-62d9-0526-6f384c73ed83"), null },
                    { new Guid("4648f9ec-3017-6dc1-4823-16543e317148"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("0cb12fbb-961c-66af-bad6-52e04cc75baf"), new Guid("cef275a0-cb75-62d9-0526-6f384c73ed83"), null },
                    { new Guid("551fd7a3-7b47-76d6-f521-f13642d84ebe"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("089360bf-26c1-cb8d-3730-b563e1d30452"), new Guid("cef275a0-cb75-62d9-0526-6f384c73ed83"), null },
                    { new Guid("5af069af-cee7-875a-01a9-60fe8282650e"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("b61e201c-e31e-b5f3-5b6b-734d52dd3236"), new Guid("cef275a0-cb75-62d9-0526-6f384c73ed83"), null },
                    { new Guid("63392b6f-2546-70ab-3ee9-72e67a908077"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("6d113c17-7ea6-e84c-0dea-420f5c8ae41a"), new Guid("cef275a0-cb75-62d9-0526-6f384c73ed83"), null },
                    { new Guid("9f6e9e63-1bf3-d0e5-1c4f-4544e1fb18f9"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("0e16a590-19da-5ccb-5c5e-002d6a1873e6"), new Guid("cef275a0-cb75-62d9-0526-6f384c73ed83"), null },
                    { new Guid("a7c2e95a-112a-1fb0-17f5-37dd78104d98"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("cb7df096-8268-3ca4-808d-7ac2e51a96da"), new Guid("cef275a0-cb75-62d9-0526-6f384c73ed83"), null },
                    { new Guid("b164bce5-8bfe-486f-288f-afdbfc71132f"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("abb80d87-0568-1d25-a682-1dc3f4b9a923"), new Guid("cef275a0-cb75-62d9-0526-6f384c73ed83"), null },
                    { new Guid("b55d29cd-cfc5-dc5f-0c79-d74dc5912f7b"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("f8947b81-ab97-7b45-36d7-367384e66e89"), new Guid("cef275a0-cb75-62d9-0526-6f384c73ed83"), null },
                    { new Guid("b97b363d-8e82-6179-7ce1-fde12cb65cb6"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("4af1692d-b2ee-21c8-fb83-9fdcd08a7b3c"), new Guid("cef275a0-cb75-62d9-0526-6f384c73ed83"), null },
                    { new Guid("bde5a46c-0f1f-3c44-4ea1-23128563169e"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("a289ec56-a1c3-28c3-dfcd-38faba04e702"), new Guid("cef275a0-cb75-62d9-0526-6f384c73ed83"), null },
                    { new Guid("bedaca31-0504-d1ef-8e15-6542960fe8f5"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("e649b00f-10f2-63a7-9d83-6d548d650fbb"), new Guid("cef275a0-cb75-62d9-0526-6f384c73ed83"), null },
                    { new Guid("c80432b8-ba5f-8762-d48e-0252bdcaf827"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("58f13f47-3286-967c-b3dd-f007105e62ef"), new Guid("cef275a0-cb75-62d9-0526-6f384c73ed83"), null },
                    { new Guid("ca31704e-0319-a383-8108-d4a9e266f4e6"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("5c278eba-16e6-8478-54e3-4bbe261dc9cd"), new Guid("cef275a0-cb75-62d9-0526-6f384c73ed83"), null },
                    { new Guid("cec54b4e-5deb-1152-d45d-eb0d078b9e4a"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("68941986-5142-5dc5-97ab-87a4dd78c29c"), new Guid("cef275a0-cb75-62d9-0526-6f384c73ed83"), null },
                    { new Guid("d10bf197-c453-2564-3702-55bad589f8e8"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("84da2989-9a40-e6a5-968e-57cb026ddf54"), new Guid("cef275a0-cb75-62d9-0526-6f384c73ed83"), null },
                    { new Guid("e178b914-2946-11b3-5ae3-476324f533ef"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("e1479074-56fb-83e9-368b-e5a3e9b7a7cd"), new Guid("cef275a0-cb75-62d9-0526-6f384c73ed83"), null }
                });

            // ---------- Conversão dos dados (antes de apagar as rules antigas) ----------
            migrationBuilder.Sql("""
                -- 1) Alçadas-modelo viram alçadas de cada organização; as atribuições passam para a cópia da organização.
                INSERT INTO rules (id, organization_id, code, name, created_at)
                SELECT gen_random_uuid(), o.id, t.code, t.name, now()
                FROM organizations o
                CROSS JOIN rules t
                WHERE t.organization_id IS NULL AND t.code IN ('gestor', 'operador', 'middle_office');

                INSERT INTO rule_roles (id, rule_id, role_id, created_at)
                SELECT gen_random_uuid(), c.id, rr.role_id, now()
                FROM rules c
                JOIN rules t ON t.code = c.code AND t.organization_id IS NULL
                JOIN rule_roles rr ON rr.rule_id = t.id
                WHERE c.organization_id IS NOT NULL AND c.code IN ('gestor', 'operador', 'middle_office');

                UPDATE organization_rules a
                SET rule_id = c.id
                FROM rules t, rules c
                WHERE a.rule_id = t.id
                  AND t.organization_id IS NULL AND t.code IN ('gestor', 'operador', 'middle_office')
                  AND c.organization_id = a.organization_id AND c.code = t.code;

                -- 2) founder passa a ser owner.
                UPDATE organization_rules
                SET rule_id = (SELECT id FROM rules WHERE code = 'owner' AND organization_id IS NULL)
                WHERE rule_id = (SELECT id FROM rules WHERE code = 'founder' AND organization_id IS NULL);

                -- 3) Papéis internos da FIX nunca ficam em organizações clientes.
                DELETE FROM organization_rules
                WHERE rule_id IN (SELECT id FROM rules WHERE organization_id IS NULL AND code IN ('administrador', 'super_administrador'));

                -- 4) Todo membro tem uma base: quem não é owner passa a ser user.
                INSERT INTO organization_rules (id, organization_id, rule_id, member_id, created_at)
                SELECT gen_random_uuid(), m.organization_id, (SELECT id FROM rules WHERE code = 'user' AND organization_id IS NULL), m.id, now()
                FROM organization_members m
                WHERE NOT EXISTS (
                    SELECT 1 FROM organization_rules a
                    JOIN rules r ON r.id = a.rule_id
                    WHERE a.member_id = m.id AND r.organization_id IS NULL AND r.code IN ('owner', 'user'));

                -- 5) O grupo raiz deixa de se chamar "Administradores" (administrador agora é papel interno da FIX).
                UPDATE organization_groups g
                SET name = 'Direção'
                WHERE g.is_default AND g.name = 'Administradores'
                  AND NOT EXISTS (SELECT 1 FROM organization_groups x WHERE x.organization_id = g.organization_id AND x.name = 'Direção');
                """);

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("060ef5b5-6396-b8fd-abda-f9a8b3145245"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("062bbe76-3dea-c178-6555-bb6ddd81b849"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("08d60ff4-ff17-a886-11a0-6cebd1ae178c"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("09b7f998-4ce9-6e2f-e829-e03d7d3bcd25"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("09f1f5bc-c2bf-38c6-6f59-3161ba6dbd72"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("0f28d31d-e28d-6896-1a9d-30e89d592161"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("109d46c5-b42b-12e4-6b19-0062e9f3cdf2"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("1ef99c34-8082-503e-e1d7-f82c4b6d59fb"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("1f93cd4e-633c-8ff2-fe13-4b61e2a54d9b"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("20dcf68f-2aa5-c2dc-6897-8a1b78c70ecc"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("228cbd67-5e7d-dd46-114c-d9738761600e"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("229d297f-ff4a-8a6e-3c91-ded11d09f1f3"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("261fcc97-edef-6ba8-0fcc-35c13890c158"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("276a1da8-49c0-63b8-a56e-e8e76772427f"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("3668a920-7521-cc6e-5a4e-b2f3e214a5c7"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("37f3d158-a70c-d304-afe7-86539d887115"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("3a3f0e26-66cc-5446-8996-efb8aa9a78c1"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("3a879884-ab30-4dff-16d4-b220df1fb5d6"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("3b79f533-fc35-3410-0f48-8abfe6150fe8"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("40f9f578-e85b-17fd-0888-90a9bfc67079"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("437fe351-1d3a-b1e0-517d-343bc75450f5"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("4e446593-929f-0d72-67b3-0454d687b20d"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("5037a7ed-ba59-3ec1-c133-257e255b0ac2"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("507b763f-8991-f79c-300e-594967dd78e9"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("5af4df66-28a0-d963-b824-d8e92c82adb0"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("5c7b65ea-7b20-4317-c8f4-0ecd9f05abdf"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("5c968571-c02f-b91b-25f9-9f2b5c278c79"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("5cc03b65-1a70-30ff-9b05-5ec156882856"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("5f2610b9-e549-0b4b-d531-637078987aab"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("68c04523-c8b0-6dd1-a432-aa5ba291e886"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("79f4263c-8243-1f82-f228-16d7b56277e0"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("83308d38-7c72-4897-34be-8b7efe531c16"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("836eef9f-46f8-eba5-6107-a91597f50e64"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("86a11b0b-1c84-5c18-8b27-def8f4951a83"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("87803ea1-1076-cb86-50e2-5dbec238f432"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("8818697b-9e14-1f98-d336-783ad486c88f"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("8956be42-6573-d57d-bb49-30c5fbe7c9c5"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("89915608-af84-13fb-3fed-4033ee492145"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("8a1d977f-acf3-db98-c396-ad29d3b0c20e"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("8e8af506-92fd-cd41-53d7-4a0f8830d5e0"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("920e913b-4226-d286-931e-21a5e79c014b"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("9990eab8-8afc-f805-6ced-8a544c7226f6"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("9ecb4cb6-052d-1495-250f-8e7c79674adf"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("a0b5227a-f0ca-a23b-7bf7-7961f034d334"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("a36cfab6-e59a-1cf2-92ae-4a240eac6ef6"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("a97a12d6-f036-0255-5811-7ec1c60b774d"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("a98983b2-b208-51de-f793-b7f9fc103b98"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("b0e3a7b3-a023-9ddf-0049-44dbf5df895b"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("b97b025c-630f-8cb2-e795-466d2b16fec8"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("b9837fb7-7d51-e2f9-f564-fa7b26244b4c"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("c4f40c6e-97be-2eb3-5497-7e6e953a75ba"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("c732e801-54e0-40a2-180d-b3d9561b5acc"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("ca49de3f-4e4a-07ec-6525-77c3ca6cd7fc"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("cbeb5de0-92d3-dff8-5913-e01e879554a2"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("cc8f3e08-3491-138f-9fcc-bec2b56d09d0"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("ce904764-a638-e3ec-fd1a-ce5e6668e162"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("d1e59df1-0a45-c953-e87c-6f98664f4fe8"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("d2cae4f1-e660-8d2b-9d4e-4658ec71637c"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("d39f35ea-c835-9159-3f38-b9dc32df1e91"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("d3cd4596-614e-4b83-24d7-c0cb2a43906c"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("dc21788c-7407-3bd9-821d-6e6fa5d52887"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("e46064b1-e6b2-31bb-b9e1-20ed7d38c7d0"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("e6e122f2-6b1b-0def-e6af-bac6c0f553c0"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("e89de91e-75b7-212f-f438-390db16f4b8b"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("e9287493-dda4-a685-5b6b-f2e3c7d061a8"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("edf2aebe-c859-206b-11d8-afb107763c80"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("f6e6da21-b4f6-8787-45c4-66c741a1f1c6"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("f799892e-dfa2-6bde-f3bc-799bf1c8f90c"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("fa394ae1-bc1a-41e9-d42b-b7ab79b3e506"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("fef2729b-5cd7-9204-9d5b-5dea464d2704"));

            migrationBuilder.DeleteData(
                table: "rules",
                keyColumn: "id",
                keyValue: new Guid("7a01e8e4-272f-86a5-cbaa-5651bfdf24cc"));

            migrationBuilder.DeleteData(
                table: "rules",
                keyColumn: "id",
                keyValue: new Guid("88f13fff-76a9-49b1-02d4-3a5dca0c4b03"));

            migrationBuilder.DeleteData(
                table: "rules",
                keyColumn: "id",
                keyValue: new Guid("99e68c35-b651-8b6e-055c-a33759442737"));

            migrationBuilder.DeleteData(
                table: "rules",
                keyColumn: "id",
                keyValue: new Guid("a78282c6-0826-0b3b-cb7c-58950284590e"));

            migrationBuilder.UpdateData(
                table: "rules",
                keyColumn: "id",
                keyValue: new Guid("60e88433-a40c-0da3-e91a-78e398c6bf1c"),
                columns: new[] { "name", "organization_id" },
                values: new object[] { "Usuário", null });

            migrationBuilder.UpdateData(
                table: "rules",
                keyColumn: "id",
                keyValue: new Guid("92acc37f-4dd4-c818-1aca-1b5993a088cd"),
                columns: new[] { "name", "organization_id" },
                values: new object[] { "Administrador (FIX)", null });

            migrationBuilder.UpdateData(
                table: "rules",
                keyColumn: "id",
                keyValue: new Guid("eca4b1bc-3f11-091d-7220-6a4ac2047557"),
                columns: new[] { "name", "organization_id" },
                values: new object[] { "Super administrador (FIX)", null });

            migrationBuilder.CreateIndex(
                name: "ix_rules_organization_id_code",
                table: "rules",
                columns: new[] { "organization_id", "code" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.AddForeignKey(
                name: "fk_rules_organizations_organization_id",
                table: "rules",
                column: "organization_id",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_rules_organizations_organization_id",
                table: "rules");

            migrationBuilder.DropIndex(
                name: "ix_rules_organization_id_code",
                table: "rules");

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("2b7353df-7bca-dae2-54a1-2b182086b6f9"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("30e6f2e5-7565-7b49-9330-69227ff9c402"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("3544e216-8f60-af59-9c50-d14911b0d835"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("379495e2-72d7-3d7f-00f6-344a1559bbdc"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("3b8ce2ce-8330-f7d1-f846-68e148e3b8a8"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("4548f229-7c88-298a-0199-11e5fa8c01af"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("4648f9ec-3017-6dc1-4823-16543e317148"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("551fd7a3-7b47-76d6-f521-f13642d84ebe"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("5af069af-cee7-875a-01a9-60fe8282650e"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("63392b6f-2546-70ab-3ee9-72e67a908077"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("9f6e9e63-1bf3-d0e5-1c4f-4544e1fb18f9"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("a7c2e95a-112a-1fb0-17f5-37dd78104d98"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("b164bce5-8bfe-486f-288f-afdbfc71132f"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("b55d29cd-cfc5-dc5f-0c79-d74dc5912f7b"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("b97b363d-8e82-6179-7ce1-fde12cb65cb6"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("bde5a46c-0f1f-3c44-4ea1-23128563169e"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("bedaca31-0504-d1ef-8e15-6542960fe8f5"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("c80432b8-ba5f-8762-d48e-0252bdcaf827"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("ca31704e-0319-a383-8108-d4a9e266f4e6"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("cec54b4e-5deb-1152-d45d-eb0d078b9e4a"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("d10bf197-c453-2564-3702-55bad589f8e8"));

            migrationBuilder.DeleteData(
                table: "rule_roles",
                keyColumn: "id",
                keyValue: new Guid("e178b914-2946-11b3-5ae3-476324f533ef"));

            migrationBuilder.DeleteData(
                table: "rules",
                keyColumn: "id",
                keyValue: new Guid("cef275a0-cb75-62d9-0526-6f384c73ed83"));

            migrationBuilder.DropColumn(
                name: "organization_id",
                table: "rules");

            migrationBuilder.DropColumn(
                name: "is_internal",
                table: "organizations");

            migrationBuilder.InsertData(
                table: "rule_roles",
                columns: new[] { "id", "author_created", "author_updated", "created_at", "role_id", "rule_id", "updated_at" },
                values: new object[,]
                {
                    { new Guid("09f1f5bc-c2bf-38c6-6f59-3161ba6dbd72"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("089360bf-26c1-cb8d-3730-b563e1d30452"), new Guid("92acc37f-4dd4-c818-1aca-1b5993a088cd"), null },
                    { new Guid("0f28d31d-e28d-6896-1a9d-30e89d592161"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("b7fdd907-0764-68a3-24d6-8a6015fcd025"), new Guid("92acc37f-4dd4-c818-1aca-1b5993a088cd"), null },
                    { new Guid("5037a7ed-ba59-3ec1-c133-257e255b0ac2"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("4af1692d-b2ee-21c8-fb83-9fdcd08a7b3c"), new Guid("92acc37f-4dd4-c818-1aca-1b5993a088cd"), null },
                    { new Guid("507b763f-8991-f79c-300e-594967dd78e9"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("f8947b81-ab97-7b45-36d7-367384e66e89"), new Guid("92acc37f-4dd4-c818-1aca-1b5993a088cd"), null },
                    { new Guid("5cc03b65-1a70-30ff-9b05-5ec156882856"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("c7cdfc04-a77d-679a-06e8-d3e3f518b22c"), new Guid("92acc37f-4dd4-c818-1aca-1b5993a088cd"), null },
                    { new Guid("79f4263c-8243-1f82-f228-16d7b56277e0"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("68941986-5142-5dc5-97ab-87a4dd78c29c"), new Guid("92acc37f-4dd4-c818-1aca-1b5993a088cd"), null },
                    { new Guid("8956be42-6573-d57d-bb49-30c5fbe7c9c5"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("84da2989-9a40-e6a5-968e-57cb026ddf54"), new Guid("92acc37f-4dd4-c818-1aca-1b5993a088cd"), null },
                    { new Guid("9990eab8-8afc-f805-6ced-8a544c7226f6"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("cb7df096-8268-3ca4-808d-7ac2e51a96da"), new Guid("92acc37f-4dd4-c818-1aca-1b5993a088cd"), null },
                    { new Guid("9ecb4cb6-052d-1495-250f-8e7c79674adf"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("6d113c17-7ea6-e84c-0dea-420f5c8ae41a"), new Guid("92acc37f-4dd4-c818-1aca-1b5993a088cd"), null },
                    { new Guid("a0b5227a-f0ca-a23b-7bf7-7961f034d334"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("5a4ac9a6-1c8e-167b-9182-80cbf2db6de8"), new Guid("92acc37f-4dd4-c818-1aca-1b5993a088cd"), null },
                    { new Guid("a97a12d6-f036-0255-5811-7ec1c60b774d"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("e1479074-56fb-83e9-368b-e5a3e9b7a7cd"), new Guid("92acc37f-4dd4-c818-1aca-1b5993a088cd"), null },
                    { new Guid("b97b025c-630f-8cb2-e795-466d2b16fec8"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("a16880ae-dbaa-9c47-15c1-73e006c2db60"), new Guid("92acc37f-4dd4-c818-1aca-1b5993a088cd"), null },
                    { new Guid("c732e801-54e0-40a2-180d-b3d9561b5acc"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("abb80d87-0568-1d25-a682-1dc3f4b9a923"), new Guid("92acc37f-4dd4-c818-1aca-1b5993a088cd"), null },
                    { new Guid("cbeb5de0-92d3-dff8-5913-e01e879554a2"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("5c278eba-16e6-8478-54e3-4bbe261dc9cd"), new Guid("92acc37f-4dd4-c818-1aca-1b5993a088cd"), null },
                    { new Guid("cc8f3e08-3491-138f-9fcc-bec2b56d09d0"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("a289ec56-a1c3-28c3-dfcd-38faba04e702"), new Guid("92acc37f-4dd4-c818-1aca-1b5993a088cd"), null },
                    { new Guid("ce904764-a638-e3ec-fd1a-ce5e6668e162"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("e649b00f-10f2-63a7-9d83-6d548d650fbb"), new Guid("92acc37f-4dd4-c818-1aca-1b5993a088cd"), null },
                    { new Guid("d1e59df1-0a45-c953-e87c-6f98664f4fe8"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("0e16a590-19da-5ccb-5c5e-002d6a1873e6"), new Guid("92acc37f-4dd4-c818-1aca-1b5993a088cd"), null },
                    { new Guid("d39f35ea-c835-9159-3f38-b9dc32df1e91"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("b61e201c-e31e-b5f3-5b6b-734d52dd3236"), new Guid("92acc37f-4dd4-c818-1aca-1b5993a088cd"), null },
                    { new Guid("dc21788c-7407-3bd9-821d-6e6fa5d52887"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("58f13f47-3286-967c-b3dd-f007105e62ef"), new Guid("92acc37f-4dd4-c818-1aca-1b5993a088cd"), null },
                    { new Guid("e89de91e-75b7-212f-f438-390db16f4b8b"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("a3e8139b-2a76-1335-88e4-bd3f2e0a0cc1"), new Guid("92acc37f-4dd4-c818-1aca-1b5993a088cd"), null },
                    { new Guid("edf2aebe-c859-206b-11d8-afb107763c80"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("8a073b01-3457-fd52-fb35-97f11048f63f"), new Guid("92acc37f-4dd4-c818-1aca-1b5993a088cd"), null }
                });

            migrationBuilder.UpdateData(
                table: "rules",
                keyColumn: "id",
                keyValue: new Guid("60e88433-a40c-0da3-e91a-78e398c6bf1c"),
                column: "name",
                value: "Usuário (leitura)");

            migrationBuilder.UpdateData(
                table: "rules",
                keyColumn: "id",
                keyValue: new Guid("92acc37f-4dd4-c818-1aca-1b5993a088cd"),
                column: "name",
                value: "Administrador");

            migrationBuilder.UpdateData(
                table: "rules",
                keyColumn: "id",
                keyValue: new Guid("eca4b1bc-3f11-091d-7220-6a4ac2047557"),
                column: "name",
                value: "Super administrador");

            migrationBuilder.InsertData(
                table: "rules",
                columns: new[] { "id", "author_created", "author_updated", "code", "created_at", "name", "updated_at" },
                values: new object[,]
                {
                    { new Guid("7a01e8e4-272f-86a5-cbaa-5651bfdf24cc"), null, null, "founder", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Founder", null },
                    { new Guid("88f13fff-76a9-49b1-02d4-3a5dca0c4b03"), null, null, "middle_office", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Middle office (Controle de riscos)", null },
                    { new Guid("99e68c35-b651-8b6e-055c-a33759442737"), null, null, "operador", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Operador (Mesa)", null },
                    { new Guid("a78282c6-0826-0b3b-cb7c-58950284590e"), null, null, "gestor", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Gestor (Diretoria)", null }
                });

            migrationBuilder.InsertData(
                table: "rule_roles",
                columns: new[] { "id", "author_created", "author_updated", "created_at", "role_id", "rule_id", "updated_at" },
                values: new object[,]
                {
                    { new Guid("060ef5b5-6396-b8fd-abda-f9a8b3145245"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("4af1692d-b2ee-21c8-fb83-9fdcd08a7b3c"), new Guid("99e68c35-b651-8b6e-055c-a33759442737"), null },
                    { new Guid("062bbe76-3dea-c178-6555-bb6ddd81b849"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("6d113c17-7ea6-e84c-0dea-420f5c8ae41a"), new Guid("99e68c35-b651-8b6e-055c-a33759442737"), null },
                    { new Guid("08d60ff4-ff17-a886-11a0-6cebd1ae178c"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("cb7df096-8268-3ca4-808d-7ac2e51a96da"), new Guid("7a01e8e4-272f-86a5-cbaa-5651bfdf24cc"), null },
                    { new Guid("09b7f998-4ce9-6e2f-e829-e03d7d3bcd25"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("0cb12fbb-961c-66af-bad6-52e04cc75baf"), new Guid("7a01e8e4-272f-86a5-cbaa-5651bfdf24cc"), null },
                    { new Guid("109d46c5-b42b-12e4-6b19-0062e9f3cdf2"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("b7fdd907-0764-68a3-24d6-8a6015fcd025"), new Guid("7a01e8e4-272f-86a5-cbaa-5651bfdf24cc"), null },
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
                    { new Guid("4e446593-929f-0d72-67b3-0454d687b20d"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("089360bf-26c1-cb8d-3730-b563e1d30452"), new Guid("99e68c35-b651-8b6e-055c-a33759442737"), null },
                    { new Guid("5af4df66-28a0-d963-b824-d8e92c82adb0"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("6d113c17-7ea6-e84c-0dea-420f5c8ae41a"), new Guid("7a01e8e4-272f-86a5-cbaa-5651bfdf24cc"), null },
                    { new Guid("5c7b65ea-7b20-4317-c8f4-0ecd9f05abdf"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("cb7df096-8268-3ca4-808d-7ac2e51a96da"), new Guid("88f13fff-76a9-49b1-02d4-3a5dca0c4b03"), null },
                    { new Guid("5c968571-c02f-b91b-25f9-9f2b5c278c79"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("a3e8139b-2a76-1335-88e4-bd3f2e0a0cc1"), new Guid("a78282c6-0826-0b3b-cb7c-58950284590e"), null },
                    { new Guid("5f2610b9-e549-0b4b-d531-637078987aab"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("58f13f47-3286-967c-b3dd-f007105e62ef"), new Guid("7a01e8e4-272f-86a5-cbaa-5651bfdf24cc"), null },
                    { new Guid("68c04523-c8b0-6dd1-a432-aa5ba291e886"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("a289ec56-a1c3-28c3-dfcd-38faba04e702"), new Guid("7a01e8e4-272f-86a5-cbaa-5651bfdf24cc"), null },
                    { new Guid("83308d38-7c72-4897-34be-8b7efe531c16"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("58f13f47-3286-967c-b3dd-f007105e62ef"), new Guid("a78282c6-0826-0b3b-cb7c-58950284590e"), null },
                    { new Guid("836eef9f-46f8-eba5-6107-a91597f50e64"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("a289ec56-a1c3-28c3-dfcd-38faba04e702"), new Guid("a78282c6-0826-0b3b-cb7c-58950284590e"), null },
                    { new Guid("86a11b0b-1c84-5c18-8b27-def8f4951a83"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("58f13f47-3286-967c-b3dd-f007105e62ef"), new Guid("99e68c35-b651-8b6e-055c-a33759442737"), null },
                    { new Guid("87803ea1-1076-cb86-50e2-5dbec238f432"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("8a073b01-3457-fd52-fb35-97f11048f63f"), new Guid("88f13fff-76a9-49b1-02d4-3a5dca0c4b03"), null },
                    { new Guid("8818697b-9e14-1f98-d336-783ad486c88f"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("8a073b01-3457-fd52-fb35-97f11048f63f"), new Guid("99e68c35-b651-8b6e-055c-a33759442737"), null },
                    { new Guid("89915608-af84-13fb-3fed-4033ee492145"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("089360bf-26c1-cb8d-3730-b563e1d30452"), new Guid("a78282c6-0826-0b3b-cb7c-58950284590e"), null },
                    { new Guid("8a1d977f-acf3-db98-c396-ad29d3b0c20e"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("4af1692d-b2ee-21c8-fb83-9fdcd08a7b3c"), new Guid("a78282c6-0826-0b3b-cb7c-58950284590e"), null },
                    { new Guid("8e8af506-92fd-cd41-53d7-4a0f8830d5e0"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("5c278eba-16e6-8478-54e3-4bbe261dc9cd"), new Guid("7a01e8e4-272f-86a5-cbaa-5651bfdf24cc"), null },
                    { new Guid("920e913b-4226-d286-931e-21a5e79c014b"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("089360bf-26c1-cb8d-3730-b563e1d30452"), new Guid("7a01e8e4-272f-86a5-cbaa-5651bfdf24cc"), null },
                    { new Guid("a36cfab6-e59a-1cf2-92ae-4a240eac6ef6"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("c7cdfc04-a77d-679a-06e8-d3e3f518b22c"), new Guid("7a01e8e4-272f-86a5-cbaa-5651bfdf24cc"), null },
                    { new Guid("a98983b2-b208-51de-f793-b7f9fc103b98"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("a3e8139b-2a76-1335-88e4-bd3f2e0a0cc1"), new Guid("7a01e8e4-272f-86a5-cbaa-5651bfdf24cc"), null },
                    { new Guid("b0e3a7b3-a023-9ddf-0049-44dbf5df895b"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("4af1692d-b2ee-21c8-fb83-9fdcd08a7b3c"), new Guid("88f13fff-76a9-49b1-02d4-3a5dca0c4b03"), null },
                    { new Guid("b9837fb7-7d51-e2f9-f564-fa7b26244b4c"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("0e16a590-19da-5ccb-5c5e-002d6a1873e6"), new Guid("88f13fff-76a9-49b1-02d4-3a5dca0c4b03"), null },
                    { new Guid("c4f40c6e-97be-2eb3-5497-7e6e953a75ba"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("68941986-5142-5dc5-97ab-87a4dd78c29c"), new Guid("a78282c6-0826-0b3b-cb7c-58950284590e"), null },
                    { new Guid("ca49de3f-4e4a-07ec-6525-77c3ca6cd7fc"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("4af1692d-b2ee-21c8-fb83-9fdcd08a7b3c"), new Guid("7a01e8e4-272f-86a5-cbaa-5651bfdf24cc"), null },
                    { new Guid("d2cae4f1-e660-8d2b-9d4e-4658ec71637c"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("68941986-5142-5dc5-97ab-87a4dd78c29c"), new Guid("7a01e8e4-272f-86a5-cbaa-5651bfdf24cc"), null },
                    { new Guid("d3cd4596-614e-4b83-24d7-c0cb2a43906c"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("a289ec56-a1c3-28c3-dfcd-38faba04e702"), new Guid("99e68c35-b651-8b6e-055c-a33759442737"), null },
                    { new Guid("e46064b1-e6b2-31bb-b9e1-20ed7d38c7d0"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("5a4ac9a6-1c8e-167b-9182-80cbf2db6de8"), new Guid("7a01e8e4-272f-86a5-cbaa-5651bfdf24cc"), null },
                    { new Guid("e6e122f2-6b1b-0def-e6af-bac6c0f553c0"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("f8947b81-ab97-7b45-36d7-367384e66e89"), new Guid("7a01e8e4-272f-86a5-cbaa-5651bfdf24cc"), null },
                    { new Guid("e9287493-dda4-a685-5b6b-f2e3c7d061a8"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("8a073b01-3457-fd52-fb35-97f11048f63f"), new Guid("a78282c6-0826-0b3b-cb7c-58950284590e"), null },
                    { new Guid("f6e6da21-b4f6-8787-45c4-66c741a1f1c6"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("abb80d87-0568-1d25-a682-1dc3f4b9a923"), new Guid("a78282c6-0826-0b3b-cb7c-58950284590e"), null },
                    { new Guid("f799892e-dfa2-6bde-f3bc-799bf1c8f90c"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("abb80d87-0568-1d25-a682-1dc3f4b9a923"), new Guid("7a01e8e4-272f-86a5-cbaa-5651bfdf24cc"), null },
                    { new Guid("fa394ae1-bc1a-41e9-d42b-b7ab79b3e506"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("0e16a590-19da-5ccb-5c5e-002d6a1873e6"), new Guid("7a01e8e4-272f-86a5-cbaa-5651bfdf24cc"), null },
                    { new Guid("fef2729b-5cd7-9204-9d5b-5dea464d2704"), null, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("e1479074-56fb-83e9-368b-e5a3e9b7a7cd"), new Guid("7a01e8e4-272f-86a5-cbaa-5651bfdf24cc"), null }
                });

            migrationBuilder.CreateIndex(
                name: "ix_rules_code",
                table: "rules",
                column: "code",
                unique: true);
        }
    }
}
