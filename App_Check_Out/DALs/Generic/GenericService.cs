using APP_CHECKOUT.Models.SQL;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DAL.Generic
{
    public class GenericService<TEntity> : IGenericService<TEntity> where TEntity : class
    {
        public static string _connection;

        public GenericService(string connection)
        {
            _connection = connection;
        }

        public long Create(TEntity entity)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    _DbContext.Set<TEntity>().Add(entity);
                    _DbContext.SaveChangesAsync();
                    return Convert.ToInt64(entity.GetType().GetProperty("Id").GetValue(entity, null));
                }
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        public async Task<long> CreateAsync(TEntity entity)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    await _DbContext.Set<TEntity>().AddAsync(entity);
                    await _DbContext.SaveChangesAsync();
                    return Convert.ToInt64(entity.GetType().GetProperty("Id").GetValue(entity, null));
                }
            }
            catch (Exception ex)
            {
                return 0;
            }
        }


        public void Delete(object id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var entity = Find(id);
                    _DbContext.Set<TEntity>().Remove(entity);
                    _DbContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
            }
        }

        public async Task DeleteAsync(object id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var entity = await FindAsync(id);
                    _DbContext.Set<TEntity>().Remove(entity);
                    await _DbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
            }
        }


        public void Update(TEntity entity)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    _DbContext.Set<TEntity>().Update(entity);
                    _DbContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
            }
        }
        public async Task UpdateAsync(TEntity entity)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    _DbContext.Set<TEntity>().Update(entity);
                    await _DbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
            }
        }


        public List<TEntity> GetAll()
        {
            try
            {
                var _DbContext = new EntityDataContext(_connection);
                return _DbContext.Set<TEntity>().AsNoTracking().ToList();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task<List<TEntity>> GetAllAsync()
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return await _DbContext.Set<TEntity>().AsNoTracking().ToListAsync();
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        public TEntity Find(object id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return _DbContext.Set<TEntity>().Find(id);
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task<TEntity> FindAsync(object id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return await _DbContext.Set<TEntity>().FindAsync(id);
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public List<TEntity> GetByCondition(Expression<Func<TEntity, bool>> expression)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return _DbContext.Set<TEntity>().AsNoTracking().Where(expression).ToList();
                }
            }
            catch
            {
                return null;
            }
        }
        public async Task<List<TEntity>> GetByConditionAsync(Expression<Func<TEntity, bool>> expression)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return await _DbContext.Set<TEntity>().AsNoTracking().Where(expression).ToListAsync();
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
