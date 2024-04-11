using System;
using System.Text;

namespace Events
{
    public class EventListener : IEventListener
    {
        private Action m_Action = null;

        public EventListener(Action aAction)
        {
            m_Action = aAction;
        }

        public void OnEventInvoked()
        {
            m_Action?.Invoke();
        }

        public void PopulateLogs(StringBuilder aBuilder)
        {
            if (m_Action == null)
                return;

            EventLoggingUtility.PopulateDelegateArrayLogs(aBuilder, m_Action.GetInvocationList());
        }
    }
}
