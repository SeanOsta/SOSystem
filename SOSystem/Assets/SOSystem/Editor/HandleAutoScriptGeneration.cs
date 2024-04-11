using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Data;
using System.Text;
using System.Runtime.Serialization;

namespace SOSystem
{
    public class HandleAutoScriptGeneration : EditorWindow
    {
        private const string TITLE_MENU_LAYOUT = "SOSystem/Script Autogeneration Wizard";
        private const string AUTO_GEN_COMMENT = "This class was automatically generated, do not modify or delete this script. If you need to regenerate use the file menu wizard at: " + TITLE_MENU_LAYOUT;

        private const string TARGET_ASSEMBLY_NAME = "SOSystem";
        private const string DATA_SCRIPTS_PATH = "SOSystem/Data/Scripts/";
        private const string DATA_REF_FOLDER_PATH = DATA_SCRIPTS_PATH + "RefTypes";
        private const string DATA_EDITOR_FOLDER_PATH = DATA_SCRIPTS_PATH + "Editor/GeneratedPropDrawers";

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

        public void OnGUI()
        {
            if (GUILayout.Button("Force and overwrite regeneration of scripts") == true)
                AutoGenerateAllRequiredScripts(true);

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

        private static  string GetRefClassName(string aTypeNameForUseInClassName) => aTypeNameForUseInClassName + NAME_REFERENCE_CLASS;
        private static  string GetPropDrawClassName(string aTypeNameForUseInClassName) => aTypeNameForUseInClassName + NAME_REFERENCE_PROP_DRAW_CLASS;
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
