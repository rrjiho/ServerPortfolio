﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Server.DB
{
    public class AppDbContext : DbContext
    {
        public DbSet<AccountDb> Accounts { get; set; }
        public DbSet<PlayerDb> Players { get; set; }
        public DbSet<ItemDb> Items { get; set; }
        public DbSet<QuestDb> Quests { get; set; }
        public DbSet<PostDb> Posts { get; set; }

        static readonly ILoggerFactory _logger = LoggerFactory.Create(builder => { builder.AddConsole(); });

        string _connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=GameDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options               
                .UseSqlServer(ConfigManager.Config == null ? _connectionString : ConfigManager.Config.connectionString);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<AccountDb>()
                .HasIndex(a => a.AccountName)
                .IsUnique();

            builder.Entity<PlayerDb>()
                .HasIndex(p => p.PlayerName)
                .IsUnique();

            builder.Entity<PostDb>()
                .HasIndex(p => new { p.PlayerDbId, p.PostNumber });
        }
    }
}
