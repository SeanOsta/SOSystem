using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Data
{
    /// <summary>
    /// Object pool for monobehaviour objects
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SOObjectPool<T> : ScriptableObject, IObjectPool<T> where T : MonoBehaviour, IPoolable<T>
    {
        public enum SurpassPoolLimitBehaviour
        {
            IncrementSize,
            DoubleSize
        }

        private List<T> m_Pool = new List<T>();

        [SerializeField] private T m_PooledObject = default(T);
        [SerializeField, Range(0, 1000)] private int m_InitializeCount = 0;

        [SerializeField] private SurpassPoolLimitBehaviour m_SurpassLimitBehaviour = SurpassPoolLimitBehaviour.IncrementSize;

        private bool m_Initialized = false;
        private int m_RuntimePoolMax = 0;

        public void Initialize()
        {
            if (m_Initialized == true)
            {
                Debug.LogWarning($"Trying to initialize object pool {name} twice!");
                return;
            }

            if (m_PooledObject == null)
            {
                Debug.LogError($"The prefab from pool ({name}) is null!");
                return;
            }

            for (int i = 0; i < m_InitializeCount; i++)
            {
                T instance = Instantiate(m_PooledObject);
                m_Pool.Add(instance);
            }

            m_RuntimePoolMax = m_InitializeCount;
            m_Initialized = true;
        }

        public void Add(T aItem)
        {
            if (m_Initialized == false)
            {
                Debug.LogWarning($"Initialize object pool before trying to add a poolable object!");
                return;
            }

            if (aItem == null)
            {
                Debug.LogError("Trying to add null item to object pool!");
                return;
            }

            aItem.Disable();
            m_Pool.Add(aItem);
        }

        public T Extract()
        {
            if (m_Initialized == false)
            {
                Debug.LogWarning($"Initialize object pool before trying to extract a poolable object!");
                return default(T);
            }

            if (m_Pool.Count == 0)
                HandlePoolOverflow(m_SurpassLimitBehaviour);

            T extracted = m_Pool[m_Pool.Count - 1];
            m_Pool.RemoveAt(m_Pool.Count - 1);


            //Always set the return handler when extracting
            extracted.onReturnToPool = Add;
            extracted.Enable();

            return extracted;
        }

        private void HandlePoolOverflow(SurpassPoolLimitBehaviour aLimitBehaviour)
        {
            if (aLimitBehaviour == SurpassPoolLimitBehaviour.IncrementSize)
            {
                m_Pool.Add(Instantiate(m_PooledObject));
                m_RuntimePoolMax++;
            }
            else if (aLimitBehaviour == SurpassPoolLimitBehaviour.DoubleSize)
            {
                for (int i = 0; i < m_RuntimePoolMax; i++)
                {
                    m_Pool.Add(Instantiate(m_PooledObject));
                }

                m_RuntimePoolMax *= 2;
            }
        }

#if UNITY_EDITOR
        protected virtual void OnEnable()
        {
            UnityEditor.EditorApplication.playModeStateChanged += ClearPoolOnPlayStateChange;
        }

        protected virtual void OnDisable()
        {
            UnityEditor.EditorApplication.playModeStateChanged -= ClearPoolOnPlayStateChange;
        }

        protected void ClearPoolOnPlayStateChange(PlayModeStateChange aState)
        {
            //Clear pool whether we just started entering playmode, or if we finished exiting
            if (aState == UnityEditor.PlayModeStateChange.EnteredPlayMode || aState == UnityEditor.PlayModeStateChange.ExitingPlayMode)
                return;

            m_Pool.Clear();
            m_Initialized = false;
        }
#endif
    }
}
