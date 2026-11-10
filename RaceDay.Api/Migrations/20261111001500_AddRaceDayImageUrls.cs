using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RaceDay.Api.Data;

#nullable disable

namespace RaceDay.Api.Migrations;

[DbContext(typeof(RaceDayDbContext))]
[Migration("20261111001500_AddRaceDayImageUrls")]
public partial class AddRaceDayImageUrls : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(name: "BannerImageUrl", table: "Events", type: "nvarchar(2048)", maxLength: 2048, nullable: true);
        migrationBuilder.AddColumn<string>(name: "ProfilePictureUrl", table: "Users", type: "nvarchar(2048)", maxLength: 2048, nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "BannerImageUrl", table: "Events");
        migrationBuilder.DropColumn(name: "ProfilePictureUrl", table: "Users");
    }
}
