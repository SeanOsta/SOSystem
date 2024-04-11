using UnityEngine;

namespace Events
{
    /// <summary>
    /// Abstract class solely used for dependency injection
    /// </summary>
    public abstract class SOBaseEvent : ScriptableObject, IEvent
    {
        public abstract void Invoke();

        public abstract void Register(IEventListener aListener);

        public abstract void Unregister(IEventListener aListener);
    }
}
