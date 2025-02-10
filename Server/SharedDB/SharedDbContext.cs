using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedDB
{
    public class SharedDbContext : DbContext
    {
        public DbSet<TokenDb> Tokens { get; set; }
        public DbSet<ServerDb> Servers { get; set; }

        // GameServer
        public SharedDbContext()
        {
            
        }

        // ASP.NET ( AccountServer에서 이미 연결해주고 있음)
        public SharedDbContext(DbContextOptions<SharedDbContext> options) : base(options)
        {

        }

        // GameServer
        public static string ConnectionString { get; set; } = @"Data Source=52.231.109.108;Initial Catalog=SharedDB;User ID=sa;Password=Aa0123456789;TrustServerCertificate=True";

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (options.IsConfigured == false)
            {
                options
                    .UseSqlServer(ConnectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<TokenDb>()
                .HasIndex(t => t.AccountDbId)
                .IsUnique();

            builder.Entity<ServerDb>()
                .HasIndex(s => s.ServerDbId)
                .IsUnique();
        }

    }
}
