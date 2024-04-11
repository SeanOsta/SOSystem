
namespace Data
{
    public interface IPoolable<T>
    {
        System.Action<T> onReturnToPool { get; set; }

        void ReturnToPool();

        //Add custom enabling/disabling for greater control. Ex. it might be much more performant
        //to only enable/disable a component vs the whole hierarchy
        void Enable();
        void Disable();
    }
}
