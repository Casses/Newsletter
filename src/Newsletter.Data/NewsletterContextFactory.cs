using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Newsletter.Data;

public class NewsletterContextFactory : IDesignTimeDbContextFactory<NewsletterContext>
{
    public NewsletterContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<NewsletterContext>();
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        if (string.IsNullOrEmpty(connectionString))
        {
            // Fallback connection string for design-time
            connectionString = "Server=(localdb)\\mssqllocaldb;Database=NewsletterDb;Trusted_Connection=True;MultipleActiveResultSets=true";
        }

        optionsBuilder.UseSqlServer(connectionString);

        return new NewsletterContext(optionsBuilder.Options);
    }
} 