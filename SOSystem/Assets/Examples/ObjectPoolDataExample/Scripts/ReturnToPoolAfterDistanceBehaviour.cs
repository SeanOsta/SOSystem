using Data;
using UnityEngine;

namespace Examples
{
    public class ReturnToPoolAfterDistanceBehaviour : EventPoolableBehaviour
    {
        [SerializeField] private SOTransformData m_Transform = null;
        [SerializeField] private FloatRef m_DistanceToReturnToPool = null;

        private void Update()
        {
            Transform positionToTestAgainst = m_Transform?.data;
            if (positionToTestAgainst == null || m_DistanceToReturnToPool == null || Vector3.Distance(transform.position, positionToTestAgainst.position) < m_DistanceToReturnToPool.data)
                return;

            ReturnToPool();
        }
    }
}