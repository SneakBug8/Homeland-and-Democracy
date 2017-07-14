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

		GUILayout.Label("Name of Scene to load:");

		string LocField = GUILayout.TextField("");

        if (GUILayout.Button("Load Scene"))
        {
            MainController.Global.LoadAndDrawScene(LocField);
        }
    }

}

#endif

public class MainController : MonoBehaviour {

	public static MainController Global;

	void Awake() {
		Global = this;
	}
	public Text SceneText;

	public Scene CurrentScene;
	public Transform ButtonsParent;

	public GameObject ButtonPref;

	public delegate void SomeDelegate();

	public event SomeDelegate OnSceneChanged;

	XmlNode XmlRoot;

	public string nextlocact = "";

	// Use this for initialization
	void Start () {
		string SceneName;

		#if UNITY_EDITOR
		SceneName = "start";
		# else
		SceneName = SavesSystem.Global.Get("loc") ?? "start";
		# endif

		// Вызов функции инициализации (опционально)
		Functions.Global.SendMessage("init",SendMessageOptions.DontRequireReceiver);

		Debug.Log("Starting loc: " + SceneName);

		CurrentScene = LoadScene(SceneName);

		DrawScene();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void LoadAndDrawScene(string SceneName) {
		if (SceneName=="") {
			ErrorController.Global.EmptyScene();
		}

		CurrentScene = LoadScene(SceneName);
		DrawScene();
	}

	public Scene LoadScene(string SceneName) {
		return LoadScene(SceneName, false);
	}

	public Scene LoadScene(string SceneName, bool IsLocation) {
		/* Throw error if scene name not provided */
		if (SceneName=="") {
			ErrorController.Global.EmptyScene();
			return new Scene{};
		}

		/* Prepare to make scene */
		if (XmlRoot == null) {
			XmlRoot = LoadXmlFile("Scenes");
		}
		
		XmlElement SceneXmlElement = XmlRoot[SceneName];

		/* Throw error if such scene don't exist */
		if (SceneXmlElement==null) {
			ErrorController.Global.NoSuchScene(SceneName);
			return new Scene();
		}

		Scene loc = new Scene() {
			text = nextlocact + SceneXmlElement["text"].InnerText,
			name = SceneName
		};

		/* Copy Scene's Actions if Copy attribute provided. */
		if (SceneXmlElement["actions"].HasAttribute("copy")) {
			loc.actions = LoadScene(SceneXmlElement["actions"].GetAttribute("copy")).actions;
		}


		if (SceneXmlElement["function"]!=null) {
			loc.startupfunc = SceneXmlElement["function"].InnerText;
		}
		
		/* Constructing actions' list */
		loc.actions = LoadActionsList(SceneXmlElement["actions"]);
		
		return loc;
	}

	XmlNode LoadXmlFile(string ResourceName) {
			XmlDocument xDoc = new XmlDocument();

			TextAsset textAsset = (TextAsset) Resources.Load("Scenes"); 
			xDoc.LoadXml (textAsset.text);

			XmlNode FirstNode = xDoc.FirstChild;
			
			return FirstNode;
	}

	public List<Action> LoadActionsList(XmlElement actionsparent) {
		List<Action> actions = new List<Action>();

		foreach (XmlElement ActionXmlElement in actionsparent.ChildNodes) {
			Action act = LoadAction(ActionXmlElement);
			if (act != new Action{}) {
			actions.Add(act);
			}
		}
		return actions;
	}

	public Action LoadAction(XmlElement action) {
			Action act = new Action {};

			/* If _text provided - don't include action's name in next location */
			if (action["_text"]!=null) {
				act.text = action["_text"].InnerText;
				act.DoNotWriteToLoc = true;
			} else {
				act.text = action["text"].InnerText;
			}

			/* Construct ValueChange for applying variable's value on press. */
			if (action["variable"]!=null) {
				act.value = new ValueChange {
					name = action["variable"].GetAttribute("name"),
					value = action["variable"].InnerText
				};
			}

			if (action["Scene"]!=null)
				act.Scene = action["Scene"].InnerText;
			if (action["function"]!=null)
				act.function = action["function"].InnerText;
			if (action["UnityScene"]!=null)
				act.UnityScene = action["UnityScene"].InnerText;
			if (action["Location"]!=null)
				act.Location = new Vector2(
					float.Parse(action["Location"].GetAttribute("x")),
					float.Parse(action["Location"].GetAttribute("y"))
					);

			return act;
	}

	public void DrawScene(Scene loc) {
		// AdsController.Global.LocEvent();
		Debug.Log("Drawing "+loc.name);
		OnSceneChanged();

		// Deleting btns from prev loc
		foreach (Button btn in ButtonsParent.GetComponentsInChildren<Button>()) {
			Destroy(btn.gameObject);
		}

		loc.Draw();

		SceneText.text = loc.text;

		// Rendering new buttons
		int buttonid = 0;

		foreach (Action act in loc.actions) {
			if (!act.MetConditions()) {
				continue;
			}

			act.button = Instantiate(ButtonPref,ButtonsParent.transform.position,Quaternion.identity).GetComponent<Button>();

			act.button.transform.SetParent(ButtonsParent);
			act.button.transform.localScale = new Vector3(1,1,1);

			act.button.gameObject.GetComponentInChildren<Text>().text=act.text;

			act.button.gameObject.GetComponent<ActionButton>().id=buttonid;

			buttonid++;
		}
	}

	public void DrawScene() {
		DrawScene(CurrentScene);
	}

	public void OnButtonClick(int id) {
		CurrentScene.actions[id].Execute();
	}
}

public class Scene {
	public string name;
	public string text;
	public List<Action> actions = new List<Action>();
	public string startupfunc;

