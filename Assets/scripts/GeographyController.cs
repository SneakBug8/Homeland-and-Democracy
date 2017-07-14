using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class GeographyController : MonoBehaviour {

    public static GeographyController Global;

    public Dictionary<Vector2, Location> Map = new Dictionary<Vector2, Location> ();

    private Location currentlocation;
    public Location CurrentLocation {
        get {
            return currentlocation;
        }
        set {
            if (OnLocationChanged != null) {
                OnLocationChanged ();
            }
            currentlocation = value;
            currentlocation.Draw ();
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
            Scene TempScene = MainController.Global.LoadScene(locElement.Name,true);

            Location TempLocation = new Location {
            name = TempScene.name,
            text = TempScene.text,
            coordinates = new Vector2 (
            int.Parse (locElement["coordinates"].GetAttribute ("x")),
            int.Parse (locElement["coordinates"].GetAttribute ("y"))
            ),
            startupfunc = TempScene.startupfunc,
            actions = TempScene.actions
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
    public void GoTo (Vector2 direction) {
        GeographyController.Global.CurrentLocation = GeographyController.Global.Map[coordinates + direction];
    }
}