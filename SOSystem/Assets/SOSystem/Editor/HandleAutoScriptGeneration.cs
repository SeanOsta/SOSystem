using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Data;
using System.Linq;

namespace SOSystem
{
    public enum TestEnum { };

    public class Trie
    {
        private readonly TrieNode m_Root;

        public Trie()
        {
            m_Root = new TrieNode(' ', 0, null);
        }

        public TrieNode GetPrefixNode(string aInput)
        {
            TrieNode currentNode = m_Root;
            TrieNode result = currentNode;

            for (int i = 0; i < aInput.Length; i++)
            {
                currentNode = currentNode.GetChild(aInput[i]);
                if (currentNode == null)
                    break;

                result = currentNode;
            }

            return result;
        }

        public void Insert(string aInput, string aDisplayName)
        {
            TrieNode startingNode = GetPrefixNode(aInput);

            if (startingNode.m_Depth == aInput.Length)
            {
                startingNode.m_DisplayNames.Add(aDisplayName);
                return;
            }

            TrieNode current = startingNode;

            for (int i = current.m_Depth; i < aInput.Length - 1; i++)
            {
                current = AddChild(aInput[i], current, null);
            }

            AddChild(aInput[aInput.Length - 1], current, aDisplayName);
        }

        private TrieNode AddChild(char aValue, TrieNode aParent, string aDisplayName)
        {
            if (aParent.m_Children.ContainsKey(aValue))
            {
                if (aDisplayName != null)
                    aParent.m_Children[aValue].m_DisplayNames.Add(aDisplayName);

                return aParent.m_Children[aValue];
            }

            TrieNode added = new TrieNode(aValue, aParent.m_Depth + 1, aParent);

            aParent.m_Children.Add(aValue, added);
            if (aDisplayName != null)
                added.m_DisplayNames.Add(aDisplayName);

            return added;
        }

        public void Delete(string aInput)
        {
            TrieNode node = GetPrefixNode(aInput);
            if (node.m_Depth != aInput.Length || node.GetChild('$') == null)
                return;

            while (node.m_Children.Count != 0)
            {
                TrieNode parent = node.m_Parent;
                parent.RemoveChild(node.m_Value);
                node = parent;
            }
        }
    }

    public class TrieNode
    {
        public readonly char m_Value;
        public readonly TrieNode m_Parent = null;
        public readonly int m_Depth;

        //Display names also doubles as info for if this node is a "real word"
        public List<string> m_DisplayNames = new List<string>();
        public Dictionary<char, TrieNode> m_Children = null;

        public TrieNode(char aValue, int aDepth, TrieNode aParent)
        {
            m_Value = aValue;
            m_Children = new Dictionary<char, TrieNode>();
            m_Parent = aParent;
            m_Depth = aDepth;
        }

        public TrieNode GetChild(char aValue)
        {
            if (m_Children.ContainsKey(aValue) == false)
                return null;

            return m_Children[aValue];
        }

        public bool RemoveChild(char aValue)
        {
            return m_Children.Remove(aValue);
        }

        public void RemoveAll()
        {
            m_Children.Clear();
        }
    }

    public interface IGatherAssemblies
    {
        List<Assembly> GatherAssemblies();
    }

    public class GatherAppDomainAssemblies : IGatherAssemblies
    {
        private string[] m_Filter = null;
        public GatherAppDomainAssemblies(string[] aFilter)
        {
            m_Filter = aFilter;
        }

        public List<Assembly> GatherAssemblies()
        {
            Assembly[] domainAssemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            List<Assembly> gatheredAssemblies = new List<Assembly>();
            for (int i = 0; i < domainAssemblies.Length; i++)
            {
                Assembly assembly = domainAssemblies[i];
                if (assembly.IsDynamic == true || DoesAssemblyNameContainFilter(assembly.FullName.ToLower(), m_Filter) == true)
                    continue;

                gatheredAssemblies.Add(assembly);
            }

            return gatheredAssemblies;
        }

