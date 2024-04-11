using System.Diagnostics;
using UnityEngine;

namespace Events
{
    /// <summary>
    /// Abstract class solely used for dependency injection and help with editor tools
    /// </summary>
    public abstract class SOBaseEventArgs : ScriptableObject
    {
        [Conditional("UNITY_EDITOR")]
        public abstract void EditorInvoke(object[] aParams);
    }
}
