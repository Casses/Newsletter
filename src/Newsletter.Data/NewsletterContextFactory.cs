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
        
        // Prioritize environment variable over appsettings.json
        var connectionString = Environment.GetEnvironmentVariable("AZURE_SQL_CONNECTION_STRING") ?? 
                              configuration.GetConnectionString("DefaultConnection");
        
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Connection string 'AZURE_SQL_CONNECTION_STRING' not found in environment variables or 'DefaultConnection' not found in configuration.");
        }
        
        optionsBuilder.UseSqlServer(connectionString);

        return new NewsletterContext(optionsBuilder.Options);
    }
} 