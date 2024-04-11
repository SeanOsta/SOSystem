using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace Events
{
    public class MonoEventListener : BaseMonoEventListener, IEventListener
    {
        [SerializeField] private SOEvent m_SOEvent = null;
        [SerializeField, Space] private UnityEvent m_UnityEvent = null;

        public void PopulateLogs(StringBuilder aBuilder)
        {
            if (m_SOEvent == null)
            {
                Debug.LogError("SO event is null! Cannot get logs");
                return;
            }

            EventLoggingUtility.PopulateUnityEventLogs(aBuilder, m_UnityEvent);
        }

        public void OnEventInvoked()
        {
            m_UnityEvent?.Invoke();
        }

        protected override void Register()
        {
            if (m_SOEvent == null)
            {
                Debug.LogError($"SOEvent is null on GameObject {name}");
                return;
            }

            m_SOEvent.Register(this);
        }

        protected override void Unregister()
        {
            if (m_SOEvent == null)
            {
                Debug.LogError($"SOEvent is null on GameObject {name}");
                return;
            }

            m_SOEvent.Unregister(this);
        }
    }

    public abstract class BaseMonoEventListener : MonoBehaviour
    {
        public enum RegisterBehaviour
        {
            EnableDisable,
            AwakeDestroy,
        }

        [SerializeField] private RegisterBehaviour m_RegisterBehaviour = RegisterBehaviour.EnableDisable;

        private void OnEnable()
        {
            if (m_RegisterBehaviour != RegisterBehaviour.EnableDisable)
                return;

            Register();
        }

        private void OnDisable()
        {
            if (m_RegisterBehaviour != RegisterBehaviour.EnableDisable)
                return;

            Unregister();
        }

        private void Awake()
        {
            if (m_RegisterBehaviour != RegisterBehaviour.AwakeDestroy)
                return;

            Register();
        }

        private void OnDestroy()
        {
            if (m_RegisterBehaviour != RegisterBehaviour.AwakeDestroy)
                return;

            Unregister();
        }

        protected abstract void Register();
        protected abstract void Unregister();
    }
}
