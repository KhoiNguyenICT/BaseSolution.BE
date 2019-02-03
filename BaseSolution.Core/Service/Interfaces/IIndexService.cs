using System.Threading.Tasks;

namespace BaseSolution.Core.Service.Interfaces
{
    public interface IIndexService<T, TKey>
        where T : class, new()
    {
        Task IndexAllItems();

        Task ReIndexItem(TKey id);

        Task DeleteIndexItem(TKey id);
    }
}