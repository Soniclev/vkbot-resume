using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using VkBot.Information.User.Models;

namespace VkBot.Information.User
{
    public class UserContext : DbContext
    {
        private const string DbPath = "VarData/users.db";
        public DbSet<Nickname> Nicknames { get; set; }
        public DbSet<GeoPosition> GeoPositions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={DbPath}");
        }
    }
}
