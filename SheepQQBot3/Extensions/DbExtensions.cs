using Microsoft.EntityFrameworkCore;
using SheepQQBot3.DbModel;

namespace SheepQQBot3.Extensions
{
    public static class DbExtensions
    {
        /// <summary>
        /// 取得数据库Context
        /// </summary>
        public static BotDbContext CreateBotDbContext() => new(new DbContextOptions<BotDbContext>());
    }
}