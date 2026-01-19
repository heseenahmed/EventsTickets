using Tickets.Domain.Entity;
using Microsoft.EntityFrameworkCore.Query;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Tickets.Domain.IRepository
{
    public interface IBaseRepository<TEntity> where TEntity : class
    {
        #region Create_Update_Delete Async
        Task<int> AddAsync(TEntity entity);
        Task<int> AddRangeAsync(List<TEntity> entity);
        Task<int> UpdateAsync(TEntity entity);
        Task<int> RemoveAsync(TEntity entity);
        #endregion

        #region Create_Update_Delete
        int Add(TEntity entity);
        int AddRange(List<TEntity> entity);
        int Update(TEntity entity);
        int Remove(TEntity entity);
        void AddWithoutSave(TEntity entity);
        void AddRangeWithoutSave(List<TEntity> entity);
        void UpdateWithoutSave(TEntity entity);
        void RemoveWithoutSave(TEntity entity);
        #endregion

        #region Get 
        TEntity GetByGuid(Guid id);
        bool Exists(Guid id);
        Task<TEntity> GetByGuidAsync(Guid id);
        Task<TEntity> GetByIdAsync(int id);
        TEntity? Get(Expression<Func<TEntity, bool>> predicate);
        Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate);
        IEnumerable<TEntity> GetList(Expression<Func<TEntity, bool>> predicate);
        Task<IEnumerable<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate);
        Task<IEnumerable<TEntity>> GetListAsync();
        IQueryable<TEntity> GetAllQueryable();
        #endregion
        #region Save_Dis
        int Save();
        void Dispose();
        #endregion
    }
}
