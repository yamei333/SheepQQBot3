using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace SheepQQBot3.DBModel
{
    /// <summary>
    /// BotDbContext连接部分
    /// </summary>
    public partial class BotDbContext
    {
        private static readonly object _syncLock = new();

        private DbSet<SetuDoushiInfo> _setuDoushiInfos;

        public virtual DbSet<SetuDoushiInfo> SetuDoushiInfos
        {
            get
            {
                lock (_syncLock)
                {
                    return _setuDoushiInfos;
                }
            }
            set => _setuDoushiInfos = value;
        }

        private DbSet<SetuSendHistory> _setuSendHistorys;

        public virtual DbSet<SetuSendHistory> SetuSendHistorys
        {
            get
            {
                lock (_syncLock)
                {
                    return _setuSendHistorys;
                }
            }
            set => _setuSendHistorys = value;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
                optionsBuilder.UseSqlite($"Data Source={Path.Combine(Environment.CurrentDirectory, "SheepQQBot3.db")}");
        }

        /// <inheritdoc />
        public override ValueTask<EntityEntry<TEntity>> AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = new CancellationToken())
        {
            lock (_syncLock)
            {
                var result = base.AddAsync(entity, cancellationToken);
                this.SaveChanges();
                return result;
            }
        }

        /// <inheritdoc />
        public override ValueTask<TEntity> FindAsync<TEntity>(params object[] keyValues) where TEntity : class
        {
            lock (_syncLock)
            {
                return base.FindAsync<TEntity>(keyValues); ;
            }
        }

        /// <inheritdoc />
        public override EntityEntry<TEntity> Update<TEntity>(TEntity entity)
        {
            lock (_syncLock)
            {
                var result = base.Update(entity);
                this.SaveChanges();
                return result;
            }
        }
    }
}