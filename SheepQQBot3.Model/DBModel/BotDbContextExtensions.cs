using Microsoft.EntityFrameworkCore;

namespace SheepQQBot3.DbModel;

public static class BotDbContextExtensions
{
    private static readonly object _syncLock = new();

    public static T Find<T>(this DbSet<T> dbSet, params object[] keyValues)
        where T : class
    {
        lock (_syncLock)
        {
            return dbSet.Find(keyValues);
        }
    }

    public static void AddLock<T>(this BotDbContext botDbContext, T obj)
        where T : class
    {
        lock (_syncLock)
        {
            botDbContext.Add(obj);
            botDbContext.SaveChanges();
        }
    }
}