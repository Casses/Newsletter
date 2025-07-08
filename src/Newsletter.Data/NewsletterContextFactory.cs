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
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<NewsletterContext>();
        var connectionString = configuration.GetConnectionString("DefaultConnection") ?? 
                              Environment.GetEnvironmentVariable("AZURE_SQL_CONNECTION_STRING");
        
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found in configuration or environment variables.");
        }
        
        optionsBuilder.UseSqlServer(connectionString);

        return new NewsletterContext(optionsBuilder.Options);
    }
} 