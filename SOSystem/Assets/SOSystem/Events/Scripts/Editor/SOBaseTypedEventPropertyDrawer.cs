using UnityEditor;
using UnityEngine;

namespace Events
{
    [CustomEditor(typeof(SOBaseEventArgs), true)]
    public class SOBaseTypedEventPropertyDrawer : Editor
    {
        private const string TITLE_INVOKE_DATA_FIELD = "Invoke using: ";
        private object m_InvokeEventWithData = null;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            System.Action invokeEvent = DrawEventDataField(serializedObject.targetObject);

            if (GUILayout.Button("Invoke Event") == true)
                invokeEvent?.Invoke();

        }

        /// <summary>
        /// Invoke the typed event with a paramater. TODO: Is there any better way to do this?
        /// </summary>
        /// <param name="aTarget"></param>
        /// <param name="aTypes"></param>
        private System.Action DrawEventDataField(Object aTarget)
        {
            if (aTarget is SOFloatEvent soFloatEvent)
            {
                InitializeToDefaultIfNull<float>();

                m_InvokeEventWithData = EditorGUILayout.FloatField(TITLE_INVOKE_DATA_FIELD, (float)m_InvokeEventWithData);
                return () => soFloatEvent.Invoke((float)m_InvokeEventWithData);
            }
            else if (aTarget is SOIntEvent soIntEvent)
            {
                InitializeToDefaultIfNull<int>();

                m_InvokeEventWithData = EditorGUILayout.IntField(TITLE_INVOKE_DATA_FIELD, (int)m_InvokeEventWithData);
                return () => soIntEvent.Invoke((int)m_InvokeEventWithData);
            }
            else if (aTarget is SOBoolEvent soBoolEvent)
            {
                InitializeToDefaultIfNull<bool>();

                m_InvokeEventWithData = EditorGUILayout.Toggle(TITLE_INVOKE_DATA_FIELD, (bool)m_InvokeEventWithData);
                return () => soBoolEvent.Invoke((bool)m_InvokeEventWithData);
            }
            else if (aTarget is SOByteEvent soByteEvent)
            {
                InitializeToDefaultIfNull<byte>();

                m_InvokeEventWithData = EditorGUILayout.FloatField(TITLE_INVOKE_DATA_FIELD, (byte)m_InvokeEventWithData);
                return () => soByteEvent.Invoke((byte)m_InvokeEventWithData);
            }
            else if (aTarget is SOStringEvent soStringEvent)
            {
                InitializeToDefaultIfNull<string>();

                m_InvokeEventWithData = EditorGUILayout.TextField(TITLE_INVOKE_DATA_FIELD, (string)m_InvokeEventWithData);
                return () => soStringEvent.Invoke((string)m_InvokeEventWithData);
            }
            else if (aTarget is SOColorEvent soColorEvent)
            {
                InitializeToDefaultIfNull<Color>();

                m_InvokeEventWithData = EditorGUILayout.ColorField(TITLE_INVOKE_DATA_FIELD, (Color)m_InvokeEventWithData);
                return () => soColorEvent.Invoke((Color)m_InvokeEventWithData);
            }
            else if (aTarget is SOVector2Event soVector2Event)
            {
                InitializeToDefaultIfNull<Vector2>();

                m_InvokeEventWithData = EditorGUILayout.Vector2Field(TITLE_INVOKE_DATA_FIELD, (Vector2)m_InvokeEventWithData);
                return () => soVector2Event.Invoke((Vector2)m_InvokeEventWithData);
            }
            else if (aTarget is SOVector3Event soVector3Event)
            {
                InitializeToDefaultIfNull<Vector3>();

                m_InvokeEventWithData = EditorGUILayout.Vector3Field(TITLE_INVOKE_DATA_FIELD, (Vector3)m_InvokeEventWithData);
                return () => soVector3Event.Invoke((Vector3)m_InvokeEventWithData);
            }
            else if (aTarget is SOTransformEvent soTransformEvent)
            {
                InitializeToDefaultIfNull<Transform>();

                m_InvokeEventWithData = EditorGUILayout.ObjectField(TITLE_INVOKE_DATA_FIELD, (Object)m_InvokeEventWithData, typeof(Transform), true);
                return () => soTransformEvent.Invoke((Transform)m_InvokeEventWithData);
            }

            Debug.LogWarning("Cannot find associated type of event!");
            return null;
        }

        private void InitializeToDefaultIfNull<T>()
        {
            if (m_InvokeEventWithData == null)
                m_InvokeEventWithData = default(T);
        }
    }
}
