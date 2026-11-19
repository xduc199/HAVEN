using Haven.Infrastructure.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Haven.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261119170000_AddUserAvatar")]
public partial class AddUserAvatar : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) =>
        migrationBuilder.AddColumn<string>(
            name: "AvatarPath",
            table: "AspNetUsers",
            type: "nvarchar(256)",
            maxLength: 256,
            nullable: true);

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropColumn(name: "AvatarPath", table: "AspNetUsers");
}
