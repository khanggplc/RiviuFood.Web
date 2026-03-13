namespace RiviuFood.Web.Repositories;

public interface IGenericRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();

    // Hàm mới: Lấy dữ liệu kèm theo các bảng liên quan (ví dụ: lấy Post kèm User)
    Task<T?> GetFirstOrDefaultAsync(
        System.Linq.Expressions.Expression<Func<T, bool>> predicate,
        params System.Linq.Expressions.Expression<Func<T, object>>[] includes);

    Task<T?> GetByIdAsync(object id);
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
    Task<bool> SaveChangesAsync();
}