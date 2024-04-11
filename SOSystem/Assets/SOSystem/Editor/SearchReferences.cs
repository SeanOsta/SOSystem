using UnityEditor;
using UnityEngine;

namespace SOSystem
{
    public static class SearchReferences 
    {
        private const string MENU_NAME = "Assets";
        private const string ITEM_NAME = "Search For Scriptable Data Asset";
        private const string FULL_PATH = MENU_NAME + "/" + ITEM_NAME;

        [MenuItem(FULL_PATH)]
        private static void SearchSOReferences()
        {
            SearchReferencesWindow.ShowWindow();
        }

        [MenuItem(FULL_PATH, validate = true)]
        private static bool ValidateSearchSOReferences()
        {
            return Selection.objects.Length == 1 && Selection.activeObject.GetType().IsSubclassOf(typeof(ScriptableObject));
        }
    }
}
