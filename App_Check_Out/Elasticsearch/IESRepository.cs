namespace APP_CHECKOUT.Elasticsearch
{
    public interface IESRepository<TEntity> where TEntity : class
    {
        TEntity FindById(string indexName, object value, string field_name);

    }
}
