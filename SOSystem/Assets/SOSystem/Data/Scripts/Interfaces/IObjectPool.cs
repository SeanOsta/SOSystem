
namespace Data
{
    public interface IObjectPool<T>
    {
        void Initialize();

        void Add(T aItem);
        T Extract();
    }
}
