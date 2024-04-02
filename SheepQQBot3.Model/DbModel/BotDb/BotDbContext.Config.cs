using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace SheepQQBot3.DbModel
{
    /// <summary>
    /// BotDbContext连接部分
    /// </summary>
    public partial class BotDbContext
    {
        public readonly object SyncLock = new();

        private DbSet<BotGroupMessage> _botGroupMessages;

        public virtual DbSet<BotGroupMessage> BotGroupMessages
        {
            get
            {
                lock (SyncLock)
                {
                    return _botGroupMessages;
                }
            }
            set => _botGroupMessages = value;
        }

        private DbSet<SetuDoushiInfo> _setuDoushiInfos;

        public virtual DbSet<SetuDoushiInfo> SetuDoushiInfos
        {
            get
            {
                lock (SyncLock)
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
                lock (SyncLock)
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
        public override ValueTask<object> FindAsync(Type entityType, params object[] keyValues)
        {
            lock (SyncLock) return base.FindAsync(entityType, keyValues);
        }

        /// <inheritdoc />
        public override ValueTask<EntityEntry<TEntity>> AddAsync<TEntity>(
            TEntity entity,
            CancellationToken cancellationToken = default)
            where TEntity : class
        {
            ValueTask<EntityEntry<TEntity>> result;
            lock (SyncLock)
            {
                result = base.AddAsync(entity, cancellationToken);
                SaveChanges();
            }

            return result;
        }

        /// <inheritdoc />
        public override ValueTask<TEntity> FindAsync<TEntity>(
            object[] keyValues,
            CancellationToken cancellationToken)
            where TEntity : class
        {
            lock (SyncLock) return base.FindAsync<TEntity>(keyValues, cancellationToken);
        }

        /// <inheritdoc />
        public override ValueTask<TEntity> FindAsync<TEntity>(params object[] keyValues)
            where TEntity : class
        {
            lock (SyncLock) return base.FindAsync<TEntity>(keyValues);
        }

        /// <inheritdoc />
        public override ValueTask<object> FindAsync(
            Type entityType,
            object[] keyValues,
            CancellationToken cancellationToken)
        {
            lock (SyncLock) return base.FindAsync(entityType, keyValues, cancellationToken);
        }

        /// <inheritdoc />
        public override EntityEntry Update(object entity)
        {
            lock (SyncLock)
            {
                var result = base.Update(entity);
                this.SaveChanges();
                return result;
            }
        }
    }
}