        private bool DoesAssemblyNameContainFilter(string aAssemblyName, string[] aFilters)
        {
            for (int i = 0; i < aFilters.Length; i++)
            {
                if (aAssemblyName.Contains(aFilters[i].ToLower()) == true)
                    return true;
            }

            return false;
        }
    }

    public class GatherUnityAssemblies : IGatherAssemblies
    {
        private UnityEditor.Compilation.AssembliesType m_AssemblyType;
        public GatherUnityAssemblies(UnityEditor.Compilation.AssembliesType aAssemblyType)
        {
            m_AssemblyType = aAssemblyType;
        }

        public List<Assembly> GatherAssemblies()
        {
            UnityEditor.Compilation.Assembly[] unityAssemblies = UnityEditor.Compilation.CompilationPipeline.GetAssemblies(m_AssemblyType);
            List<Assembly> gatheredAssemblies = new List<Assembly>();
            for (int i = 0; i < unityAssemblies.Length; i++)
            {
                Assembly assembly = Assembly.Load(unityAssemblies[i].name);
                if (assembly.IsDynamic == true)
                    continue;

                gatheredAssemblies.Add(assembly);
            }

            return gatheredAssemblies;
        }
    }

    public class HandleAutoScriptGeneration : EditorWindow
    {
        private const string TITLE_MENU_LAYOUT = "SOSystem/Script Autogeneration Wizard";
        private const string AUTO_GEN_COMMENT = "This class was automatically generated, do not modify or delete this script. If you need to regenerate use the file menu wizard at: " + TITLE_MENU_LAYOUT;

        private const string TARGET_ASSEMBLY_NAME = "SOSystem";
        private const string DATA_SCRIPTS_PATH = "SOSystem/Data/Scripts/";
        private const string DATA_REF_FOLDER_PATH = DATA_SCRIPTS_PATH + "RefTypes";
        private const string DATA_EDITOR_FOLDER_PATH = DATA_SCRIPTS_PATH + "Editor/GeneratedPropDrawers";
        private const string DATA_TEST_FOLDER_PATH = DATA_SCRIPTS_PATH + "TEST";

        private const string DATA = "Data";
        private const string NAME_REFERENCE_CLASS = "Ref";
        private const string NAME_REFERENCE_PROP_DRAW_CLASS = NAME_REFERENCE_CLASS + "PropertyDrawer";

        private static readonly Dictionary<Type, string> m_CSharpAliasMap = new Dictionary<Type, string>
        {
            { typeof(sbyte), "sbyte" },
            { typeof(byte), "byte" },
            { typeof(short), "short" },
            { typeof(int), "int" },
            { typeof(uint), "uint" },
            { typeof(long), "long" },
            { typeof(ulong), "ulong" },
            { typeof(float), "float" },
            { typeof(double), "double" },
            { typeof(decimal), "decimal" },
            { typeof(bool), "bool" },
            { typeof(char), "char" },
            { typeof(string), "string" },
            { typeof(object), "object" },
        };

        [MenuItem(TITLE_MENU_LAYOUT)]
        public static void ShowWindow()
        {
            HandleAutoScriptGeneration window = GetWindow<HandleAutoScriptGeneration>();
        }

        private Trie m_AssemblyTypeWordSearch = null;
        private string[] m_MatchingTypesArray = null;
        private List<string> m_MatchingTypesList = new List<string>();

        private string[] m_AssemblyFilters = new string[] { "Editor" };

        private string test_thing = string.Empty;

        private void OnEnable()
        {
            IGatherAssemblies assemblies = new GatherAppDomainAssemblies(m_AssemblyFilters);
            m_AssemblyTypeWordSearch = new Trie();
            PopulateTrie(assemblies.GatherAssemblies(), m_AssemblyTypeWordSearch);

            m_MatchingTypesArray = null;
        }

        private void OnDisable()
        {
            m_AssemblyTypeWordSearch = null;
            m_MatchingTypesArray = null;
        }

        private int m_SelectedTypeIndex = -1;
        private Vector2 m_TypeScrollViewPosition = Vector2.zero;

