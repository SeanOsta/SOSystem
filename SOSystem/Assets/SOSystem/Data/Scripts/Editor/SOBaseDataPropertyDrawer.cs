using Events;
using UnityEditor;
using UnityEngine;

namespace Data
{
    [CustomEditor(typeof(SOBaseData), true)]
    public class SOBaseDataPropertyDrawer : Editor
    {
        private const string SET_VALUE_MESSAGE = "Set current value to Default";
        private const string SUCCESS_MESSAGE = "Save Successful!";

        private string m_ButtonDisplayMessage = SET_VALUE_MESSAGE;

        private double m_TimeSinceLastClick = 0f;
        private double m_DoubleClickThresholdInSeconds = 0.25;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            SerializedProperty serializedProp = serializedObject.FindProperty("m_OnValueChange");
            EditorGUILayout.Space();

            //While in playmode: Allow save to default value, and don't allow creating/deleting event assets
            if (UnityEditor.EditorApplication.isPlaying == true)
            {
                DrawEventButton(m_ButtonDisplayMessage, GUI.backgroundColor, (soEvent) =>
                {
                    m_ButtonDisplayMessage = SUCCESS_MESSAGE;
                    soEvent.SaveCurrentValueAsDefault();
                });

                return;
            }

            if (serializedProp == null)
                return;

            if (serializedProp.objectReferenceValue == null)
            {
                DrawEventButton("Attach on value changed event", Color.green, (soEvent) => AttachEventToSOData());
            }
            else
            {
                DrawEventButton("Double click to delete event", Color.red, (soEvent) =>
                {
                    if (EditorApplication.timeSinceStartup - m_TimeSinceLastClick <= m_DoubleClickThresholdInSeconds)
                        DeleteEventFromSOData();

                    m_TimeSinceLastClick = EditorApplication.timeSinceStartup;
                });
            }

        }

        private void DrawEventButton(string aButtonText, Color aBackgroundColor, System.Action<SOBaseData> aEventAction)
        {
            Color previousGUIColor = GUI.color;
            GUI.backgroundColor = aBackgroundColor;

            if (GUILayout.Button(aButtonText) == true)
            {
                if (serializedObject.targetObject is SOBaseData data)
                {
                    aEventAction.Invoke(data);
                }
            }

            GUI.backgroundColor = previousGUIColor;
        }

        private void AttachEventToSOData()
        {
            SerializedProperty onValueChangedProp = serializedObject.FindProperty("m_OnValueChange");
            if (onValueChangedProp.objectReferenceValue != null)
                return;

            SOEvent soEvent = ScriptableObject.CreateInstance<SOEvent>();

            //Make sure we add the asset to the data base before setting the serialized property, then save the assets
            AssetDatabase.AddObjectToAsset(soEvent, serializedObject.targetObject);

            onValueChangedProp.objectReferenceValue = soEvent;
            serializedObject.ApplyModifiedProperties();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void DeleteEventFromSOData()
        {
            SerializedProperty onValueChangedProp = serializedObject.FindProperty("m_OnValueChange");
            if (onValueChangedProp.objectReferenceValue == null)
                return;

            AssetDatabase.RemoveObjectFromAsset(onValueChangedProp.objectReferenceValue);

            DestroyImmediate(onValueChangedProp.objectReferenceValue, true);
            serializedObject.ApplyModifiedProperties();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
