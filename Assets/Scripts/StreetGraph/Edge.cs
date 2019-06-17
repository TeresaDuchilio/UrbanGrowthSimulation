using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class Edge {

	public Node start;
	public Node finish;
	public int traffic;
	public List<int> cellIDs;
	public GameObject mesh;
	public Vector3 direction;
	public Block[] blocks;
	public float length;
	public float cost;
	public int capacity;

	public Vector3[] meshStart;
	public Vector3[] meshFinish;

	public int size;

	public Edge(){
		traffic = 0;
		size = 1;
		capacity = 10;
		blocks = new Block[2];
		cellIDs = new List<int>();
	}

	public Node GetNeighbor(Node start){
		if (start == this.start)
			return this.finish;
		else if (start == this.finish)
			return this.start;
		else
			return null;
	}

	public float StreetCost(){//update at the end of traffic simulation
		float freeCapacity = ((float)capacity - (float)traffic)/(float)capacity;
		
		if (freeCapacity >= 0.5)
			return length;
		else if (freeCapacity >= 0.3)
			return length + length * 0.2f;
		else if (freeCapacity > 0)
			return length + length * 0.4f;
		else
			return float.MaxValue;
	}

	public void AddBlock(Block block){
		//left and right?
		if (blocks [0] == null)
			blocks [0] = block;
		else
			blocks [1] = block;
	}
	protected abstract void FindLenght();
	public abstract Vector3 GetDirectionAtNode(Node node);
    public abstract void Grow();
}