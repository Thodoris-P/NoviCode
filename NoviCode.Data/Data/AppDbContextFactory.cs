using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NoviCode.Data.Data;

public class NoviCodeContextFactory : IDesignTimeDbContextFactory<NoviCodeContext>
{
    public NoviCodeContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<NoviCodeContext>();
        
        optionsBuilder.UseSqlServer("Server=localhost,1433;Database=NoviCode;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;");
        return new NoviCodeContext(optionsBuilder.Options);
    }
}