/*using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(StreetGraph))]
public class GraphInspector : Editor {
	public void OnSceneGUI () {
		StreetGraph graph = target as StreetGraph;

		for (int i = 0; i < graph.streets.Count; i++) {
		  
			Vector3 p0 = graph.streets[i].start.position;
			Vector3 p1 = graph.streets[i].finish.position;

			Handles.color = Color.white;
			Handles.DrawLine (p0, p1);

		}	
	}
}*/
