namespace Events
{
    public interface IEvent
    {
        void Invoke();
        void Register(IEventListener aListener);
        void Unregister(IEventListener aListener);
    }

    public interface IEvent<T>
    {
        void Invoke(T aValue);
        void Register(IEventListener<T> aListener);
        void Unregister(IEventListener<T> aListener);
    }

    public interface IEvent<Ta, Tb>
    {
        void Invoke(Ta aValue, Tb aValue2);
        void Register(IEventListener<Ta, Tb> aListener);
        void Unregister(IEventListener<Ta, Tb> aListener);
    }
}