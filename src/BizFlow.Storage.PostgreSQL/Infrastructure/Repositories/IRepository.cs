
namespace BizFlow.Storage.PostgreSQL.Infrastructure.Repositories
{
    interface IRepository<TEntity> where TEntity : class
    {
        void SetUnitOfWork(UnitOfWork uow);
        Task<TEntity> AddAsync(TEntity entity, CancellationToken ct = default);
        Task<TEntity> GetByColumnAsync(string fieldName, object value, CancellationToken ct = default);
    }
}
