using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SOSystem
{
    public class SearchReferencesWindow : EditorWindow
    {
        public class ObjectReferenceDisplayData
        {
            public ObjectReferenceDisplayData(string aGroupName, List<Object> aReferences)
            {
                m_GroupName = aGroupName;
                m_ReferencesFound = aReferences;
            }

            public bool m_IsFoldedOut = true;
            public string m_GroupName = null;
            public List<Object> m_ReferencesFound = null;
        }

        [System.Flags]
        public enum SearchOptions
        {
            SearchScriptableObjects = 1 << 0,
            SearchPrefabs = 1 << 1,
            SearchScenes = 1 << 2,
        }

        public enum SceneSearchOptions
        {
            ActiveScene,
            AllScenesInProject,
            AllScenesInBuild,
        }

        private const float PADDING_SEPARATOR_LIST = 10f;
        private const float PADDING_SEPARATOR_OPTIONS = 25f;

        private const float THICKNESS_SEPARATOR = 2f;

        private static Color COLOR_SEPARATOR = Color.black;

        [SerializeField] private Object m_ObjectToSearch = null;

        private List<ObjectReferenceDisplayData> m_ReferenceDisplayData = new List<ObjectReferenceDisplayData>();

        private Dictionary<SearchOptions, AnimBool> m_SearchOptionFadeGroupBool = null;
        private Dictionary<SearchOptions, bool> m_DeepSearchOptions = new Dictionary<SearchOptions, bool>()
        {
            { SearchOptions.SearchScriptableObjects, true},
            { SearchOptions.SearchPrefabs, true},
            { SearchOptions.SearchScenes, true},
        };

        Vector2 m_ReferenceDisplayScrollPosition = Vector2.zero;

        private SearchOptions m_SearchOptions = SearchOptions.SearchScriptableObjects | SearchOptions.SearchPrefabs | SearchOptions.SearchScenes;
        private SceneSearchOptions m_SceneOptions = SceneSearchOptions.AllScenesInBuild;

        private List<Scene> m_OpenedScenes = new List<Scene>();
        private Scene m_ActiveSceneBeforeSearch = default(Scene);


        GUIStyle m_HorizontalSeparator = null;
        GUIContent m_DeepSearchTitle = new GUIContent("Deep Search", "Enabling this will search nested serialized classes. This can significantly increase search times");

        public static void ShowWindow()
        {
            SearchReferencesWindow window = GetWindow<SearchReferencesWindow>(typeof(SearchReferencesWindow));

            window.SetSearchData(Selection.activeObject);
        }

        public void OnGUI()
        {
            EditorGUILayout.Space();
            m_ObjectToSearch = EditorGUILayout.ObjectField("Search References For: ", m_ObjectToSearch, typeof(Object), false);

            if (m_ObjectToSearch == null)
                return;

            GUIStyle mainTitleStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 20, clipping = TextClipping.Overflow };
            GUIStyle secondaryTitleStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 15, clipping = TextClipping.Overflow };

            DrawSeparator(THICKNESS_SEPARATOR, PADDING_SEPARATOR_OPTIONS, COLOR_SEPARATOR);

            EditorGUILayout.LabelField(new GUIContent("Search Options"), mainTitleStyle);
            EditorGUILayout.Space();

            m_SearchOptions = (SearchOptions)EditorGUILayout.EnumFlagsField(string.Empty, m_SearchOptions);

            EditorGUILayout.Space();

            DrawAllSearchOptions(secondaryTitleStyle);

            DrawSeparator(THICKNESS_SEPARATOR, PADDING_SEPARATOR_OPTIONS, COLOR_SEPARATOR);

            if (GUILayout.Button("Search") == true)
            {
                ResetAllData();
                CollectAllDependencies(m_SearchOptions);
            }

            if (m_ReferenceDisplayData.Count != 0 && GUILayout.Button("Clear") == true)
                ResetAllData();

            DrawDisplayDataList();
        }


        private void Awake()
        {
            m_HorizontalSeparator = new GUIStyle();
            m_HorizontalSeparator.normal.background = EditorGUIUtility.whiteTexture;
            m_HorizontalSeparator.margin = new RectOffset(0, 0, 4, 4);
            m_HorizontalSeparator.fixedHeight = 4;
        }

        private void OnEnable()
        {
            m_SearchOptionFadeGroupBool = new Dictionary<SearchOptions, AnimBool>()
            {
                { SearchOptions.SearchScriptableObjects, CreateDefaultAnimBool() },
                { SearchOptions.SearchPrefabs, CreateDefaultAnimBool() },
                { SearchOptions.SearchScenes, CreateDefaultAnimBool() }
            };
        }

        private void OnDisable()
        {
            ResetAllData();
        }

        private AnimBool CreateDefaultAnimBool()
        {
            AnimBool animBool = new AnimBool(true);
            animBool.valueChanged.AddListener(Repaint);

            return animBool;
        }

        private void CollectAllDependencies(SearchOptions aSearchOptions)
        {
            if (aSearchOptions.HasFlag(SearchOptions.SearchScriptableObjects) == true)
                CollectSODependencies(m_ObjectToSearch, m_ReferenceDisplayData, m_DeepSearchOptions[SearchOptions.SearchScriptableObjects]);

            if (aSearchOptions.HasFlag(SearchOptions.SearchPrefabs) == true)
                CollectPrefabDependencies(m_ObjectToSearch, m_ReferenceDisplayData, m_DeepSearchOptions[SearchOptions.SearchPrefabs]);

            if (aSearchOptions.HasFlag(SearchOptions.SearchScenes) == true)
                CollectSceneDependencies(m_SceneOptions, m_ObjectToSearch, m_ReferenceDisplayData, m_DeepSearchOptions[SearchOptions.SearchScenes]);
        }

        private void DrawAllSearchOptions(GUIStyle aHeaderStyle)
        {
            GUILayout.BeginHorizontal();

            m_SearchOptionFadeGroupBool[SearchOptions.SearchScriptableObjects].target = m_SearchOptions.HasFlag(SearchOptions.SearchScriptableObjects);
            DrawOptionsGroup(m_SearchOptionFadeGroupBool[SearchOptions.SearchScriptableObjects], "Asset Options", aHeaderStyle, () =>
            {
                m_DeepSearchOptions[SearchOptions.SearchScriptableObjects] = EditorGUILayout.Toggle(m_DeepSearchTitle, m_DeepSearchOptions[SearchOptions.SearchScriptableObjects]);
            });

            m_SearchOptionFadeGroupBool[SearchOptions.SearchPrefabs].target = m_SearchOptions.HasFlag(SearchOptions.SearchPrefabs);
            DrawOptionsGroup(m_SearchOptionFadeGroupBool[SearchOptions.SearchPrefabs], "Prefab Options", aHeaderStyle, () =>
            {
                m_DeepSearchOptions[SearchOptions.SearchPrefabs] = EditorGUILayout.Toggle(m_DeepSearchTitle, m_DeepSearchOptions[SearchOptions.SearchPrefabs]);
            });

            m_SearchOptionFadeGroupBool[SearchOptions.SearchScenes].target = m_SearchOptions.HasFlag(SearchOptions.SearchScenes);
            DrawOptionsGroup(m_SearchOptionFadeGroupBool[SearchOptions.SearchScenes], "Scene Options", aHeaderStyle, () =>
            {
                m_DeepSearchOptions[SearchOptions.SearchScenes] = EditorGUILayout.Toggle(m_DeepSearchTitle, m_DeepSearchOptions[SearchOptions.SearchScenes]);
                m_SceneOptions = (SceneSearchOptions)EditorGUILayout.EnumPopup(m_SceneOptions);
            });

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void DrawOptionsGroup(AnimBool aCanDraw, string aLabel, GUIStyle aHeaderStyle, System.Action aDrawContent)
        {
            if (aCanDraw.target == true)
            {
                GUILayout.FlexibleSpace();
                GUILayout.BeginVertical();
                if (EditorGUILayout.BeginFadeGroup(aCanDraw.faded) == true)
                {
                    EditorGUILayout.LabelField(aLabel, aHeaderStyle);
                    aDrawContent?.Invoke();
                }
                EditorGUILayout.EndFadeGroup();
                GUILayout.EndVertical();
            }
        }

        private void CollectSODependencies(Object aReferenceToSearch, List<ObjectReferenceDisplayData> aDisplayData, bool aDeepSearch)
        {
            string[] assetGUIDs = AssetDatabase.FindAssets("t:ScriptableObject");

            for (int i = 0; i < assetGUIDs.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(assetGUIDs[i]);
                Object scriptableObject = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);

                if (DependencyCollectionUtility.DoesObjectContainReference(scriptableObject, aReferenceToSearch, aDeepSearch) == false)
                    continue;

                m_ReferenceDisplayData.Add(new ObjectReferenceDisplayData("SCRIPTABLE OBJECT: " + scriptableObject.name, new List<Object>() { scriptableObject }));
            }

        }

        private void CollectPrefabDependencies(Object aReferenceToSearch, List<ObjectReferenceDisplayData> aDisplayData, bool aDeepSearch)
        {
            string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab");

            List<Component> traversedComponents = new List<Component>();
            for (int i = 0; i < prefabGUIDs.Length; i++)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(prefabGUIDs[i]));

                List<Object> referencesFound = new List<Object>();

                DependencyCollectionUtility.PopulateTransformPathsToReference(prefab.transform, aReferenceToSearch, referencesFound, aDeepSearch, traversedComponents);

                if (referencesFound.Count != 0)
                    aDisplayData.Add(new ObjectReferenceDisplayData("PREFAB: " + prefab.name, referencesFound));
            }
        }

        private void CollectSceneDependencies(SceneSearchOptions aSceneSearchOptions, Object aReferenceToSearch, List<ObjectReferenceDisplayData> aDisplayData, bool aDeepSearch)
        {
            if (UnityEditor.EditorApplication.isPlaying == true)
            {
                Debug.LogWarning("Cannot search scene dependencies while in play mode!");
                return;
            }

            List<string> scenePaths = GetAllScenePathsBasedOnOptions(aSceneSearchOptions);

            m_ActiveSceneBeforeSearch = EditorSceneManager.GetActiveScene();
            m_OpenedScenes = TryOpenAllScenesByPath(scenePaths);

            for (int i = 0; i < m_OpenedScenes.Count; i++)
            {
                Scene scene = m_OpenedScenes[i];

                List<Object> referencesFound = new List<Object>();

                DependencyCollectionUtility.PopulateSceneRootPathsToReference(scene, aReferenceToSearch, referencesFound, aDeepSearch, new List<GameObject>(), new List<Component>());

                if (referencesFound.Count != 0)
                    aDisplayData.Add(new ObjectReferenceDisplayData("SCENE: " + scene.name, referencesFound));
            }
        }

        private List<string> GetAllScenePathsBasedOnOptions(SceneSearchOptions aOptions)
        {
            switch (aOptions)
            {
                case SceneSearchOptions.ActiveScene:
                    return new List<string> { EditorSceneManager.GetActiveScene().path };
                case SceneSearchOptions.AllScenesInProject:
                    return DependencyCollectionUtility.GetAllScenePathsInProject();
                case SceneSearchOptions.AllScenesInBuild:
                    return DependencyCollectionUtility.GetAllScenePathsInBuild();
                default:
                    return new List<string>();
            }
        }

        private void DrawDisplayDataList()
        {
            if (m_ReferenceDisplayData != null && m_ReferenceDisplayData.Count != 0)
            {
                DrawSeparator(THICKNESS_SEPARATOR, PADDING_SEPARATOR_LIST, COLOR_SEPARATOR);

                m_ReferenceDisplayScrollPosition = GUILayout.BeginScrollView(m_ReferenceDisplayScrollPosition);
                for (int displayIndex = 0; displayIndex < m_ReferenceDisplayData.Count; displayIndex++)
                {
                    DrawDisplayData(m_ReferenceDisplayData[displayIndex]);
                }
                GUILayout.EndScrollView();
            }
        }

        private void DrawDisplayData(ObjectReferenceDisplayData aDisplayData)
        {
            List<Object> referencePaths = aDisplayData.m_ReferencesFound;
            if (referencePaths == null || referencePaths.Count == 0)
                return;

            aDisplayData.m_IsFoldedOut = EditorGUILayout.BeginFoldoutHeaderGroup(aDisplayData.m_IsFoldedOut, new GUIContent(aDisplayData.m_GroupName));
            if (aDisplayData.m_IsFoldedOut == true)
            {
                for (int i = 0; i < referencePaths.Count; i++)
                {
                    Object reference = referencePaths[i];

                    EditorGUILayout.ObjectField(reference.name, reference, typeof(GameObject), true);
                }
            }


            EditorGUILayout.EndFoldoutHeaderGroup();
            DrawSeparator(THICKNESS_SEPARATOR, PADDING_SEPARATOR_LIST, COLOR_SEPARATOR);
        }

        private void DrawSeparator(float aThickness, float aVerticalPadding, Color aColor, bool aRemoveHorizontalPadding = true)
        {
            Rect rect = EditorGUILayout.GetControlRect(GUILayout.Height(aVerticalPadding + aThickness));

            rect.height = aThickness;

            rect.y += aVerticalPadding / 2;

            if (aRemoveHorizontalPadding == true)
            {
                rect.x -= 3;
                rect.width += 6;
            }

            EditorGUI.DrawRect(rect, aColor);
        }


        private void ResetAllData()
        {
            m_ReferenceDisplayData.Clear();
            CloseAllOpenedScenes();
        }

        private void CloseAllOpenedScenes()
        {
            for (int i = 0; i < m_OpenedScenes.Count; i++)
            {
                Scene openedScene = m_OpenedScenes[i];

                if (openedScene.IsValid() && openedScene.isLoaded && openedScene != m_ActiveSceneBeforeSearch)
                {
                    EditorSceneManager.CloseScene(openedScene, true);
                }
            }
        }

        private List<Scene> TryOpenAllScenesByPath(List<string> aScenes)
        {
            List<Scene> openedScenes = new List<Scene>();
            for (int i = 0; i < aScenes.Count; i++)
            {
                string path = aScenes[i];
                Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                openedScenes.Add(scene);
            }

            return openedScenes;
        }

        public void SetSearchData(Object aData)
        {
            m_ObjectToSearch = aData;
        }

    }
}
