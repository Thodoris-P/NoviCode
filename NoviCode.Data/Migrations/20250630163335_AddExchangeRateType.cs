using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NoviCode.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddExchangeRateType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            CREATE TYPE dbo.ExchangeRateType AS TABLE
            (
                Currency    NVARCHAR(3)   NOT NULL,
                EffectiveAt DATETIME2     NOT NULL,
                Rate        DECIMAL(18,6) NOT NULL
            );
        ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TYPE IF EXISTS dbo.ExchangeRateType;");
        }
    }
}
