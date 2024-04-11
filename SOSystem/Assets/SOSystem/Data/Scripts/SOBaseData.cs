using Events;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace Data
{
    /// <summary>
    /// Non-Generic abstract base type to make some editor scripts easier to implement
    /// </summary>
    public abstract class SOBaseData : ScriptableObject
    {
#if UNITY_EDITOR
        //Used for in editor as a right click to save value while in playmode
        public abstract void SaveCurrentValueAsDefault();
#endif
    }

    public abstract class SOBaseData<T> : SOBaseData, IData<T>
    {
        //Before Unity 2021.1 editor only fields aren't taken into account when making builds
        //causing wrong serialized layouts, potentially causing corruption.
        //2021.1+ does not have this issue
#if UNITY_EDITOR
        private const string EVENT_SUFFIX = "_Event";
#if UNITY_2021_1_OR_NEWER
        [SerializeField]
        private bool m_DebugStackTraceOnValueChange = false;

        [SerializeField]
        private bool m_KeepPlayModeChanges = false;
#endif
#endif

        [SerializeField]
        protected T m_Data = default;

        [SerializeField, HideInInspector]
        private SOBaseEvent m_OnValueChange = null;

        /// <summary>
        /// Internal default value to revert to after exiting playmode. Prevents tons of data being registered
        /// as changed in git
        /// </summary>
        protected T m_DefaultValue = default;

        public virtual T data
        {
            get
            {
                return m_Data;
            }

            set
            {
                if (m_Data != null && m_Data.Equals(value))
                    return;

                if (m_Data == null && value == null)
                    return;

                //Avoid using a conditional method, makes stack trace a little nicer.
                //Only use in editor, not as much value in a build
#if UNITY_EDITOR
                if (m_DebugStackTraceOnValueChange == true)
                    UnityEngine.Debug.Log($"<b>[{name}]</b> was changed.\n" +
                                          $"<b>[Old Value]</b> {m_Data}\n" +
                                          $"<b>[New Value]</b> {value}\n\n" +
                                          UnityEngine.StackTraceUtility.ExtractStackTrace(), this);
#endif
                m_Data = value;

                m_OnValueChange?.Invoke();
            }
        }


#if UNITY_EDITOR
        private void UpdateEventName()
        {
            if (m_OnValueChange == null)
                return;

            m_OnValueChange.name = name + EVENT_SUFFIX;
        }

        private void OnValidate()
        {
            UpdateEventName();
        }

        protected virtual void OnEnable()
        {
            UnityEditor.EditorApplication.playModeStateChanged += ResetToDefaultOnPlayModeChange;
        }

        protected virtual void OnDisable()
        {
            UnityEditor.EditorApplication.playModeStateChanged -= ResetToDefaultOnPlayModeChange;
        }

        public override void SaveCurrentValueAsDefault()
        {
            m_DefaultValue = m_Data;
        }

        protected virtual void ResetToDefaultOnPlayModeChange(PlayModeStateChange aState)
        {
            //Reset the value to the value it was before entering playmode.
            //This is important to avoid a lot of annoying git staging issues.
            //Make sure set the data to default only when we enter edit mode,
            //since we do some editor opperations when exiting playmode
            if (aState == UnityEditor.PlayModeStateChange.ExitingEditMode)
            {
                m_DefaultValue = m_Data;
            }
            else if (aState == UnityEditor.PlayModeStateChange.EnteredEditMode)
            {
#if UNITY_2021_1_OR_NEWER
                if (m_KeepPlayModeChanges == true)
                    return;
#endif
                m_Data = m_DefaultValue;
            }
        }
#endif
    }
}