        public void OnGUI()
        {
            Vector2 rectStart = Vector2.zero;
            Vector2 standardSize = new Vector2(position.width, 20f);

            float padding = 5f;

            string testValBefore = test_thing;
            test_thing = EditorGUI.TextField(new Rect(rectStart, standardSize), test_thing);
            rectStart.y += standardSize.y + padding;

            if (GUI.Button(new Rect(rectStart, standardSize), "Force and overwrite regeneration of scripts") == true)
                AutoGenerateAllRequiredScripts(true);

            rectStart.y += standardSize.y + padding;

            if (string.IsNullOrWhiteSpace(test_thing) == true)
                m_MatchingTypesArray = null;
            else if (testValBefore != test_thing)
            {
                m_SelectedTypeIndex = -1;
                m_TypeScrollViewPosition = Vector2.zero;

                TrieNode startingPrefix = m_AssemblyTypeWordSearch.GetPrefixNode(test_thing.ToLower());

                m_MatchingTypesList.Clear();
                PopulateAutocompleteType(startingPrefix, int.MaxValue, m_MatchingTypesList);

                m_MatchingTypesArray = m_MatchingTypesList.ToArray();
            }

            if (m_MatchingTypesArray == null)
                return;

            Vector2 itemSize = new Vector2(position.width - GUI.skin.verticalScrollbar.fixedWidth, 20f);
            Vector2 scrollViewSize = new Vector2(position.width, 250f);

            DrawItemsScrollRect(rectStart, itemSize, scrollViewSize, padding);
            SetScrollRectItemIndexChosen(rectStart, itemSize, scrollViewSize, padding);

            if (m_SelectedTypeIndex != -1)
            {
                TryCreateDataTypeFromString(m_MatchingTypesArray[m_SelectedTypeIndex]);
                m_SelectedTypeIndex = -1;
            }
        }

        private Vector2 DrawItemsScrollRect(Vector2 rectStart, Vector2 itemSize, Vector2 scrollViewSize, float padding)
        {
            if (m_MatchingTypesArray == null)
                return rectStart;

            float totalPadding = padding * (m_MatchingTypesArray.Length - 1);
            float totalButtonHeight = itemSize.y * m_MatchingTypesArray.Length;
            float totalHeight = totalPadding + totalButtonHeight;

            Vector2 viewRect = new Vector2(scrollViewSize.x - GUI.skin.verticalScrollbar.fixedWidth, totalHeight);

            Rect itemRect = new Rect(rectStart, itemSize);
            //No need for scroll rect if our total item height is smaller that the rect height
            if (totalHeight <= scrollViewSize.y)
            {
                return DrawItems(itemRect, padding, 0, m_MatchingTypesArray.Length);
            }

            m_TypeScrollViewPosition = GUI.BeginScrollView(new Rect(rectStart, scrollViewSize), m_TypeScrollViewPosition, new Rect(rectStart, viewRect));

            //Only attempt to draw the actual scrollview items that we need instead of potential thousands
            int startDrawIndex = (int)(m_TypeScrollViewPosition.y / (itemSize.y + padding));
            int maxItemsInView = (int)(scrollViewSize.y / (itemSize.y + padding));
            itemRect.y = rectStart.y + padding + (startDrawIndex * itemSize.y) + ((startDrawIndex - 1) * padding);
            Vector2 nextItemPosition = DrawItems(itemRect, padding, startDrawIndex, startDrawIndex + maxItemsInView);

            GUI.EndScrollView();

            return nextItemPosition;
        }

        private void SetScrollRectItemIndexChosen(Vector2 rectStart, Vector2 itemSize, Vector2 scrollViewSize, float padding)
        {
            if (m_MatchingTypesArray == null)
                return;

            Event e = Event.current;
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            EventType eventType = e.GetTypeForControl(controlID);
            
            if (eventType != EventType.Used || e.button != 0 || e.clickCount == 0)
                return;

            Rect scrollRect = new Rect(rectStart, scrollViewSize);
            if (scrollRect.Contains(e.mousePosition) == false)
                return;

            float mousePositionInScrollView = m_TypeScrollViewPosition.y + e.mousePosition.y - rectStart.y;
            m_SelectedTypeIndex = (int)(mousePositionInScrollView / (itemSize.y + padding));
        }

