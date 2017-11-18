using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor (typeof (MainController))]
public class customButton : Editor {
    // Отрисовка полей под загрузку локаций в Play mode.
    public override void OnInspectorGUI () {
        DrawDefaultInspector ();

        GUILayout.Space (15);

        GUILayout.Label ("Name of Scene to load:");

        string LocField = GUILayout.TextField ("");

        if (GUILayout.Button ("Load Scene")) {
            MainController.Global.LoadAndDrawScene (LocField);
        }
    }

}

#endif

public class MainController : MonoBehaviour {

    public static MainController Global;
    public Text SceneText;

    public Scene CurrentScene;
    public Transform ButtonsParent;

    public GameObject ButtonPref;

    public event GlobalController.EventDelegate OnSceneChanged;

    public string nextlocact = "";

    void Awake () {
        Global = this;

        GlobalController.Global.OnEarlyLoad += Init;
    }
    void Init () {
        string SceneName;

#if UNITY_EDITOR
        SceneName = "start";
#else
        SceneName = SavesSystem.Global.Get ("loc") ?? "start";
#endif

        Debug.Log ("Starting loc: " + SceneName);

        CurrentScene = LoadScene (SceneName);

        DrawScene ();
    }

    public void LoadAndDrawScene (string SceneName) {
        if (SceneName == "") {
            ErrorController.Global.EmptyScene ();
        }

        CurrentScene = LoadScene (SceneName);
        DrawScene ();
    }

    public Scene LoadScene (string SceneName) {
        return LoadScene (SceneName, false);
    }

    public Scene LoadScene (string SceneName, bool IsLocation) {
        /* Throw error if scene name not provided */
        if (SceneName == "") {
            ErrorController.Global.EmptyScene ();
            return new Scene { };
        }



        /* Prepare to make scene */
        // if (XmlRoot == null) {
            string ResourceName = (IsLocation) ? "Locations" : "Scenes";
            XmlNode XmlRoot = LoadXmlFile (ResourceName);
        // }

        XmlElement SceneXmlElement = XmlRoot[SceneName];

        /* Throw error if such scene don't exist */
        if (SceneXmlElement == null) {
            ErrorController.Global.NoSuchScene (SceneName);
            return new Scene ();
        }

        Scene loc = new Scene () {
            text = nextlocact + SceneXmlElement["text"].InnerText,
            name = SceneName
        };

        /* Copy Scene's Actions if Copy attribute provided. */
        if (SceneXmlElement["actions"].HasAttribute ("copy")) {
            loc.actions = LoadScene (SceneXmlElement["actions"].GetAttribute ("copy")).actions;
        }

        if (SceneXmlElement["function"] != null) {
            loc.startupfunc = SceneXmlElement["function"].InnerText;
        }

        /* Constructing actions' list */
        loc.actions = LoadActionsList (SceneXmlElement["actions"]);

        return loc;
    }

    public XmlNode LoadXmlFile (string ResourceName) {
        XmlDocument xDoc = new XmlDocument ();

        TextAsset textAsset = (TextAsset) Resources.Load (ResourceName);
        xDoc.LoadXml (textAsset.text);

        XmlNode FirstNode = xDoc.FirstChild;

        return FirstNode;
    }

    public List<Action> LoadActionsList (XmlElement actionsparent) {
        List<Action> actions = new List<Action> ();

        foreach (XmlElement ActionXmlElement in actionsparent.ChildNodes) {
            Action act = LoadAction (ActionXmlElement);
            if (act != new Action { }) {
                actions.Add (act);
            }
        }
        return actions;
    }

