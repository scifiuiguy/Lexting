using SpatialGestureText.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
//using UnityEngine.UI.Mod;

namespace SpatialGestureText.LetterCube
{
    /// <summary>
    /// Enables a sibling component of type Text or InputField to receive output from the zone-texting cube.
    /// </summary>
    //[RequireComponent(typeof(InputField))]
    public class ZoneFieldOutputReceiver : MonoBehaviour
    {
        private InputField receiver;
        private void Awake()
        {
            receiver = GetComponent<InputField>();
        }

        private void OnEnable()
        {
            Debug.Log("ZoneFieldOutputReceiver on transform named " + transform.name + " about to subscribe to output manager.");     
            Lexting.printer.RegisterOutput(ref receiver);
            Debug.Log("ZoneFieldOutputReceiver on transform named " + transform.name + " just subscribed to output manager.");

            //transform.GetChild(0).GetComponent<MeshRenderer>().material = Resources.Load<Material>("Caret_MAT");

            //StartCoroutine(UnitTest_Walk_Caret());
            
        }

        private void OnDisable()
        {
            Lexting.printer?.UnregisterOutput(ref receiver);
        }

        public void OnFieldChanged()
        {
            receiver.caretPosition = Lexting.printer.CurrentOutputValue.Length;
            receiver.ForceLabelUpdate();
            //SpeedTestManager.Instance.Compare();
        }

        /// <summary>
        ///  Pulled from Unity source for InputField.cs
        ///  returns the caretPosition to jump to when user presses UpArrow
        /// </summary>

        private int LineUpCharacterPosition(int originalPos, bool goToFirstChar)
        {
            TextGenerator cachedInputTextGenerator = receiver.textComponent.cachedTextGenerator;
            if (originalPos >= cachedInputTextGenerator.characterCountVisible)
                return 0;

            UICharInfo originChar = cachedInputTextGenerator.characters[originalPos];
            int originLine = DetermineCharacterLine(originalPos, cachedInputTextGenerator);

            // We are on the last line return last character
            if (originLine - 1 < 0)
                return goToFirstChar ? 0 : originalPos;


            int endCharIdx = cachedInputTextGenerator.lines[originLine].startCharIdx - 1;

            for (int i = cachedInputTextGenerator.lines[originLine - 1].startCharIdx; i < endCharIdx; ++i)
            {
                if (cachedInputTextGenerator.characters[i].cursorPos.x >= originChar.cursorPos.x)
                    return i;
            }
            return endCharIdx;
        }

        /// <summary>
        ///  Pulled from Unity source for InputField.cs
        ///  returns the caretPosition to jump to when user presses DownArrow
        /// </summary>

        private int LineDownCharacterPosition(int originalPos, bool goToLastChar)
        {
            TextGenerator cachedInputTextGenerator = receiver.textComponent.cachedTextGenerator;
            if (originalPos >= cachedInputTextGenerator.characterCountVisible)
                return receiver.text.Length;

            UICharInfo originChar = cachedInputTextGenerator.characters[originalPos];
            int originLine = DetermineCharacterLine(originalPos, cachedInputTextGenerator);

            // We are on the last line return last character
            if (originLine + 1 >= cachedInputTextGenerator.lineCount)
                return goToLastChar ? receiver.text.Length : originalPos;

            // Need to determine end line for next line.
            int endCharIdx = GetLineEndPosition(cachedInputTextGenerator, originLine + 1);

            for (int i = cachedInputTextGenerator.lines[originLine + 1].startCharIdx; i < endCharIdx; ++i)
            {
                if (cachedInputTextGenerator.characters[i].cursorPos.x >= originChar.cursorPos.x)
                    return i;
            }
            return endCharIdx;
        }

        private int DetermineCharacterLine(int charPos, TextGenerator generator)
        {
            for (int i = 0; i < generator.lineCount - 1; ++i)
            {
                if (generator.lines[i + 1].startCharIdx > charPos)
                    return i;
            }

            return generator.lineCount - 1;
        }

        private static int GetLineEndPosition(TextGenerator gen, int line)
        {
            line = Mathf.Max(line, 0);
            if (line + 1 < gen.lines.Count)
                return gen.lines[line + 1].startCharIdx;
            return gen.characterCountVisible;
        }

        #region Unit Tests
        private IEnumerator UnitTest_Walk_Caret()
        {
            yield return new WaitForEndOfFrame();
            EventSystem.current.SetSelectedGameObject(receiver.gameObject, null);
            receiver.OnPointerClick(new PointerEventData(EventSystem.current));
            receiver.ActivateInputField();

            yield return new WaitForSeconds(1);


            receiver.caretPosition = (int)(receiver.text.Length*0.75f);
            receiver.ForceLabelUpdate();

            yield return new WaitForSeconds(3);
            
            int pos = receiver.text.Length-1;
            bool direction = true;
            while (receiver.caretPosition > 0)
            {
                yield return new WaitForSeconds(1);
                receiver.caretPosition = (direction) 
                    ? LineUpCharacterPosition(receiver.caretPosition,false)
                    : LineDownCharacterPosition(receiver.caretPosition,false);
                //direction = !direction;
                receiver.ForceLabelUpdate();
                pos--;
            }
            yield break;
        }
        #endregion Unit Tests
    }
}