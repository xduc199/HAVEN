using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Haven.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddListingVideosAndAdministrativeLocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProvinceCode",
                table: "Listings",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WardCode",
                table: "Listings",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AdministrativeProvinces",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdministrativeProvinces", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "ListingVideos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ListingId = table.Column<int>(type: "int", nullable: false),
                    StorageKey = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Length = table.Column<long>(type: "bigint", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListingVideos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ListingVideos_Listings_ListingId",
                        column: x => x.ListingId,
                        principalTable: "Listings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AdministrativeWards",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    ProvinceCode = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdministrativeWards", x => x.Code);
                    table.UniqueConstraint("AK_AdministrativeWards_Code_ProvinceCode", x => new { x.Code, x.ProvinceCode });
                    table.ForeignKey(
                        name: "FK_AdministrativeWards_AdministrativeProvinces_ProvinceCode",
                        column: x => x.ProvinceCode,
                        principalTable: "AdministrativeProvinces",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Listings_ProvinceCode",
                table: "Listings",
                column: "ProvinceCode");

            migrationBuilder.CreateIndex(
                name: "IX_Listings_WardCode_ProvinceCode",
                table: "Listings",
                columns: new[] { "WardCode", "ProvinceCode" });

            migrationBuilder.CreateIndex(
                name: "IX_AdministrativeProvinces_Name",
                table: "AdministrativeProvinces",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdministrativeWards_ProvinceCode_Name",
                table: "AdministrativeWards",
                columns: new[] { "ProvinceCode", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_ListingVideos_ListingId_SortOrder",
                table: "ListingVideos",
                columns: new[] { "ListingId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_ListingVideos_StorageKey",
                table: "ListingVideos",
                column: "StorageKey",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Listings_AdministrativeProvinces_ProvinceCode",
                table: "Listings",
                column: "ProvinceCode",
                principalTable: "AdministrativeProvinces",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Listings_AdministrativeWards_WardCode_ProvinceCode",
                table: "Listings",
                columns: new[] { "WardCode", "ProvinceCode" },
                principalTable: "AdministrativeWards",
                principalColumns: new[] { "Code", "ProvinceCode" },
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Listings_AdministrativeProvinces_ProvinceCode",
                table: "Listings");

            migrationBuilder.DropForeignKey(
                name: "FK_Listings_AdministrativeWards_WardCode_ProvinceCode",
                table: "Listings");

            migrationBuilder.DropTable(
                name: "AdministrativeWards");

            migrationBuilder.DropTable(
                name: "ListingVideos");

            migrationBuilder.DropTable(
                name: "AdministrativeProvinces");

            migrationBuilder.DropIndex(
                name: "IX_Listings_ProvinceCode",
                table: "Listings");

            migrationBuilder.DropIndex(
                name: "IX_Listings_WardCode_ProvinceCode",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "ProvinceCode",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "WardCode",
                table: "Listings");
        }
    }
}
