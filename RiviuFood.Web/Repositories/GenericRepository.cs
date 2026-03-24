using Microsoft.EntityFrameworkCore;
using RiviuFood.Web.Data;

namespace RiviuFood.Web.Repositories;

public class GenericRepository<T>(ApplicationDbContext context) : IGenericRepository<T> where T : class
{
    private readonly ApplicationDbContext _context = context;

    public async Task<IEnumerable<T>> GetAllAsync() => await _context.Set<T>().ToListAsync();

    // Triển khai hàm lấy kèm bảng liên quan
    public async Task<T?> GetFirstOrDefaultAsync(
        System.Linq.Expressions.Expression<Func<T, bool>> predicate,
        params System.Linq.Expressions.Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _context.Set<T>();

        // Thực hiện "nối" các bảng liên quan
        if (includes != null)
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
        }

        return await query.FirstOrDefaultAsync(predicate);
    }

    public async Task<T?> GetByIdAsync(object id) => await _context.Set<T>().FindAsync(id);
    public async Task AddAsync(T entity) => await _context.Set<T>().AddAsync(entity);
    public void Update(T entity) => _context.Set<T>().Update(entity);
    public void Delete(T entity) => _context.Set<T>().Remove(entity);
    public async Task<bool> SaveChangesAsync() => await _context.SaveChangesAsync() > 0;

    public Task<IEnumerable<T>> GetAllAsync(string includeProperties)
    {
        throw new NotImplementedException();
    }

    Task<IEnumerable<object>> IGenericRepository<T>.GetAllAsync()
    {
        throw new NotImplementedException();
    }
}