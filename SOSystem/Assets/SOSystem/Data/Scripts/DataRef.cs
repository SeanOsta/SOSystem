using System;
using UnityEngine;

namespace Data
{
    /// <summary>
    /// Serialized class to easily swap between scriptable object data references or hard variables in the inspector
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class DataRef<T> : IData<T>
    {
        [SerializeField]
        private SOBaseData<T> m_DataRef = null;

        [SerializeField]
        private T m_RawData = default(T);

        public T data
        {
            get
            {
                if (m_DataRef != null)
                    return m_DataRef.data;

                return m_RawData;
            }

            set
            {
                if (m_DataRef != null)
                {
                    m_DataRef.data = value;
                    return;
                }

                m_RawData = value;
            }
        }
    }
}
