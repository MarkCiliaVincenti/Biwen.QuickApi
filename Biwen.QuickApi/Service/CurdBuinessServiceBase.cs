﻿using Biwen.QuickApi.UnitOfWork;
using Biwen.QuickApi.UnitOfWork.Pagenation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Biwen.QuickApi.Service
{
    /// <summary>
    /// Entity Curd Service Base
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TDbContext"></typeparam>
    public abstract class CurdBuinessServiceBase<TEntity, TDbContext> : BuinessServiceBase<TDbContext>, ICurdService<TEntity>
        where TEntity : class
        where TDbContext : DbContext
    {
        /// <summary>
        /// 仓储
        /// </summary>
        protected IRepository<TEntity> Repository => Uow.GetRepository<TEntity>();

        public CurdBuinessServiceBase(IUnitOfWork<TDbContext> uow, ILogger? logger = null) : base(uow, logger)
        {
        }

        public virtual async Task AddAsync(TEntity entity)
        {
            await Repository.InsertAsync(entity);
            await Uow.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(TEntity entity)
        {
            Repository.Delete(entity);
            await Uow.SaveChangesAsync();
        }

        public virtual async Task<TEntity> GetAsync(object[] ids)
        {
            return await Repository.FindAsync(ids) ?? throw new Exception("未找到数据");
        }

        public virtual async Task UpdateAsync(TEntity entity)
        {
            Repository.Update(entity);
            await Uow.SaveChangesAsync();
        }

        public virtual async Task<IPagedList<TEntity>> GetPagedList(
            int pageIndex = 0,
            int pageSize = 20,
            Expression<Func<TEntity, bool>>? predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null)
        {
            return await Repository.GetPagedListAsync(
                predicate: predicate,
                pageIndex: pageIndex,
                pageSize: pageSize,
                orderBy: orderBy,
                include: include);
        }
    }
}