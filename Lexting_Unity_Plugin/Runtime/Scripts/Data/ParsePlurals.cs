using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

/// <summary>
/// DEPRECATED: only compatible w/ dictionary V1. V2 has different keys.
/// </summary>
public class ParsePlurals : MonoBehaviour
{
    public string sourceFileName = "";
    
    List<string> pluralsToProcess = new List<string>();
    List<string> results = new List<string>();
    string currentResult = "";

    void Start()
    {
        results.Add("{\"longerWords\": [");

        TextAsset textAsset = (TextAsset)Resources.Load(sourceFileName);
        JSONNode nodes = JSON.Parse(textAsset.text)["longerWords"];

        bool containsPlurals = false;
        foreach (JSONNode node in nodes.Values)
        {
            containsPlurals = false;
            foreach (string value in node["other"].Values)
            {
                if (value.EndsWith("s") && ContainsBothPluralAndSingular(value, node["other"]))
                {
                    containsPlurals = true;
                    pluralsToProcess.Add(value);
                }
               
            }
            if (containsPlurals)
                ProcessPlurals(node);
            else results.Add(node.ToString());

            if (node["default"].Value == "agree")
            {
                for (int i = 0; i < 10; i++)
                    Debug.Log("ParseTest: agree - " + results[results.Count-10 + i]);
            }
        }
        results.Add("]}");
    }

    private bool ContainsBothPluralAndSingular(string value, JSONNode others)
    {
        foreach (string possibleValue in others.Values)
            if (possibleValue == value.Substring(0, value.Length - 1))
                return true;
        return false;
    }
    
    private void ProcessPlurals(JSONNode node)
    {
        //if (node["default"].Value == "aggress")
        //{
        //}
        //currentResult = "{\n\"longerWords\": [\n";
        foreach (string plural in pluralsToProcess)
        {
            currentResult = "{";
            currentResult += "\"word\":" + ((node["default"].Value == null) ? "null" : "\"" + node["default"].Value + "\"") + ",";
            currentResult += "\"wordClass\":" + node["wordClass"].Value + ",";
            currentResult += "\"plural\":" + ((node["plural"].Value == null) ? "null" : "\"" + node["plural"].Value + "\"") + ",";
            currentResult += "\"past\":" + ((node["past"].Value == null) ? "null" : "\"" + node["past"].Value + "\"") + ",";
            currentResult += "\"progressive\":" + ((node["progressive"].Value == null) ? "null" : "\"" + node["progressive"].Value + "\"") + ",";
            currentResult += "\"other\":" + ((node["other"].Value == null) ? "null" : SubtractPlurals(node["other"]));
            currentResult += "}";
        }
        results.Add(currentResult);

        foreach (string plural in pluralsToProcess)
        {
            currentResult = "{";
            currentResult += "\"word\":\"" + plural.Substring(0,plural.Length-1) + "\",";
            currentResult += "\"wordClass\":1,";
            currentResult += "\"plural\":\"" + plural + "\",";
            currentResult += "\"past\":null,";
            currentResult += "\"progressive\":null,";
            currentResult += "\"other\":null";
            currentResult += "}";
            results.Add(currentResult);
        }
        
        pluralsToProcess.Clear();
    }

    private string SubtractPlurals(JSONNode others)
    {
        string result = "[";

        
        foreach (string value in others.Values)
        {
            foreach (string plural in pluralsToProcess)
                if (value != plural && value != plural.Substring(0, plural.Length - 1))
                    result += "\"" + value + "\",";
        }
        if (result.Length > 2)// remove last comma?
            result = result.Substring(0, result.Length - 1);

        return result + "]";
    }
}
