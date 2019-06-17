using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Building {
	public float height;
	public int size;
	List<Vector3> foundation;
	GameObject building;
	MeshFilter filter;
	Material material;

	public Building(List<Vector3> foundation){
		size = 1;
		height = 1f;
		this.foundation = foundation;
	}

	public void PlaceBuilding(Material material){
		this.material = material;

		if (building == null) {
			MeshHelper tmp = new MeshHelper ();
			tmp.BuildingMesh (foundation, height);
			building = tmp.CreateMesh (material, "buildings");
		} else
			UpdateBuilding ();
	}

	public void ResetBuilding(){
		size = 1;
		height = 1f;
	}

	void UpdateBuilding(){
		GameObject.Destroy (building);
		MeshHelper tmp = new MeshHelper ();
		tmp.BuildingMesh (foundation, height);
		building = tmp.CreateMesh (material, "buildings");
	}

	public void GrowBuilding(){
		size ++;

		if (size == 2)
			height = 2f;
		else
			height = 3f;

		UpdateBuilding ();

	}
}
