using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(MainController))]
public class customButton : Editor
{
	// Отрисовка полей под загрузку локаций в Play mode.
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

		GUILayout.Space(15);

		GUILayout.Label("Name of location to load:");

		string LocField = GUILayout.TextField("");

        if (GUILayout.Button("Load location"))
        {
            MainController.Global.ApplyLocation(LocField);
        }
    }

}

#endif

public class MainController : MonoBehaviour {

	public static MainController Global;

	void Awake() {
		Global = this;
	}
	public Text LocatonText;

	public Location curloc;
	public Transform ButtonsParent;

	public GameObject ButtonPref;

	XmlNode XmlRoot;

	public string nextlocact = "";

	// Use this for initialization
	void Start () {
		string locname;

		#if UNITY_EDITOR
		locname = "start";
		# else
		locname = SavesSystem.Global.Get("loc") ?? "start";
		# endif

		// Вызов функции инициализации (опционально)
		Functions.Global.SendMessage("init",SendMessageOptions.DontRequireReceiver);

		Debug.Log("Starting loc: " + locname);

		curloc = FindLocation(locname);

		DrawLoc();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ApplyLocation(string locname) {
		if (locname=="") {
			ErrorController.Global.EmptyLocation();
		}

		curloc = FindLocation(locname);
		DrawLoc();
	}

	public Location FindLocation(string locname) {
		if (locname=="") {
			ErrorController.Global.EmptyLocation();
			return new Location{};
		}


		if (XmlRoot == null) {
			XmlDocument xDoc = new XmlDocument();

			TextAsset textAsset = (TextAsset) Resources.Load("locations"); 
			xDoc.LoadXml (textAsset.text);

			XmlRoot = xDoc.FirstChild;
		}
		
		XmlElement locelement = XmlRoot[locname];

		if (locelement==null) {
			ErrorController.Global.NoSuchLocation(locname);
			return new Location();
		}

		Location loc = new Location() {
			text = nextlocact + locelement["text"].InnerText,
			name = locname
		};

		if (locelement["function"]!=null) {
			Functions.Global.SendMessage(locelement["function"].InnerText);
		}

		if (locelement["actions"].HasAttribute("copy")) {
			loc.actions = FindLocation(locelement["actions"].GetAttribute("copy")).actions;
		}

		foreach (XmlElement action in locelement["actions"].ChildNodes) {

			Action act = new Action {};

			if (action["_text"]!=null) {
				act.text = action["_text"].InnerText;
				act.DoNotWriteToLoc = true;
			} else {
				act.text = action["text"].InnerText;
			}

			if (action["variable"]!=null) {
				act.value = new ValueChange {
					name = action["variable"].GetAttribute("name"),
					value = action["variable"].InnerText
				};
			}

				if (action["conditions"]!=null) {
					if (action["conditions"]["item"]!=null) {
						if (!GlobalController.Global.items.list.Contains(action["conditions"]["item"].InnerText)) {
							continue;
						}
					}
					if (action["conditions"]["variable"]!=null) {
						if (SavesSystem.Global.Get(action["conditions"]["variable"].GetAttribute("name"))!=action["conditions"]["variable"].InnerText) {
							continue;
						}
					}
				}

				if (action["location"]!=null)
					act.location = action["location"].InnerText;
				if (action["function"]!=null)
					act.function = action["function"].InnerText;
				if (action["scene"]!=null)
					act.scene = action["scene"].InnerText;

			loc.actions.Add(act);
		}
        
		return loc;
	}

	public void DrawLoc(Location loc) {
		// AdsController.Global.LocEvent();
		Debug.Log("Drawing "+loc.name);
		AdsController.Global.DisplayAd();

		// Deleting btns from prev loc
		foreach (Button btn in ButtonsParent.GetComponentsInChildren<Button>()) {
			Destroy(btn.gameObject);
		}

		LocatonText.text = loc.text;

		// Rendering new buttons
		int buttonid = 0;

		foreach (Action act in loc.actions) {
			act.button = Instantiate(ButtonPref,ButtonsParent.transform.position,Quaternion.identity).GetComponent<Button>();

			act.button.transform.SetParent(ButtonsParent);
			act.button.transform.localScale = new Vector3(1,1,1);

			act.button.gameObject.GetComponentInChildren<Text>().text=act.text;

			act.button.gameObject.GetComponent<ActionButton>().id=buttonid;

			buttonid++;
		}
	}

	public void DrawLoc() {
		DrawLoc(curloc);
	}

	public void OnButtonClick(int id) {
		curloc.actions[id].Execute();
	}
}

public class Location {
	public string name;
	public string text;
	public List<Action> actions = new List<Action>();
}

public class Action {
	public string text;
	public string location;
	public string function;
	public string scene;

	public ValueChange value;
	
	public bool DoNotWriteToLoc = false;
	public Button button;

	public void Execute() {
		if (value!=null) {
			value.Execute();
		}

		if (!DoNotWriteToLoc) {
			MainController.Global.nextlocact = text + "\n";
			if (!text.StartsWith("-")) {
				MainController.Global.nextlocact = "> " + MainController.Global.nextlocact;
			}
		} else {
			MainController.Global.nextlocact = "";
		}

		if (function!=null) {
			MainController.Global.gameObject.GetComponent<Functions>().SendMessage(function);
		}

		if (location!=null) {
			SavesSystem.Global.Set("loc",location);

			MainController.Global.ApplyLocation(location);

			return;
		}

		if (scene!=null) {
			SceneManager.LoadScene(scene);
		}

		ErrorController.Global.NoActionOnButton(text);
	}

	public void Remove() {
		GameObject.Destroy(button);
		MainController.Global.curloc.actions.Remove(this);
	}
}

public class ValueChange {
	public string name;
	public string value;

	public void Execute() {
		SavesSystem.Global.Set(name,value);
	}
}