        private Vector2 DrawItems(Rect itemRect, float padding, int startIndex, int endIndex)
        {
            for (int i = startIndex; i < endIndex; i++)
            {
                GUI.Button(itemRect, m_MatchingTypesArray[i]);
                itemRect.y += itemRect.height + padding;
            }

            return itemRect.position;
        }

        private void TryCreateDataTypeFromString(string aDatatype)
        {

        }

        //private static void GenerateScriptFromDataTypeString(string aDatatype, bool aOverwrite)
        //{
        //    Type dataType = aDataAttribute.m_DataType;

        //    string typeNameForUseInClassName;
        //    if (m_CSharpAliasMap.TryGetValue(dataType, out string typeNameAsCSharpAlias) == true)
        //    {
        //        typeNameForUseInClassName = char.ToUpper(typeNameAsCSharpAlias[0]) + typeNameAsCSharpAlias.Substring(1);
        //    }
        //    else
        //    {
        //        typeNameAsCSharpAlias = dataType.FullName; //Use full name to avoid not referencing the right assembly
        //        typeNameForUseInClassName = dataType.Name; //Use only the name itself for the class
        //    }

        //    string testFolderPath = Path.Combine(UnityEngine.Application.dataPath, DATA_TEST_FOLDER_PATH);
        //    if (Directory.Exists(testFolderPath) == false)
        //        Directory.CreateDirectory(testFolderPath);

        //    TryBuildRefClass(testFolderPath, typeNameForUseInClassName, typeNameAsCSharpAlias);

        //    AssetDatabase.Refresh();
        //}

        private static void TryBuildTestClass(string aFolderPath, string aTypeNameForUseInClassName, string aTypeNameAsCSharpAlias)
        {
            //namespace Data
            //{
            //    [CreateAssetMenu(fileName = "data_Vector2", menuName = "ScriptableObjects/Data/Vector2"), SOData(typeof(Vector2))]
            //    public class SOVector2Data : SOBaseData<Vector2> { }
            //}

            string testClassName = "SO" + aTypeNameForUseInClassName + "TEST";
            string testClassfileName = GetCSFileFromName(testClassName);

            string testClassFilePath = Path.Combine(aFolderPath, testClassfileName);
            if (File.Exists(testClassFilePath) == true)
                return;

            using (FileStream stream = File.Create(testClassFilePath)) { }

            string[] usingDirectives = new string[] { "System" };
            string parentClass = $"Data{NAME_REFERENCE_CLASS}<{aTypeNameAsCSharpAlias}>";

            BuildSimpleClass(testClassFilePath, usingDirectives, DATA, AUTO_GEN_COMMENT, "Serializable", testClassName, parentClass);
        }

        private void PopulateAutocompleteType(TrieNode aPrefixNode, int aMaxDepth, List<string> aAutoCompleteSuggestions)
        {
            if (aPrefixNode.m_Depth >= aMaxDepth)
                return;

            for (int i = 0; i < aPrefixNode.m_DisplayNames.Count; i++)
            {
                aAutoCompleteSuggestions.Add(aPrefixNode.m_DisplayNames[i]);
            }

            foreach (KeyValuePair<char, TrieNode> child in aPrefixNode.m_Children)
            {
                PopulateAutocompleteType(child.Value, aMaxDepth, aAutoCompleteSuggestions);
            }
        }

        private void PopulateTrie(List<Assembly> aAssemblies, Trie aTrie)
        {
            for (int i = 0; i < aAssemblies.Count; i++)
            {
                PopulateTrie(aAssemblies[i], aTrie);
            }
        }

        private void PopulateTrie(Assembly aAssembly, Trie aTrie)
        {
            Type[] types;
            try
            {
                types = aAssembly.GetExportedTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                types = e.Types;
            }

            if (types == null)
                return;

            for (int i = 0; i < types.Length; i++)
            {
                PopulateTrie(types[i], aAssembly, aTrie);
            }
        }

