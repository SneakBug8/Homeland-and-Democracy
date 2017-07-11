using UnityEngine;
using System.Collections;

//this script used for test purpose ,it do by default 1000 logs  + 1000 warnings + 1000 errors
//so you can check the functionality of in game logs
//just drop this scrip to any empty game object on first scene your game start at
public class TestReporter : MonoBehaviour {
	
	public int logTestCount = 1000 ;
	int currentLogTestCount;
	Reporter reporter ;
	GUIStyle style ;
	// Use this for initialization
	void Start () {
		Application.runInBackground = true ;

		reporter = FindObjectOfType( typeof(Reporter)) as Reporter ;
		style = new GUIStyle();
		style.alignment = TextAnchor.MiddleCenter ;
		style.normal.textColor = Color.white ;
		style.wordWrap = true ;
		}
	
	// Update is called once per frame
	void Update () 
	{
	}
	void OnGUI()
	{
		if( reporter && !reporter.show )
		{
			if (GUI.Button(new Rect(Screen.width/2-120,5,240,60),"Show")){
				gameObject.AddComponent<ReporterGUI>();
				reporter.show=true;
			}
			}
	}
}