    public Action LoadAction (XmlElement action) {
        Action act = new Action {
            Scene = (action["scene"] != null) ? action["scene"].InnerText : null,
                func = (action["function"] != null) ? action["scene"].InnerText : null,
                UnityScene = (action["unityscene"] != null) ? action["unityscene"].InnerText : null,
                Location = (action["location"] != null) ? new Vector2 (float.Parse (action["location"].GetAttribute ("x")), float.Parse (action["location"].GetAttribute ("y"))) : Vector2.zero,
                text = action["text"].InnerText,
                /* If notshowonnextloc provided - don't include action's name in next location */
                DoNotWriteToLoc = (action["text"]["notshowonnextloc"] != null) ? true : false,
                /* Construct ValueChange for applying variable's value on press. */
                value = (action["variable"] != null) ? new ValueChange {
                    name = action["variable"].GetAttribute ("name"),
                        value = action["variable"].InnerText
                } : null,
                Conditions = (action["condititions"] != null) ? new ConditionValidator {
                    variables = new Func<Dictionary<string, string>> (() => {
                            Dictionary<string, string> dict = new Dictionary<string, string> ();
                            if (action["condititions"]["variables"] != null) {
                                foreach (XmlElement VariableXmlElement in action["condititions"]["variables"].ChildNodes) {
                                    dict.Add (VariableXmlElement.GetAttribute ("name"), VariableXmlElement.InnerText);
                                }
                            }
                            return dict;
                        }) (),
                        location = (action["condititions"]["location"] != null) ? action["condititions"]["location"].InnerText : null,
                        items = new Func<List<string>> (() => {
                            List<string> list = new List<string> ();
                            if (action["conditions"]["items"] != null) {
                                foreach (XmlElement ItemXmlElement in action["conditions"]["items"].ChildNodes) {
                                    list.Add (ItemXmlElement.InnerText);
                                }
                            }
                            return list;
                        }) ()
                } : null,
        };

    return act;
}

public void DrawScene (Scene loc) {
    if (loc == null) {
        return;
    }
    // AdsController.Global.LocEvent();
    Debug.Log ("Drawing " + loc.name);

    if (OnSceneChanged != null) {
        OnSceneChanged ();
    }

    // Deleting btns from prev loc
    foreach (Button btn in ButtonsParent.GetComponentsInChildren<Button> ()) {
        Destroy (btn.gameObject);
    }
    SceneText.text = DynamicText.Parse (loc.text);

    // Rendering new buttons
    int buttonid = 0;

    foreach (Action act in loc.actions) {
        if (!act.ConditionsMet ()) {
            continue;
        }

        GameObject actionbutton = Instantiate (ButtonPref, ButtonsParent.transform.position, Quaternion.identity); //.GetComponent<Button>();

        actionbutton.transform.SetParent (ButtonsParent);
        actionbutton.transform.localScale = new Vector3 (1, 1, 1);

        actionbutton.gameObject.GetComponentInChildren<Text> ().text = act.text;

        act.button = actionbutton.GetComponent<Button>();

        actionbutton.GetComponent<ActionButton>().action = act;

        buttonid++;
    }
}

public void DrawScene () {
    DrawScene (CurrentScene);
}
}

public class Scene {
    public string name;
    public string text;
    public List<Action> actions = new List<Action> ();
    public string startupfunc;

    public void Draw () {
        if (startupfunc != null) {
            Functions.Global.SendMessage (startupfunc);
        }
        MainController.Global.DrawScene (this);
    }
}

public class Action{
    public string text;
    public string Scene;
    public string func;
    public string UnityScene;
    public Vector2 Location;
    // Remake for multiple variables support
    public ValueChange value;
    public bool DoNotWriteToLoc;
    public Button button;

    public ConditionValidator Conditions;

    public bool ConditionsMet () {
        if (Conditions != null) {
            return Conditions.ConditionsMet ();
        } else {
            return true;
        }
    }

    public void Execute () {
        if (value != null) {
            value.Execute ();
        }

        if (Location != Vector2.zero) {
            GeographyController.Global.CurrentLocation = GeographyController.Global.Map[Location];
        }

        if (!DoNotWriteToLoc) {
            MainController.Global.nextlocact = text + "\n";
            if (!text.StartsWith ("-")) {
                MainController.Global.nextlocact = "> " + MainController.Global.nextlocact;
            }
        } else {
            MainController.Global.nextlocact = "";
        }

        if (func != null) {
            MainController.Global.gameObject.GetComponent<Functions> ().SendMessage (func);
        }

        if (Scene != null) {
            SavesSystem.Global.Set ("loc", Scene);

            MainController.Global.LoadScene (Scene).Draw ();

            return;
        }

        if (UnityScene != null) {
            SceneManager.LoadScene (UnityScene);

            return;
        }

        ErrorController.Global.NoActionOnButton (text);
    }

    public void Remove () {
        GameObject.Destroy (button);
        MainController.Global.CurrentScene.actions.Remove (this);
    }
}

/// Class used to change some variable's value on ActionButton press
public class ValueChange {
    public string name;
    public string value;
    public void Execute () {
        SavesSystem.Global.Set (name, value);
    }
}

public class ConditionValidator {
    public List<string> items;
    public Dictionary<string, string> variables;
    public string location;

    public bool ConditionsMet () {
        /* Check if Player has needed items */
        if (items != null) {
            foreach (string item in items) {
                if (SavesSystem.Items[item] > 0) {
                    return false;
                }
            }
        }
        /* Check if Variable has needed value */
        if (variables != null) {
            foreach (KeyValuePair<string, string> variable in variables) {
                if (SavesSystem.Global.Get (variable.Key) != variable.Value) {
                    return false;
                }
            }

        }
        /* Check if Player is in needed location */
        if (location != null) {
            if (GeographyController.Global.CurrentLocation.name != location) {
                return false;
            }
        }
        return true;
    }
}