        private void PopulateTrie(Type aType, Assembly aAssembly, Trie aTrie)
        {
            string assemblyName = aAssembly.GetName().Name;
            
            if (m_CSharpAliasMap.TryGetValue(aType, out string displayName) == true)
            {
                aTrie.Insert(displayName, displayName);
                return;
            }

            displayName = aType.FullName + " | (" + assemblyName + ")";

            //Even though we're technically adding multiple entries for the same one they'll only rarely
            //show up together in a search. In which case it's not the worst, we can filter that out using a hash set with the display name.
            //This hasn't been tested out in a large project, this may turn out to be a lot of memory
            aTrie.Insert(aType.Name.ToLower(), displayName);
            aTrie.Insert(aType.FullName.ToLower(), displayName);
            aTrie.Insert(assemblyName.ToLower() + "." + aType.Name.ToLower(), displayName);
            aTrie.Insert(assemblyName.ToLower() + "." + aType.FullName.ToLower(), displayName);
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void AutoGenerateScripts()
        {
            AutoGenerateAllRequiredScripts(false);
        }

        public static void AutoGenerateAllRequiredScripts(bool aOverwrite)
        {
            //Assembly[] projectAssemblies = CompilationPipeline.GetAssemblies(AssembliesType.PlayerWithoutTestAssemblies);
            List<Assembly> matchingAssemblies = GatherAssembliesMatchingName(System.AppDomain.CurrentDomain.GetAssemblies(), TARGET_ASSEMBLY_NAME);


            for (int i = 0; i < matchingAssemblies.Count; i++)
            {
                GenerateScriptsFromAssembly(matchingAssemblies[i], aOverwrite);
            }
        }

        private static void GenerateScriptsFromAssembly(Assembly aAssembly, bool aOverwrite)
        {
            Type[] assemblyTypes = aAssembly.GetTypes();

            for (int i = 0; i < assemblyTypes.Length; i++)
            {
                GenerateScriptsFromType(assemblyTypes[i], aOverwrite);
            }
        }

        private static void GenerateScriptsFromType(Type aType, bool aOverwrite)
        {
            object[] attributes = aType.GetCustomAttributes(typeof(SODataAttribute), false);
            if (attributes == null)
                return;

            for (int i = 0; i < attributes.Length; i++)
            {
                if (attributes[0] is SODataAttribute dataAttribute)
                    GenerateScriptsFromDataAttribute(dataAttribute, aOverwrite);
            }
        }

        private static void GenerateScriptsFromDataAttribute(SODataAttribute aDataAttribute, bool aOverwrite)
        {
            Type dataType = aDataAttribute.m_DataType;

            string typeNameForUseInClassName;
            if (m_CSharpAliasMap.TryGetValue(dataType, out string typeNameAsCSharpAlias) == true)
            {
                typeNameForUseInClassName = char.ToUpper(typeNameAsCSharpAlias[0]) + typeNameAsCSharpAlias.Substring(1);
            }
            else
            {
                typeNameAsCSharpAlias = dataType.FullName; //Use full name to avoid not referencing the right assembly
                typeNameForUseInClassName = dataType.Name; //Use only the name itself for the class
            }

            string refFolderPath = Path.Combine(UnityEngine.Application.dataPath, DATA_REF_FOLDER_PATH);
            if (Directory.Exists(refFolderPath) == false)
                Directory.CreateDirectory(refFolderPath);

            TryBuildRefClass(refFolderPath, typeNameForUseInClassName, typeNameAsCSharpAlias);

            string dataEditorFolderPath = Path.Combine(UnityEngine.Application.dataPath, DATA_EDITOR_FOLDER_PATH);
            if (Directory.Exists(dataEditorFolderPath) == false)
                Directory.CreateDirectory(dataEditorFolderPath);

            TryBuildRefEditorClass(dataEditorFolderPath, typeNameForUseInClassName, typeNameAsCSharpAlias, aOverwrite);

            AssetDatabase.Refresh();
        }

        private static string GetRefClassName(string aTypeNameForUseInClassName) => aTypeNameForUseInClassName + NAME_REFERENCE_CLASS;
        private static string GetPropDrawClassName(string aTypeNameForUseInClassName) => aTypeNameForUseInClassName + NAME_REFERENCE_PROP_DRAW_CLASS;
        private static string GetCSFileFromName(string aName) => aName + ".cs";

        private static void TryBuildRefEditorClass(string aFolderPath, string aTypeNameForUseInClassName, string aTypeNameAsCSharpAlias, bool aOverwrite)
        {
            string refClassName = GetRefClassName(aTypeNameForUseInClassName);
            string propDrawClassName = GetPropDrawClassName(aTypeNameForUseInClassName);
            string propDrawClassfileName = GetCSFileFromName(propDrawClassName);

            string refClassFilePath = Path.Combine(aFolderPath, propDrawClassfileName);
            if (File.Exists(refClassFilePath) == true && aOverwrite == false)
                return;

            using (FileStream stream = File.Create(refClassFilePath)) { }

            string[] usingDirectives = new string[] { "UnityEditor" };
            string attribute = $"CustomPropertyDrawer(typeof({refClassName}))";
            string parentClass = $"{DATA}{NAME_REFERENCE_PROP_DRAW_CLASS}<SOBaseData<{aTypeNameAsCSharpAlias}>, {aTypeNameAsCSharpAlias}>";

            BuildSimpleClass(refClassFilePath, usingDirectives, DATA, AUTO_GEN_COMMENT, attribute, propDrawClassName, parentClass);
        }

        private static void TryBuildRefClass(string aFolderPath, string aTypeNameForUseInClassName, string aTypeNameAsCSharpAlias)
        {
            string refClassName = aTypeNameForUseInClassName + NAME_REFERENCE_CLASS;
            string refClassfileName = GetCSFileFromName(refClassName);

            string refClassFilePath = Path.Combine(aFolderPath, refClassfileName);
            if (File.Exists(refClassFilePath) == true)
                return;

            using (FileStream stream = File.Create(refClassFilePath)) { }

            string[] usingDirectives = new string[] { "System" };
            string parentClass = $"Data{NAME_REFERENCE_CLASS}<{aTypeNameAsCSharpAlias}>";

            BuildSimpleClass(refClassFilePath, usingDirectives, DATA, AUTO_GEN_COMMENT, "Serializable", refClassName, parentClass);
        }

        private static void BuildSimpleClass(string aPath, string[] aAssembliesReferenced, string aNamspace, string aClassComment, string aAttribute, string aClassName, string aParentClass)
        {
            using (StreamWriter stream = new StreamWriter(aPath))
            {
                for (int i = 0; i < aAssembliesReferenced.Length; i++)
                {
                    stream.WriteLine($"using {aAssembliesReferenced[i]};");
                }

                string indentLength = "    ";

                stream.WriteLine();
                stream.WriteLine("namespace " + aNamspace);
                stream.WriteLine("{");

                if (string.IsNullOrEmpty(aClassComment) == false)
                    stream.WriteLine(indentLength + "//" + aClassComment);

                if (string.IsNullOrEmpty(aAttribute) == false)
                    stream.WriteLine(indentLength + "[" + aAttribute + "]");

                //Have all classes public until otherwise needed
                stream.Write(indentLength + "public class " + aClassName);

                if (string.IsNullOrEmpty(aParentClass))
                    stream.WriteLine(" { }");
                else
                    stream.WriteLine(" : " + aParentClass + " { }");

                stream.Write("}");

            }
        }

        private static List<Assembly> GatherAssembliesMatchingName(Assembly[] aAssemblies, string aName)
        {
            List<Assembly> matchingAssemblies = new List<Assembly>();
            for (int i = 0; i < aAssemblies.Length; i++)
            {
                Assembly assembly = aAssemblies[i];
                string assemblyName = assembly.GetName().Name;
                if (assemblyName == TARGET_ASSEMBLY_NAME)
                    matchingAssemblies.Add(assembly);


            }

            return matchingAssemblies;
        }
    }
}
