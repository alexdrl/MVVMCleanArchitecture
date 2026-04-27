using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wpf.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerLastNameValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Update existing records to have a default last name value.
            migrationBuilder.Sql("UPDATE Customers SET LastName = 'Unknown' WHERE LastName = ''");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
