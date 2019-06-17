using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Trip {
	public List<Edge> path;
	public float length;
	public float attractiveness;
	public int volume;

	public Trip(List<Edge> path, float length,float attractiveness){
		this.path = path;
		this.length = length;
		this.attractiveness = attractiveness;
	}
}
