using UnityEditor;
using UnityEngine;

namespace Data
{
    public class DataRefPropertyDrawer<SOType, Type> : PropertyDrawer where SOType : SOBaseData<Type>
    {
        public enum PropertyType
        { 
            Reference,
            Raw,
        }

        private PropertyType m_PropertyType = PropertyType.Reference;
        private void SetPropertyType(PropertyType aType) 
        {
            m_PropertyType = aType;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Event e = Event.current;
            if (e.type == EventType.MouseDown && e.button == 1 && position.Contains(e.mousePosition))
            {
                GenericMenu context = new GenericMenu();

                context.AddItem(new GUIContent(PropertyType.Reference.ToString()), m_PropertyType == PropertyType.Reference, () => SetPropertyType(PropertyType.Reference));
                context.AddItem(new GUIContent(PropertyType.Raw.ToString()), m_PropertyType == PropertyType.Raw, () => SetPropertyType(PropertyType.Raw));

                context.ShowAsContext();
            }

            SerializedProperty dataRef = property.FindPropertyRelative("m_DataRef");
            GUIContent propertyDisplayName = new GUIContent(property.displayName + " (Data Ref)", "Right click to switch between reference and hard values");

            if (m_PropertyType == PropertyType.Reference)
            {
                EditorGUI.ObjectField(position, dataRef, typeof(SOType), propertyDisplayName);
            }
            else if (m_PropertyType == PropertyType.Raw)
            {
                dataRef.objectReferenceValue = null;

                SerializedProperty rawRef = property.FindPropertyRelative("m_RawData");
                EditorGUI.PropertyField(position, rawRef, propertyDisplayName);
            }

        }
    }
}
