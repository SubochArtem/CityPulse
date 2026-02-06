namespace Users.DataAccess.Interfaces;
using Users.DataAccess.Entities;

public interface IRepository<T> where T : EntityBase
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task CreateAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(Guid id);
    Task SaveChangesAsync();
}
