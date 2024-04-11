using Data;
using UnityEngine;

namespace Examples
{
    public class SpawnPoolableObjectBehaviour : MonoBehaviour
    {
        [SerializeField] private FloatRef m_SpawnEverySecond = null;
        [SerializeField] private SOBasePoolablePool m_ObjectPool = null;

        [SerializeField] private Transform m_SpawnPosition = null;

        private float m_LastSpawnTime = 0f;

        private void Awake()
        {
            if (m_ObjectPool == null)
            {
                Debug.LogError("Object pool is null");
                return;
            }

            m_ObjectPool.Initialize();
        }

        private void Update()
        {
            if (m_SpawnEverySecond == null || m_ObjectPool == null || m_SpawnPosition == null)
                return;

            if (Time.time - m_LastSpawnTime < m_SpawnEverySecond.data)
                return;

            BasePoolableBehaviour behaviour = m_ObjectPool.Extract();
            behaviour.transform.position = m_SpawnPosition.position;

            m_LastSpawnTime = Time.time;
        }
    }
}