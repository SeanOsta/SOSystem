using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace Events
{
    public abstract class MonoArgs1EventListener<T> : BaseMonoEventListener, IEventListener<T>
    {
        [SerializeField] private SOEventArg1<T> m_SOEvent = null;
        [SerializeField] private UnityEvent<T> m_UnityEvent = null;

        public void OnEventInvoked(T aValue)
        {
            m_UnityEvent.Invoke(aValue);
        }

        public void PopulateLogs(StringBuilder aBuilder)
        {
            if (m_SOEvent == null)
            {
                Debug.LogError("SO event is null! Cannot get logs");
                return;
            }

            EventLoggingUtility.PopulateUnityEventLogs(aBuilder, m_UnityEvent);
        }

        protected override void Register()
        {
            m_SOEvent.Register(this);
        }

        protected override void Unregister()
        {
            m_SOEvent.Unregister(this);
        }
    }
}
