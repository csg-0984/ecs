using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KitchenAfterSales.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AuditSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Customers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Customers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Appliances",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Appliances",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MainImageUrl",
                table: "Appliances",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WarrantyFileUrl",
                table: "Appliances",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "WorkOrders",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "WorkOrders",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EntityName = table.Column<string>(type: "TEXT", nullable: false),
                    EntityId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Action = table.Column<string>(type: "TEXT", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Appliances");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Appliances");

            migrationBuilder.DropColumn(
                name: "MainImageUrl",
                table: "Appliances");

            migrationBuilder.DropColumn(
                name: "WarrantyFileUrl",
                table: "Appliances");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "WorkOrders");
        }
    }
}