using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class DynamicText {
    public static string Parse(string text) {
        if (text.Contains("{") && text.Contains("}")) {
            int OpeningIndex = text.IndexOf("{") + 1;
            int ClosingIndex = text.IndexOf("}");
            int RemovingLenght = ClosingIndex - OpeningIndex;

            string variablename = text.Substring(OpeningIndex,RemovingLenght);

            text = text.Replace("{"+variablename+"}",SavesSystem.Global.Get(variablename));
        }
        return text;
    }
}