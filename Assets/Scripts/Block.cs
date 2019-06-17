using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Block {
	List<Vector3> polyPoints;
	Material[] materials;
	public Building building;
	public List <Node> nodes;
	public List<Trip> trips;
	public List <Edge> streets;
	public LandUseType lut;
	public List<int> cellIDs;
	public GameObject marker;
	public int numInhabitants;
	public int inhabitantCapacity;
    public bool historicalCenter;

	public Block(List <Node> nodes){
		this.polyPoints = new List<Vector3> ();
		this.trips = new List<Trip> ();
		this.numInhabitants = 10;
		this.cellIDs = new List<int> ();
		this.streets = new List<Edge> ();
		this.nodes = nodes;
		FindEdges ();
		FindPolyPoints ();
		building = new Building (polyPoints);
		inhabitantCapacity = 100;

		materials = new Material[4];
		materials[0] = Resources.Load("Industrial", typeof(Material)) as Material;
		materials[1] = Resources.Load("Commercial", typeof(Material)) as Material;
		materials[2] = Resources.Load("Residential", typeof(Material)) as Material;
        materials[3] = Resources.Load("Core", typeof(Material)) as Material;
    }

	public void ResetBlock(){
		numInhabitants = 10;
		inhabitantCapacity = 100;
		building.ResetBuilding ();
		UpdateBuilding ();
	}

	public void UpdateBuilding(){
		if (lut.getName () == "industrial")
			building.PlaceBuilding (materials [0]);
		else if (lut.getName () == "commercial")
			building.PlaceBuilding (materials [1]);
        else if(lut.getName()== "residential")
            building.PlaceBuilding(materials[2]);
        else
			building.PlaceBuilding (materials [3]);
	}

	public List<Block> GetNeighbors(){
		List<Block> result = new List<Block>();
		for (int i =0; i<this.streets.Count; i++) {
			if(streets[i].blocks[0] == this){
				if(streets[i].blocks[1] != null)
					result.Add(streets[i].blocks[1]);
			}
			else if(streets[i].blocks[0] != null)
				result.Add(streets[i].blocks[0]);
		}

		return result;
	}

	void FindEdges(){
		for (int i=0; i<nodes.Count-1; i++) {
			for(int k = 0; nodes[i].edges.Count >k;k++){
				if(nodes[i].edges[k].start == nodes[i+1]||nodes[i].edges[k].finish == nodes[i+1]){
					streets.Add(nodes[i].edges[k]);
					nodes[i].edges[k].AddBlock(this);
				}
			}
		}

		for(int j = 0; nodes[nodes.Count-1].edges.Count >j;j++){
			if(nodes[nodes.Count-1].edges[j].start == nodes[0]||nodes[nodes.Count-1].edges[j].finish == nodes[0]){
				streets.Add(nodes[nodes.Count-1].edges[j]);
				nodes[nodes.Count-1].edges[j].AddBlock(this);
			}
		}

	}
	

	float GetAngle(Vector3 a, Vector3 b, Vector3 point)
	{
		// Get the dot product.
		float dot = Vector3.Dot(a - point,b - point);
		
		// Get the cross product.
		float cross = Vector3.Cross (a - point, b - point).magnitude; 
		
		// Calculate the angle.
		return Mathf.Atan2(cross, dot);
	}

	bool PointInsideBlock(Vector3 point){

		int length = nodes.Count - 1;
		float angle = GetAngle (nodes [length].position, nodes [0].position,point); 

		for (int i = 0; i < length; i++)
		{
			angle += GetAngle(nodes[i].position,nodes[i+1].position,point); 
		}

		return (2 * Mathf.PI - angle < 0.000001);
	}

	void FindPolyPoints(){
		int side = 0;
		int inside = 1;
		bool start = true;

        if (PointInsideBlock(streets[0].meshStart[0]))
		   inside = 0;

		for (int i =0; i< this.streets.Count; i++) {

			//start or end?
			if(i == 0)
				start = true;
			else if(start){
				if(streets[i].finish.position == streets[i-1].start.position || streets[i].start.position == streets[i-1].finish.position)
					start = true;
				else
					start = false;
			}
			else{
				if(streets[i].finish.position == streets[i-1].finish.position || streets[i].start.position == streets[i-1].start.position)
					start = true;
				else
					start = false;
			}

			//left or right?
			if(!start)
				side = 1 - inside;
			else
				side = inside;
			
			//get points
			if(start){
				if(streets[i].meshStart.Length >2 && Vector3.Distance(streets[i].meshStart[1-side],streets[i].meshStart[2]) > 
				   									 Vector3.Distance(streets[i].meshStart[side],streets[i].meshStart[2])){
					polyPoints.Add(streets[i].meshStart[2]);
				}
				else 
					 polyPoints.Add(streets[i].meshStart[side]);
			}

			else{
				if(streets[i].meshFinish.Length >2 && Vector3.Distance(streets[i].meshFinish[1-side],streets[i].meshFinish[2]) > 
				   Vector3.Distance(streets[i].meshFinish[side],streets[i].meshFinish[2])){
					polyPoints.Add(streets[i].meshFinish[2]);
				}
				else 
					polyPoints.Add(streets[i].meshFinish[side]);
			}
		}

		//fix orientation if necessary
		float k = (polyPoints[1].z - polyPoints[0].z)*(polyPoints[2].x - polyPoints[1].x)-(polyPoints[1].x - polyPoints[0].x) * (polyPoints[2].z - polyPoints[1].z);
		
		if (k < 0)
			polyPoints.Reverse ();
	}
	
}
