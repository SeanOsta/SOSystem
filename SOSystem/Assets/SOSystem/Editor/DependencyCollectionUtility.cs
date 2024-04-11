using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SOSystem
{
    public static class DependencyCollectionUtility 
    {
        public static List<string> GetAllScenePathsInProject()
        {
            List<string> scenesInProject = new List<string>();
            string[] guids = AssetDatabase.FindAssets("t:Scene");

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                scenesInProject.Add(path);
            }

            return scenesInProject;
        }

        public static List<string> GetAllScenePathsInBuild()
        {
            List<string> scenesInBuild = new List<string>();

            int sceneCount = EditorSceneManager.sceneCountInBuildSettings;
            for (int i = 0; i < sceneCount; i++)
            {
                scenesInBuild.Add(UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i));
            }

            return scenesInBuild;
        }

        public static void PopulateSceneRootPathsToReference(UnityEngine.SceneManagement.Scene aScene, Object aReference, List<Object> aFinalPaths, bool aDeepSearch, List<GameObject> aGameObjectList, List<Component> aComponentList)
        {
            aGameObjectList.Clear();
            aScene.GetRootGameObjects(aGameObjectList);

            for (int i = 0; i < aGameObjectList.Count; i++)
            {
                GameObject rootGameObject = aGameObjectList[i];

                PopulateTransformPathsToReference(rootGameObject.transform, aReference, aFinalPaths, aDeepSearch, aComponentList);
            }
        }

        /// <summary>
        /// Construct a path to all references from the root given
        /// </summary>
        /// <param name="aRoot"></param>
        /// <param name="aReference"></param>
        /// <param name="aPrevious"></param>
        /// <param name="aFinalPaths"></param>
        /// <param name="aComponentList">Empty (non null) component list</param>
        /// <returns></returns>
        public static void PopulateTransformPathsToReference(Transform aRoot, Object aReference, List<Object> aFinalPaths, bool aDeepSearch, List<Component> aComponentList)
        {
            //Validating here handles children and root instead of just children
            if (DoesTransformContainReference(aRoot, aReference, aDeepSearch, aComponentList) == true)
                aFinalPaths.Add(aRoot.gameObject);

            int childCount = aRoot.transform.childCount;

            for (int i = 0; i < childCount; i++)
            {
                Transform child = aRoot.transform.GetChild(i);

                PopulateTransformPathsToReference(child, aReference, aFinalPaths, aDeepSearch, aComponentList);
            }
        }

        /// <summary>
        /// Looks for any reference in the gameobject
        /// </summary>
        /// <param name="aTransform"></param>
        /// <param name="aReference"></param>
        /// <param name="aComponentList">Pass in a component list for less garbage collection (can make a difference in huge searches)</param>
        /// <returns></returns>
        private static bool DoesTransformContainReference(Transform aTransform, Object aReference, bool aDeepSearch, List<Component> aComponentList)
        {
            aComponentList.Clear();
            aTransform.GetComponents(aComponentList);

            for (int i = 0; i < aComponentList.Count; i++)
            {
                if (DoesObjectContainReference(aComponentList[i], aReference, aDeepSearch) == true)
                    return true;
            }

            return false;
        }

        public static bool DoesObjectContainReference(Object aObject, Object aReference, bool aDeepSearch)
        {
            SerializedObject serializedObj = new SerializedObject(aObject);
            SerializedProperty serializedProp = serializedObj.GetIterator();

            if (aDeepSearch == false)
            {
                //Always have to go call true when just getting the iterator to go into the properties children
                if (serializedProp.NextVisible(true) == false)
                    return false;

                if (serializedProp.propertyType == SerializedPropertyType.ObjectReference && serializedProp.objectReferenceValue == aReference)
                    return true;
            }

            while (serializedProp.NextVisible(aDeepSearch))
            {
                if (serializedProp.propertyType == SerializedPropertyType.ObjectReference && serializedProp.objectReferenceValue == aReference)
                    return true;
            }

            return false;
        }
    }
}
