using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class RandomEncounterController : MonoBehaviour {

    public static RandomEncounterController Global;

    public delegate void RandomEncounterDelegate (Location from, Location to);

    public List<RandomEncounter> RandomEncountersList = new List<RandomEncounter> ();
    // Use this for initialization
    void Awake () {
        Global = this;

        GeographyController.Global.OnLocationChanged += ExecuteEncounter;
    }

    void LoadEncounters () {
        XmlNode RootXml = MainController.Global.LoadXmlFile ("RandomEncounters");

        foreach (XmlElement encounterelement in RootXml.ChildNodes) {
            RandomEncounter tempencounter = new RandomEncounter {
            name = (encounterelement["name"] != null) ? encounterelement["name"].InnerText : null,
			func = (encounterelement["function"] != null) ? encounterelement["function"].InnerText : null,
			scene = (encounterelement["scene"] != null) ? encounterelement["scene"].InnerText : null,
			turnstillrepeat = (encounterelement["turnstillrepeat"] != null) ? int.Parse(encounterelement["turnstillrepeat"].InnerText) : 0
            };
        }
    }

    void ExecuteEncounter (Location from, Location to) {

    }
}

public class RandomEncounter {
    public string name;

    public string func;

    public string scene;

    public int turnstillrepeat;

    public int turnsremain = 0;

    public void Execute () {
        if (func != null)
            Functions.Global.SendMessage (func);
        if (scene != null)
            MainController.Global.LoadAndDrawScene (scene);

        turnsremain = turnstillrepeat;
    }
}