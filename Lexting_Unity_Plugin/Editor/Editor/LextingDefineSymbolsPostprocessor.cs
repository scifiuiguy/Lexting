using UnityEngine;
using UnityEditor;

/// <summary>
/// For each third-party SDK that provides input to Lexting, the Lexting plugin must check the project view regularly
/// to confirm whether that SDK is present so that it can disable dependencies within its own classes 
/// to prevent errors when those are not present. Lexting accomplishes this by dynamically adding and removing scripting 
/// define symbols e.g. "LEXTING_FOUND_OCULUS_SDK" or "LEXTING_FOUND_LEAP_MOTION_SDK".
/// </summary>
class LextingDefineSymbolsPostprocessor : AssetPostprocessor
{
    private static bool _oculusFound;
    private const string _oculusSearchTarget = "OVRSkeleton";
    private const string _oculusSymbol = "LEXTING_FOUND_OCULUS_SDK";

    private static bool _leapFound;
    private const string _leapSearchTarget = "LeapServiceProvider";
    private const string _leapSymbol = "LEXTING_FOUND_LEAP_SDK";

    private static string _symbolsConcatenated;
    private static string[] _symbols;

    private static int _i;
    private static string[] _results;

    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        ResetVars();
        // Quest device builds use Unity XR Hands + com.unity.xr.oculus — not embedded OVRSkeleton APIs.
        CheckAndUpdate(ref _oculusFound, _oculusSearchTarget, _oculusSymbol, BuildTargetGroup.Standalone);
        StripSymbolFromGroup(_oculusSymbol, BuildTargetGroup.Android);
        CheckAndUpdate(ref _leapFound, _leapSearchTarget, _leapSymbol, BuildTargetGroup.Android);
    }

    private static void ResetVars()
    {
        _oculusFound = false;
        _leapFound = false;
    }

    private static void CheckAndUpdate(ref bool sdkFound, string searchTarget, string symbol, BuildTargetGroup group)
    {
        _symbolsConcatenated = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
        _symbols = _symbolsConcatenated.Split(';');
        CheckForSDKPresence(ref sdkFound, symbol);
        UpdateDefineSymbols(sdkFound, searchTarget, symbol, group);
    }

    private static void StripSymbolFromGroup(string symbol, BuildTargetGroup group)
    {
        _symbolsConcatenated = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
        if (!_symbolsConcatenated.Contains(symbol))
            return;
        PlayerSettings.SetScriptingDefineSymbolsForGroup(group, GetSymbolsWithOneRemoved(symbol, _symbolsConcatenated));
    }

    private static void CheckForSDKPresence(ref bool sdkFound, string symbol)
    {
        for (_i = 0; _i < _symbols.Length; _i++)
            if (_symbols[_i].Contains(symbol))
                sdkFound = true;
    }

    private static void UpdateDefineSymbols(bool sdkFound, string searchTarget, string symbol, BuildTargetGroup group)
    {
        _symbolsConcatenated = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
        if (sdkFound)
        {
            _results = AssetDatabase.FindAssets(searchTarget);
            if (_results.Length == 0)
                PlayerSettings.SetScriptingDefineSymbolsForGroup(
                    group, GetSymbolsWithOneRemoved(symbol, _symbolsConcatenated));
        }
        else
        {
            _results = AssetDatabase.FindAssets(searchTarget);
            if (_results.Length > 0)
                PlayerSettings.SetScriptingDefineSymbolsForGroup(
                    group, _symbolsConcatenated + ";" + symbol);
        }
    }

    private static string _prefix;
    private static string _suffix;
    private static string GetSymbolsWithOneRemoved(string symbol, string symbolsConcatenated)
    {
        _prefix = symbolsConcatenated.Substring(0, symbolsConcatenated.IndexOf(symbol));
        _suffix = symbolsConcatenated.Substring(symbolsConcatenated.IndexOf(symbol));
        if (_suffix.Length == symbol.Length)
            return _prefix;
        _suffix = _suffix.Substring(symbol.Length + 1);
        return _prefix + _suffix;
    }
}