	public void Draw() {
		Functions.Global.SendMessage(startupfunc);
	}
}

public class Action {
	public string text;
	public string Scene;
	public string function;
	public string UnityScene;
	public Vector2 Location;

	public ValueChange value;
	
	public bool DoNotWriteToLoc = false;
	public Button button;

	public XmlElement ConditionsXmlElement;

	public bool MetConditions() {
		if (ConditionsXmlElement!=null) {
				/* Check if Player has needed items */
				if (ConditionsXmlElement["item"]!=null) {
					if (!GlobalController.Global.items.list.Contains(ConditionsXmlElement["item"].InnerText)) {
						return false;
					}
				}
				/* Check if Variable has needed value */
				if (ConditionsXmlElement["variable"]!=null) {
					if (SavesSystem.Global.Get(ConditionsXmlElement["variable"].GetAttribute("name"))!=ConditionsXmlElement["variable"].InnerText) {
						return false;
					}
				}
				/* Check if Player is in needed location */
				if (ConditionsXmlElement["location"]!=null) {
					if (GeographyController.Global.CurrentLocation.name != ConditionsXmlElement["location"].InnerText) {
						return false;
					}
				}
			}
		return true;
	}

	public void Execute() {
		if (value!=null) {
			value.Execute();
		}

		if (Location!=Vector2.zero) {
			GeographyController.Global.CurrentLocation = GeographyController.Global.Map[Location];
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

		if (Scene!=null) {
			SavesSystem.Global.Set("loc",Scene);

			MainController.Global.LoadAndDrawScene(Scene);

			return;
		}

		if (UnityScene!=null) {
			SceneManager.LoadScene(UnityScene);
		}

		ErrorController.Global.NoActionOnButton(text);
	}

	public void Remove() {
		GameObject.Destroy(button);
		MainController.Global.CurrentScene.actions.Remove(this);
	}
}

/// Class used to change some variable's value on ActionButton press
public class ValueChange {
	public string name;
	public string value;

	public void Execute() {
		SavesSystem.Global.Set(name,value);
	}
}