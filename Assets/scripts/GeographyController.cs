using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class GeographyController : MonoBehaviour {

	public static GeographyController Global;

	public Dictionary<Vector2, Location> Map = new Dictionary<Vector2, Location>();
	public Location CurrentLocation {
		get {
			return CurrentLocation;
		}
		set {
			OnLocationChanged();
			CurrentLocation = value;
		}
	}

	public delegate void SomeDelegate();

	public event SomeDelegate OnLocationChanged;

	void Awake() {
		Global = this;
	}
	void Start () {
		Map = LoadLocations();
	}

	Dictionary<Vector2,Location> LoadLocations () {
		Dictionary<Vector2,Location> LocationDictionary = new Dictionary<Vector2,Location>();

			XmlDocument xDoc = new XmlDocument();

			TextAsset textAsset = (TextAsset) Resources.Load("Locations"); 
			xDoc.LoadXml (textAsset.text);

			XmlNode XmlRoot = xDoc.FirstChild;

			foreach(XmlElement locElement in XmlRoot.ChildNodes) {
				Location TempLocation = new Location {
					name = locElement["text"].InnerText,
					coordinates = new Vector2(
						float.Parse(locElement["coordinates"].GetAttribute("x")),
						float.Parse(locElement["coordinates"].GetAttribute("y"))
						),
					startupfunc = locElement["function"].InnerText,
					actions = MainController.Global.LoadActionsList(locElement["actions"])
				};

				LocationDictionary.Add(TempLocation.coordinates, TempLocation);
			}

			return LocationDictionary;
	}
}

public class Location : Scene {
	public string description;
	public Vector2 coordinates;
	public void Load() {
		Functions.Global.SendMessage(startupfunc);
	}

	/// Assigned Scene controls first text and actions, shown on Location screen.
	public void GoTo(Vector2 direction) {
		GeographyController.Global.CurrentLocation = GeographyController.Global.Map[coordinates+direction];
	}
}