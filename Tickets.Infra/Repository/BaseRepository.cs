using Tickets.Domain.IRepository;
using Tickets.Infra.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Tickets.Infra.Repository
{
    public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class
    {
        private readonly ApplicationDbContext _appDbContext;
        public DbSet<TEntity> _dbSet => _appDbContext.Set<TEntity>();

        public BaseRepository(ApplicationDbContext context) => _appDbContext = context;

        #region GetByID
        public TEntity GetByGuid(Guid id)
        {
            return _dbSet.Find(id)
                ?? throw new InvalidOperationException("Entity not found.");
        }

        public async Task<TEntity> GetByGuidAsync(Guid id)
        {
            return await _dbSet.FindAsync(id)
                ?? throw new KeyNotFoundException("Entity not found.");
        }

        public async Task<TEntity> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id)
                ?? throw new KeyNotFoundException("Entity not found.");
        }

        public bool Exists(Guid id)
        {
            return _dbSet.Find(id) != null;
        }
        #endregion
        #region Get
        public TEntity? Get(Expression<Func<TEntity, bool>> predicate)
        {
            return _dbSet.FirstOrDefault(predicate);
        }
        public async Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }
        public IEnumerable<TEntity> GetList(Expression<Func<TEntity, bool>> predicate)
        {
            return _dbSet.Where(predicate).ToList();
        }
        public async Task<IEnumerable<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }
        public async Task<IEnumerable<TEntity>> GetListAsync()
        {
            return await _dbSet.ToListAsync();
        }
        #endregion
        #region Add
        public int Add(TEntity entity)
        {
            _dbSet.Add(entity);
            return _appDbContext.SaveChanges();
        }
        public int AddRange(List<TEntity> entity)
        {
            _dbSet.AddRange(entity);
            return _appDbContext.SaveChanges();
        }
        public void AddWithoutSave(TEntity entity) => _dbSet.Add(entity);
        public void AddRangeWithoutSave(List<TEntity> entity) => _dbSet.AddRange(entity);
        public Task<int> AddAsync(TEntity entity)
        {
            _dbSet.AddAsync(entity);
            return _appDbContext.SaveChangesAsync();
        }
        public Task<int> AddRangeAsync(List<TEntity> entity)
        {
            _dbSet.AddRangeAsync(entity);
            return _appDbContext.SaveChangesAsync();
        }
        #endregion
        #region Update
        public int Update(TEntity entity)
        {
            _dbSet.Entry(entity).State = EntityState.Modified;
            return _appDbContext.SaveChanges();
        }
        public void UpdateWithoutSave(TEntity entity) => _dbSet.Entry(entity).State = EntityState.Modified;
        public async Task<int> UpdateAsync(TEntity entity)
        {
            _dbSet.Entry(entity).State = EntityState.Modified;
            var result = await _appDbContext.SaveChangesAsync();
            Console.WriteLine($"Rows affected: {result}");
            return result;
        }
        #endregion
        #region Delete
        public int Remove(TEntity entity)
        {
            _dbSet.Remove(entity);
            return _appDbContext.SaveChanges();
        }
        public void RemoveWithoutSave(TEntity entity) => _dbSet.Remove(entity);
        public Task<int> RemoveAsync(TEntity entity)
        {
            _dbSet.Remove(entity);
            return _appDbContext.SaveChangesAsync();
        }
        #endregion
        #region Save_Dis
        public int Save()
        {
            return _appDbContext.SaveChanges();
        }
        public void Dispose()
        {
            _appDbContext.Dispose();
        }
        #endregion
        #region new Get methods
        #endregion
    }
}
