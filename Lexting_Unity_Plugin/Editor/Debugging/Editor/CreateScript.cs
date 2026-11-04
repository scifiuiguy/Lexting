using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class CreateScript : EditorWindow
{
    private static string className;
    private static string path;
    private static string templateContent;
    private static string namespaceName;
    private bool focused;

    //[MenuItem("My Commands/First Command %3")]
    //static void FirstCommand()
    //{
    //    CreateNewScriptTemplate();
    //}

    private void OnEnable()
    {
        minSize = new Vector2(400, 51);   
    }

    private void OnGUI()
    {
        if (position.size.x > 300 || position.size.y > 50)
            position = new Rect(ProjectWindow.position.position + new Vector2(ProjectWindow.position.width/2-100,70), new Vector2(400, 51));
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Class name:",GUILayout.Width(70));
        GUI.SetNextControlName("classname");
        Event e = Event.current;
        if (e.keyCode == KeyCode.Escape)
        {
            Close();
            return;
        }
        if (e.keyCode == KeyCode.Return)
        {
            CreateClass();
            Close();
            return;
        }

        className = EditorGUILayout.TextField(className,GUILayout.Width(180));
        if (!focused)
        {
            EditorGUI.FocusTextInControl("classname");
            focused = true;
        }
        
        EditorGUILayout.LabelField(".cs");
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Namespace:", GUILayout.Width(70));
        namespaceName = EditorGUILayout.TextField(namespaceName, GUILayout.Width(320));
        EditorGUILayout.EndHorizontal();
    }
    
    private static void CreateClass()
    {
        Debug.Log($"About to begin writing {className} to path: {path}");
        GenerateContent();
        RecursiveFileNameWrite(0, path, className, templateContent);
        AssetDatabase.Refresh();
    }

    [MenuItem("Assets/Create/C# Class %3",false,1)]
    private static void CreateNewScriptTemplate()
    {
        path = AssetDatabase.GetAssetPath(Selection.activeObject.GetInstanceID());
        string namespaceExtra = path.Substring(path.LastIndexOf("Scripts/") + 8);
        if (namespaceExtra.Length <= 0)
            namespaceName = EditorSettings.projectGenerationRootNamespace;
        else
        {
            string[] extraParsed = namespaceExtra.Split('/');
            namespaceName = $"{EditorSettings.projectGenerationRootNamespace}.";
            foreach (string extra in extraParsed)
                namespaceName += $"{extra}.";
            if (namespaceName.Contains(".cs"))
            {
                string[] fileNameFinder = namespaceName.Split('.');
                int cropLength = fileNameFinder[fileNameFinder.Length - 3].Length + 5;
                string fileName = fileNameFinder[fileNameFinder.Length - 3] + ".cs";
                Debug.Log($"file name: {fileName}");
                namespaceName = namespaceName.Remove(namespaceName.Length - cropLength);
            }
            else namespaceName = namespaceName.Remove(namespaceName.Length - 1);
            Debug.Log($"Full namespace: {namespaceName}");
        }  
        className = "NewScript";
        GetWindow<CreateScript>();
    }

    public static void GenerateContent()
    {
        templateContent = "using System;\n" +
            "using System.Collections;\n" +
            "using System.Collections.Generic;\n" +
            "using UnityEngine;\n" +
            "\n" +
            $"namespace {namespaceName}\n" +
            "{\n" +
            $"\tpublic class {className} : MonoBehaviour\n" +
            "\t{\n" +
            "\t\t#region vars\n" +
            "\t\t#region public vars\n" +
            "\t\t\n" +
            "\t\t#endregion public vars\n" +
            "\t\t#region private vars\n" +
            "\t\t\n" +
            "\t\t#endregion private vars\n" +
            "\t\t#endregion vars\n" +
            "\t\t\n" +
            "\t\t#region init\n" +
            "\t\tprivate void Awake()\n" +
            "\t\t{\n" +
            "\t\t\t\n" +
            "\t\t}\n" +
            "\t\t#endregion init\n" +
            "\t\n" +
            "\t\t#region loops\n" +
            "\t\t\n" +
            "\t\t#endregion loops\n" +
            "\t\t\n" +
            "\t\t#region event handlers\n" +
            "\t\t\n" +
            "\t\t#endregion event handlers\n" +
            "\t\t\n" +
            "\t\t#region helper methods\n" +
            "\t\t\n" +
            "\t\t#endregion helper methods\n" +
            "\t\t\n" +
            "\t\t#region Unit Tests\n" +
            "\t\t\n" +
            "\t\t#endregion Unit Tests\n" +
            "\t}\n" +
            "}\n";
    }

    public static void RecursiveFileNameWrite(int index, string path, string className, string templateContent)
    {
        if (!File.Exists(path))
        {
            File.WriteAllText($"{path}/" + className + ((index <= 0) ? "" : $"{index}") + ".cs", templateContent);
            Debug.Log("finished writing file named " + className + ((index <= 0) ? "" : $"{index}") + ".cs" + " to path: " + $"{path}/");
        }
        else RecursiveFileNameWrite(index++, path, className, templateContent);
    }

    public static EditorWindow GetWindowByName(string pName)
    {
        UnityEngine.Object[] objectList = Resources.FindObjectsOfTypeAll(typeof(EditorWindow));
        foreach (UnityEngine.Object obj in objectList)
        {
            if (obj.GetType().ToString() == pName)
                return ((EditorWindow)obj);
        }
        return (null);
    }

    private static EditorWindow _projectWindow = null;

    public static EditorWindow ProjectWindow
    {
        get
        {
            _projectWindow = _projectWindow ?? GetWindowByName("UnityEditor.ProjectWindow") ?? GetWindowByName("UnityEditor.ObjectBrowser") ?? GetWindowByName("UnityEditor.ProjectBrowser");
            return (_projectWindow);
        }
    }
    private static EditorWindow _hierarchyWindow = null;
    public static EditorWindow HierarchyWindow
    {
        get
        {
            _hierarchyWindow = _hierarchyWindow ?? GetWindowByName("UnityEditor.HierarchyWindow");
            return (_hierarchyWindow);
        }
    }
    
}