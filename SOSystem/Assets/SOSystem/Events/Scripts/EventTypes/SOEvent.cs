using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Events
{
    [CreateAssetMenu(fileName = "event_Void", menuName = "ScriptableObjects/Events/VoidEvent")]
    public class SOEvent : SOBaseEvent
    {
        private List<IEventListener> m_Listeners = new List<IEventListener>();

        public override void Invoke()
        {
            //Log events before so that if there's an error we still get a full stack trace
            LogEvent();

            for (int i = m_Listeners.Count - 1; i >= 0; i--)
            {
                m_Listeners[i].OnEventInvoked();
            }
        }

        public override void Register(IEventListener aListener)
        {
            m_Listeners.Add(aListener);
        }

        public override void Unregister(IEventListener aListener)
        {
            m_Listeners.Remove(aListener);
        }

        [Conditional("DEBUG_EVENTS")]
        private void LogEvent()
        {
            System.Text.StringBuilder eventLog = EventLoggingUtility.GetEventLogs(name, m_Listeners);
            UnityEngine.Debug.Log(eventLog, this);
        }

#if UNITY_EDITOR
        protected virtual void OnEnable()
        {
            UnityEditor.EditorApplication.playModeStateChanged += ClearEvent;
        }

        protected virtual void OnDisable()
        {
            UnityEditor.EditorApplication.playModeStateChanged -= ClearEvent;
        }

        private void ClearEvent(UnityEditor.PlayModeStateChange aState)
        {
            //Clear listeners whether we just started entering playmode, or if we finished exiting
            if (aState == UnityEditor.PlayModeStateChange.EnteredPlayMode || aState == UnityEditor.PlayModeStateChange.ExitingPlayMode)
                return;

            m_Listeners.Clear();
        }
#endif
    }
}
