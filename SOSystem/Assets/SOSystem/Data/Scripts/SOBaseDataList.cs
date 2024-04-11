using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    public abstract class SOBaseDataList<T> : ScriptableObject, IGetData<List<T>>
    {
        public List<T> data => m_List;

        private List<T> m_List = new List<T>();

#if UNITY_EDITOR
        protected virtual void OnEnable()
        {
            UnityEditor.EditorApplication.playModeStateChanged += ClearListOnPlayStateChange;
        }

        protected virtual void OnDisable()
        {
            UnityEditor.EditorApplication.playModeStateChanged -= ClearListOnPlayStateChange;
        }

        /// <summary>
        /// Clears list on play mode change. Data lists should be runtime only
        /// </summary>
        /// <param name="aState"></param>
        protected virtual void ClearListOnPlayStateChange(UnityEditor.PlayModeStateChange aState)
        {
            //Clear list whether we just started entering playmode, or if we finished exiting
            if (aState == UnityEditor.PlayModeStateChange.EnteredPlayMode || aState == UnityEditor.PlayModeStateChange.ExitingPlayMode)
                return;

            m_List.Clear();
        }
#endif
    }
}
