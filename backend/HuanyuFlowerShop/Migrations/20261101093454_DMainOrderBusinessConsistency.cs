using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HuanyuFlowerShop.Migrations
{
    /// <inheritdoc />
    public partial class DMainOrderBusinessConsistency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            AddColumnIfMissing(migrationBuilder, "Users", "Points", "int NOT NULL DEFAULT 0");
            AddColumnIfMissing(migrationBuilder, "Users", "TokenVersion", "int NOT NULL DEFAULT 0");
            AddColumnIfMissing(migrationBuilder, "Orders", "CardMessage", "varchar(500) NULL");
            AddColumnIfMissing(migrationBuilder, "Orders", "ClientRequestId", "varchar(64) NULL");
            AddColumnIfMissing(migrationBuilder, "Orders", "CouponId", "int NULL");
            AddColumnIfMissing(migrationBuilder, "Orders", "DeliveryDate", "datetime(6) NULL");
            AddColumnIfMissing(migrationBuilder, "Orders", "IsAnonymous", "tinyint(1) NOT NULL DEFAULT 0");
            AddColumnIfMissing(migrationBuilder, "Orders", "RequestFingerprint", "varchar(64) NULL");
            AddColumnIfMissing(migrationBuilder, "Orders", "SenderName", "varchar(50) NULL");
            AddColumnIfMissing(migrationBuilder, "Orders", "ShippingMethod", "varchar(30) NOT NULL DEFAULT 'standard'");
            AddColumnIfMissing(migrationBuilder, "Orders", "SubstitutionPreference", "varchar(30) NOT NULL DEFAULT 'contact_me'");
            migrationBuilder.Sql(@"SET @repair_sql = (SELECT IF(COUNT(*) = 0,
                'CREATE UNIQUE INDEX `IX_Orders_UserId_ClientRequestId` ON `Orders` (`UserId`, `ClientRequestId`)', 'SELECT 1')
                FROM information_schema.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Orders' AND INDEX_NAME = 'IX_Orders_UserId_ClientRequestId');
                PREPARE repair_stmt FROM @repair_sql; EXECUTE repair_stmt; DEALLOCATE PREPARE repair_stmt;", suppressTransaction: true);
        }

        private static void AddColumnIfMissing(MigrationBuilder migrationBuilder, string table, string column, string definition)
        {
            var ddl = $"ALTER TABLE `{table}` ADD COLUMN `{column}` {definition}".Replace("'", "''");
            migrationBuilder.Sql($@"SET @repair_sql = (SELECT IF(COUNT(*) = 0, '{ddl}', 'SELECT 1')
                FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = '{table}' AND COLUMN_NAME = '{column}');
                PREPARE repair_stmt FROM @repair_sql; EXECUTE repair_stmt; DEALLOCATE PREPARE repair_stmt;", suppressTransaction: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_UserId_ClientRequestId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Points",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TokenVersion",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CardMessage",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ClientRequestId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CouponId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IsAnonymous",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "RequestFingerprint",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SenderName",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingMethod",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SubstitutionPreference",
                table: "Orders");
        }
    }
}
