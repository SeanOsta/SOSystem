using System.Collections.Generic;
using System.Diagnostics;

namespace Events
{
    public abstract class SOEventArg1<T> : SOBaseEventArgs, IEvent<T>
    {
        private List<IEventListener<T>> m_Listeners = new List<IEventListener<T>>();

        public void Invoke(T aValue)
        {
            LogEvent(aValue);

            for (int i = m_Listeners.Count - 1; i >= 0; i--)
            {
                m_Listeners[i].OnEventInvoked(aValue);
            }
        }

        public void Register(IEventListener<T> aListener)
        {
            m_Listeners.Add(aListener);
        }

        public void Unregister(IEventListener<T> aListener)
        {
            m_Listeners.Remove(aListener);
        }

        [Conditional("DEBUG_EVENTS")]
        private void LogEvent(T aValue)
        {
            System.Text.StringBuilder eventLog = EventLoggingUtility.GetEventLogs(name, m_Listeners, aValue);
            UnityEngine.Debug.Log(eventLog);
        }

#if UNITY_EDITOR
        public override void EditorInvoke(object[] aParams)
        {
            if (aParams == null || aParams.Length == 0)
            {
                UnityEngine.Debug.LogError("Parameter array is not valid! Cannot invoke from editor");
                return;
            }

            for (int i = 0; i < aParams.Length; i++)
            {
                if (aParams[i].GetType() != typeof(T))
                {
                    UnityEngine.Debug.LogError($"Parameter at index {i} is not of type {typeof(T)}! Cannot invoke from editor");
                    return;
                }
            }

            Invoke((T)aParams[0]);
        }

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
