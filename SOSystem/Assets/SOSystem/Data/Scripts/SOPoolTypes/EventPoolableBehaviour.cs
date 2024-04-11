using UnityEngine;
using UnityEngine.Events;

namespace Data
{
    /// <summary>
    /// Poolable behaviour with events for what happens when it's enabled/disabled. Flexible at the cost of some performance with Unity events
    /// </summary>
    public class EventPoolableBehaviour : BasePoolableBehaviour
    {
        [SerializeField] private UnityEvent m_Enable = null;
        [SerializeField] private UnityEvent m_Disable = null;

        public override void Enable()
        {
            m_Enable.Invoke();
        }

        public override void Disable()
        {
            m_Disable.Invoke();
        }
    }
}
