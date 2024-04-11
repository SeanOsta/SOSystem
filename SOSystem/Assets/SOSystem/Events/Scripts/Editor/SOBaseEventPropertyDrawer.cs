using Unity.Plastic.Antlr3.Runtime.Misc;
using UnityEditor;
using UnityEngine;

namespace Events
{
    [CustomEditor(typeof(SOEvent))]
    public class SOBaseEventPropertyDrawer : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Invoke Event") == false)
                return;

            if (serializedObject.targetObject is SOEvent soEvent)
                soEvent.Invoke();
        }
    }
}
