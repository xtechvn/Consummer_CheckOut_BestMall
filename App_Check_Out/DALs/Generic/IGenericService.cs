using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Generic
{
    public interface IGenericService<TEntity> where TEntity : class
    {
        /// <summary>
        /// Get all row from a table
        /// </summary>
        /// <returns></returns>
        List<TEntity> GetAll();
        Task<List<TEntity>> GetAllAsync();

        /// <summary>
        /// Get row by id in a table
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        TEntity Find(object id);
        Task<TEntity> FindAsync(object id);

        /// <summary>
        /// Insert data into a table
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        long Create(TEntity entity);
        Task<long> CreateAsync(TEntity entity);

        /// <summary>
        ///  Update data in a table
        /// </summary>
        /// <param name="entity"></param>
        void Update(TEntity entity);
        Task UpdateAsync(TEntity entity);

        /// <summary>
        /// Delete data in a table
        /// </summary>
        /// <param name="id"></param>
        void Delete(object id);
        Task DeleteAsync(object id);

        List<TEntity> GetByCondition(Expression<Func<TEntity, bool>> expression);
        Task<List<TEntity>> GetByConditionAsync(Expression<Func<TEntity, bool>> expression);
    }
}
