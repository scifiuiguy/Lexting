using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Events;

public struct DebugButton
{
    public DebugButton(string buttonName, bool enabled, UnityAction buttonAction)
    {
        this.buttonName = buttonName;
        this.enabled = enabled;
        OnButtonPressed = buttonAction;
    }

    public string buttonName;
    public bool enabled;
    public UnityAction OnButtonPressed;
}