using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace wdb_backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    category = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "employer",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    password = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    verified = table.Column<bool>(type: "boolean", nullable: false),
                    blockchain_address = table.Column<string>(type: "text", nullable: true),
                    private_key = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employer", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "request",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    employer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    worker_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reason = table.Column<string>(type: "text", nullable: false),
                    expiry_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    custom_request = table.Column<string>(type: "text", nullable: true),
                    custom_request_status = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_request", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "worker",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    password = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    verified = table.Column<bool>(type: "boolean", nullable: false),
                    blockchain_address = table.Column<string>(type: "text", nullable: true),
                    private_key = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_worker", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fields",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    field = table.Column<string>(type: "text", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    label = table.Column<string>(type: "text", nullable: false),
                    allowed_type = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fields", x => x.id);
                    table.ForeignKey(
                        name: "FK_fields_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "notification",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    recipient_worker_id = table.Column<Guid>(type: "uuid", nullable: true),
                    recipient_employer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    type = table.Column<string>(type: "text", nullable: false),
                    request_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_read = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification", x => x.id);
                    table.CheckConstraint("CK_notification_single_recipient", "(recipient_worker_id IS NOT NULL) <> (recipient_employer_id IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_notification_request_request_id",
                        column: x => x.request_id,
                        principalTable: "request",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "worker_info",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    worker_id = table.Column<Guid>(type: "uuid", nullable: false),
                    field_id = table.Column<Guid>(type: "uuid", nullable: true),
                    custom_label = table.Column<string>(type: "text", nullable: true),
                    type = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_worker_info", x => x.id);
                    table.CheckConstraint("CK_worker_info_field_xor_custom", "(field_id IS NOT NULL) <> (custom_label IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_worker_info_fields_field_id",
                        column: x => x.field_id,
                        principalTable: "fields",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_worker_info_worker_worker_id",
                        column: x => x.worker_id,
                        principalTable: "worker",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "permission",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    worker_id = table.Column<Guid>(type: "uuid", nullable: false),
                    field_id = table.Column<Guid>(type: "uuid", nullable: true),
                    info_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    last_updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_permission", x => x.id);
                    table.CheckConstraint("CK_permission_approved_has_info", "status NOT IN (1, 3) OR info_id IS NOT NULL");
                    table.CheckConstraint("CK_permission_field_or_info", "field_id IS NOT NULL OR info_id IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_permission_fields_field_id",
                        column: x => x.field_id,
                        principalTable: "fields",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_permission_request_request_id",
                        column: x => x.request_id,
                        principalTable: "request",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_permission_worker_info_info_id",
                        column: x => x.info_id,
                        principalTable: "worker_info",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_categories_category",
                table: "categories",
                column: "category",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_fields_category_id_field",
                table: "fields",
                columns: new[] { "category_id", "field" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_fields_category_id_label",
                table: "fields",
                columns: new[] { "category_id", "label" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_notification_request_id",
                table: "notification",
                column: "request_id");

            migrationBuilder.CreateIndex(
                name: "IX_permission_field_id",
                table: "permission",
                column: "field_id");

            migrationBuilder.CreateIndex(
                name: "IX_permission_info_id",
                table: "permission",
                column: "info_id");

            migrationBuilder.CreateIndex(
                name: "IX_permission_request_id",
                table: "permission",
                column: "request_id");

            migrationBuilder.CreateIndex(
                name: "IX_worker_info_field_id",
                table: "worker_info",
                column: "field_id");

            migrationBuilder.CreateIndex(
                name: "IX_worker_info_worker_id_custom_label",
                table: "worker_info",
                columns: new[] { "worker_id", "custom_label" },
                unique: true,
                filter: "custom_label IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_worker_info_worker_id_field_id",
                table: "worker_info",
                columns: new[] { "worker_id", "field_id" },
                unique: true,
                filter: "field_id IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "employer");

            migrationBuilder.DropTable(
                name: "notification");

            migrationBuilder.DropTable(
                name: "permission");

            migrationBuilder.DropTable(
                name: "request");

            migrationBuilder.DropTable(
                name: "worker_info");

            migrationBuilder.DropTable(
                name: "fields");

            migrationBuilder.DropTable(
                name: "worker");

            migrationBuilder.DropTable(
                name: "categories");
        }
    }
}
