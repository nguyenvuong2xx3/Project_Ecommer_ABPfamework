using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace Acme.SimpleTaskApp.EntityFrameworkCore
{
    public static class SimpleTaskAppDbContextConfigurer
    {
        public static void Configure(DbContextOptionsBuilder<SimpleTaskAppDbContext> builder, string connectionString)
        {
            builder.UseSqlServer(connectionString);
        }

        public static void Configure(DbContextOptionsBuilder<SimpleTaskAppDbContext> builder, DbConnection connection)
        {
            builder.UseSqlServer(connection);
        }
    }
}
