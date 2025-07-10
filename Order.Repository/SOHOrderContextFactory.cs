using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Order.Repository.DataContexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Repository
{
    public class SOHOrderContextFactory : IDesignTimeDbContextFactory<SOHOrderDBContext>
    {
        private static string _connectionString;

        public SOHOrderDBContext CreateDbContext()
        {
            return CreateDbContext(null);
        }

        public SOHOrderDBContext CreateDbContext(string connectionString, int intValue = 0)
        {
            _connectionString = connectionString;
            return CreateDbContext(null);
        }

        public SOHOrderDBContext CreateDbContext(string[] args)
        {
            if (string.IsNullOrEmpty(_connectionString))
            {
                LoadConnectionString();
            }

            var builder = new DbContextOptionsBuilder<SOHOrderDBContext>();
            builder.UseSqlServer(_connectionString);

            builder.UseSqlServer(_connectionString,
                sqlServerOptionsAction: sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 90, // Number of retry attempts
                        maxRetryDelay: TimeSpan.FromSeconds(30), // Delay between retries
                        errorNumbersToAdd: null); // Additional error numbers to consider as transient errors
                });

            return new SOHOrderDBContext(builder.Options);
        }

        private static void LoadConnectionString()
        {
            _connectionString = Environment.GetEnvironmentVariable("sqldb_connection");
        }
    }
}
