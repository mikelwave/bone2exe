using UnityEngine;
#if UNITY_EDITOR
using UnityEngine.SceneManagement;
using System.Collections;
#endif

public class Fps : MonoBehaviour {

#if UNITY_EDITOR
public Vector2Int pos = new Vector2Int(5,40);
	string label = "";
	static string roomName = "";
	float count;
	GUIStyle style = new GUIStyle();
	
	IEnumerator Start ()
	{
		style = new GUIStyle();
		style.alignment = TextAnchor.MiddleLeft;
		style.normal.textColor = Color.white;
		GUI.depth = 2;
		
		while (true) {
			if (Time.timeScale == 1) {
				yield return new WaitForSeconds (0.1f);
				count = (1 / Time.deltaTime);
				label = "FPS :" + (Mathf.Round (count));
			} else {
				label = "Pause";
			}
			yield return new WaitForSeconds (0.5f);
		}
	}
	public static void updateRoomName()
	{
		roomName = SceneManager.GetActiveScene().name;
	}
	
	void OnGUI ()
	{
		if(!GameMaster.DebugInfo) return;
		GUI.Label (new Rect (Screen.width-pos.x, pos.y , 100, 25), label);
		GUI.Label(new Rect(Screen.width-((roomName.Length+11)*7)-20, Screen.height-30, 200, 20), "Room name: "+ roomName,style);
		GUI.Label(new Rect(Screen.width-103, Screen.height-45, 200, 20), "Cheat level: "+ CheatInput.CheatVal.ToString("0"),style);
	}
	#endif
}