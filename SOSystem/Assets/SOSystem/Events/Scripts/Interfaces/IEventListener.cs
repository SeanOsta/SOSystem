namespace Events
{
    public interface IDebugListener
    {
        void PopulateLogs(System.Text.StringBuilder aBuilder);
    }

    public interface IEventListener : IDebugListener
    {
        void OnEventInvoked();
    }

    public interface IEventListener<T> : IDebugListener
    {
        void OnEventInvoked(T aValue);
    }

    public interface IEventListener<Ta, Tb> : IDebugListener
    {
        void OnEventInvoked(Ta aValue, Tb aValue2);
    }
}
