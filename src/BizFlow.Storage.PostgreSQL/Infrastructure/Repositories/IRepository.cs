

namespace BizFlow.Storage.PostgreSQL.Infrastructure.Repositories
{
    interface IRepository<TEntity> where TEntity : class
    {
        void SetUnitOfWork(UnitOfWork uow);
        Task<TEntity> AddAsync(TEntity entity, CancellationToken ct = default);
        Task<TEntity> GetByUniqueColumnAsync(string fieldName, object value, CancellationToken ct = default);
        
        Task<IEnumerable<TEntity>> GetByColumnAsync(string fieldName, object value, CancellationToken ct = default);
        Task<IEnumerable<TEntity>> GetPagedAsync(long lastId, int limit = 100, CancellationToken ct = default);

        Task<IEnumerable<TEntity>> GetPagedNewAsync(PagedQuery pagedQuery, CancellationToken ct = default);

        Task<int> DeleteAsync(string fieldName, object value, CancellationToken ct = default);
        Task<bool> UpdateAsync(TEntity entity, CancellationToken ct = default);
    }
}
