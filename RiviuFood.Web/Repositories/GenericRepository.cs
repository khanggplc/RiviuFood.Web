using Microsoft.EntityFrameworkCore;
using RiviuFood.Web.Data;

namespace RiviuFood.Web.Repositories;

public class GenericRepository<T>(ApplicationDbContext context) : IGenericRepository<T> where T : class
{
    private readonly ApplicationDbContext _context = context;

    public async Task<IEnumerable<T>> GetAllAsync() => await _context.Set<T>().ToListAsync();
    public async Task<T?> GetByIdAsync(object id) => await _context.Set<T>().FindAsync(id);
    public async Task AddAsync(T entity) => await _context.Set<T>().AddAsync(entity);
    public void Update(T entity) => _context.Set<T>().Update(entity);
    public void Delete(T entity) => _context.Set<T>().Remove(entity);
    public async Task<bool> SaveChangesAsync() => await _context.SaveChangesAsync() > 0;
}