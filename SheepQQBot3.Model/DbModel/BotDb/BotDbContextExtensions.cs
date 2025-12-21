namespace SheepQQBot3.Model.DbModel;

public static class BotDbContextExtensions
{
    //public static T FindLock<T>(this DbSet<T> dbSet, params object[] keyValues)
    //    where T : class
    //{
    //    var context = (BotDbContext)dbSet.GetService<ICurrentDbContext>().Context;
    //    T result;
    //    lock (context.SyncLock)
    //    {
    //        result = dbSet.Find(keyValues);
    //    }

    //    return result;
    //}

    //public static void AddLock<T>(this BotDbContext botDbContext, T obj)
    //    where T : class
    //{
    //    lock (botDbContext.SyncLock)
    //    {
    //        botDbContext.Add(obj);
    //        botDbContext.SaveChanges();
    //    }
    //}
}