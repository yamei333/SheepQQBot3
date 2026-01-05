using Microsoft.EntityFrameworkCore;
using System;
using System.IO;

namespace SheepQQBot3.DbModel;

/// <summary>
/// BotDbContext连接部分
/// </summary>
public partial class BotDbContext
{
    public virtual DbSet<BotGroupMessage> BotGroupMessages { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
            optionsBuilder.UseSqlite($"Data Source={Path.Combine(Environment.CurrentDirectory, "SheepQQBot3.db")}");
    }
}