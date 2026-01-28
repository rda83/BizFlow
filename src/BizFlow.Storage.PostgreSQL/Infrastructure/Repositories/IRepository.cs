
namespace BizFlow.Storage.PostgreSQL.Infrastructure.Repositories
{
    interface IRepository<TEntity> where TEntity : class
    {
        Task<TEntity> AddAsync(TEntity entity, CancellationToken ct = default);
    }
}
