using System;
using UnityEditor;
using UnityEngine;
using System.IO;

namespace AdColony.Editor {
    public class ADCMenu {
        [MenuItem("Tools/AdColony/Update AndroidManifest.xml", false, 1)]
        public static void UpdateManifest() {
            ADCManifestProcessor.Process();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog(ADCPluginInfo.Name, "AndroidManifest.xml updated.", "OK");
        }

        [MenuItem("Tools/AdColony/About")]
        public static void About() {
            EditorUtility.DisplayDialog(
                ADCPluginInfo.Name,
                "Unity plugin version " + ADCPluginInfo.Version + "\n" +
                "iOS SDK version " + ADCPluginInfo.iOSSDKVersion + "\n" +
                "Android SDK version " + ADCPluginInfo.AndroidSDKVersion,
                "OK");
        }
    }
}
