using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class GeographyController : MonoBehaviour {

    public static GeographyController Global;

    public Dictionary<Vector2, Location> Map = new Dictionary<Vector2, Location> ();
    public Location CurrentLocation {
        get {
            return CurrentLocation;
        }
        set {
            if (OnLocationChanged != null) {
                OnLocationChanged ();
            }
            CurrentLocation = value;
        }
    }

    public event GlobalController.EventDelegate OnLocationChanged;

    void Awake () {
        Global = this;
    }
    void Start () {
        GlobalController.Global.OnLoad += Init;
    }
    void Init () {
        Map = LoadLocations ();

        CurrentLocation = Map[Vector2.zero];
    }

    Dictionary<Vector2, Location> LoadLocations () {
        Dictionary<Vector2, Location> LocationDictionary = new Dictionary<Vector2, Location> ();

        XmlNode XmlRoot = MainController.Global.LoadXmlFile ("Locations");

        foreach (XmlElement locElement in XmlRoot.ChildNodes) {
            Location TempLocation = new Location {
            name = locElement.GetAttribute ("name") ?? locElement.Name,
            text = locElement["text"].InnerText,
            coordinates = new Vector2 (
            int.Parse (locElement["coordinates"].GetAttribute ("x")),
            int.Parse (locElement["coordinates"].GetAttribute ("y"))
            ),
            startupfunc = (locElement["function"] != null) ? locElement["function"].InnerText : null,
            actions = MainController.Global.LoadActionsList (locElement["actions"])
            };

            if (locElement["function"] != null)

                Debug.Log (TempLocation.coordinates.x + "| " + TempLocation.coordinates.y);

            LocationDictionary.Add (TempLocation.coordinates, TempLocation);
        }

        return LocationDictionary;
    }
}

public class Location : Scene {
    public Vector2 coordinates;
    public void Load () {
        Functions.Global.SendMessage (startupfunc);
    }

    /// Assigned Scene controls first text and actions, shown on Location screen.
    public void GoTo (Vector2 direction) {
        GeographyController.Global.CurrentLocation = GeographyController.Global.Map[coordinates + direction];
    }
}