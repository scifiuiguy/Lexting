using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

namespace SpatialGestureText
{
    public class FieldCLI : EditorWindow
    {
        private static string path;

        private void OnEnable()
        {
            minSize = new Vector2(400, 240);
        }

        private void OnGUI()
        {
            if (position.size.x > 300 || position.size.y > 300)
                position = new Rect(ProjectWindow.position.position + new Vector2(ProjectWindow.position.width / 2 - 100, 70), new Vector2(400, 240));


        }

        [MenuItem("Assets/Create/C# Field", false, 1)]
        private static void LaunchFieldCLI()
        {
            path = AssetDatabase.GetAssetPath(Selection.activeObject.GetInstanceID());
            string className = "Awesome";

            string fullPath = Application.dataPath + "\\" + path.Substring(7);
            string classOutput = "";
            ReadStringFromFile(fullPath, out classOutput);
            string addition = "public float testVar = 2f;";
            string prAddition = "private float ptestVar = 4f;";

            string puVarFrontKey = "#region public vars";
            string puVarBackKey = "#endregion public vars";
            string prVarFrontKey = "#region private vars";
            string prVarBackKey = "#endregion private vars";

            Insert(ref classOutput, addition, puVarFrontKey, puVarBackKey);
            Insert(ref classOutput, prAddition, prVarFrontKey, prVarBackKey);

            File.WriteAllText($"{path}", classOutput);
            //if (classOutput.Contains("#region public vars"))
            //{
            //    int targetStartIndex = classOutput.LastIndexOf("#region public vars") + 19;
            //    string preInsert = classOutput.Substring(0, targetStartIndex);
            //    int targetEndIndex = classOutput.LastIndexOf("#endregion public vars");
            //    string postInsert = classOutput.Substring(targetEndIndex);

            //    classOutput = preInsert + "\n\t\t" + addition + "\n\t\t" + postInsert;

            //    File.WriteAllText($"{path}", classOutput);

            //}

            GetWindow<FieldCLI>();
        }

        private static void Insert(ref string output, string insertionValue, string bookendKeyFront, string bookendKeyBack)
        {
            if (output.Contains(bookendKeyFront))
            {
                int targetStartIndex = output.LastIndexOf(bookendKeyFront) + bookendKeyFront.Length;
                string preInsert = output.Substring(0, targetStartIndex);
                if (output.Contains(bookendKeyBack))
                {
                    

                    string croppedAtFront = output.Substring(targetStartIndex);

                    // relative index of the 1st valid char inside the bookends
                    int nonWhitespaceIndex = GetFirstNonWhitespaceCharIndex(croppedAtFront);

                    int targetEndIndex = croppedAtFront.LastIndexOf(bookendKeyBack);
                    string postInsert = output.Substring(targetEndIndex);

                    string innerToKeep = croppedAtFront.Substring(nonWhitespaceIndex).Remove(targetEndIndex);

                    output = preInsert + "\n\t\t" + insertionValue + innerToKeep /*"\n\t\t"*/ + postInsert;
                }
                else Debug.LogError($"FieldCLI couldn't locate the key {bookendKeyBack} in specified file. Aborting fiedl creation...");
            }
            else Debug.LogError($"FieldCLI couldn't locate the key {bookendKeyFront} in specified file. Aborting fiedl creation...");
        }

        public static int GetFirstNonWhitespaceCharIndex(string input)
        {
            for (int i = 0; i < input.Length; i++)
                if (!char.IsWhiteSpace(input[i]))    
                    return i;
            return -1;
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

        private static void ReadStringFromFile(string path, out string output)
        {
            StreamReader reader = new StreamReader(path);
            output = reader.ReadToEnd();
            reader.Close();
        }
    }
}
