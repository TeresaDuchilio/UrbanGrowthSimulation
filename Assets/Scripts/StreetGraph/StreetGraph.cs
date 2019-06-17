using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StreetGraph{

	public List<Node> corners;
	public List<Node> expandable;
	public Material[] materials;
	public List<Edge> streets;
	private MeshFilter filter;
	public Grid grid;
	

	public StreetGraph(){
		materials = new Material[5];
		materials[0] = Resources.Load("Street", typeof(Material)) as Material;
		materials[1] = Resources.Load("Corner", typeof(Material)) as Material;
        materials[2] = Resources.Load("Size1", typeof(Material)) as Material;
        materials[3] = Resources.Load("Size2", typeof(Material)) as Material;
        materials[4] = Resources.Load("Size3", typeof(Material)) as Material;

        corners = new List<Node> ();
		streets = new List<Edge> ();
		expandable = new List<Node>();
		grid = new Grid ();

    }

	public void AddNode(Vector3 pos){
		Node tmp = new Node(pos);
		corners.Add (tmp);
		expandable.Add (tmp);
	}
	

	//addEdge to avoid repeat code?
	public void AddLine(Node start, Node finish, int weight){
		if(!start.isFull() && !finish.isFull()){
			Line temp = new Line (start, finish, weight);
			streets.Add (temp);
			
			start.addEdge(temp);
			start.neighbours.Add(finish);
			finish.addEdge (temp);
			finish.neighbours.Add(start);

		}
	}

	public void AddBezier(Node start, Node finish,Vector3 handle1,Vector3 handle2, int weight){
		if(!start.isFull() && !finish.isFull()){
			BezierCurve temp = new BezierCurve (start, finish, weight,handle1,handle2);
			streets.Add (temp);

			start.addEdge(temp);
			start.neighbours.Add(finish);
			finish.addEdge (temp);
			finish.neighbours.Add(start);
		}
	}

    public Material PickMaterial(Node node)
    {
        return materials[1];
    }

    public Material PickMaterial(Edge street, bool traffic)
    {
        if (traffic)
        {
            switch (street.size)
            {
                case 1:
                    return materials[2];
                case 2:
                    return materials[3];
                case 3:
                    return materials[4];
                default:
                    return materials[0];
            }
        }

        else
            return materials[0];
    }

    //Intersection
    public void Crossing(Node node){
		node.meshPs.Clear ();
		node.calculateMesh ();
	}

	public void Render(bool mode){
		foreach (Node n in corners) {
			CalculateNewMesh (n,mode);
		}
	}

	void CalculateNewMesh(Node node, bool traffic){

		Crossing(node);

		MeshHelper meshHelper = new MeshHelper ();
		//corner
		meshHelper.CornerMesh(node);
		if(node.mesh!=null)
			meshHelper.UpdateMesh(node.mesh,materials[1]);
		else
			node.mesh=meshHelper.CreateMesh(materials[1],"corners");

		//streets
		for(int k=0;k<node.edges.Count;k++){;
			meshHelper = new MeshHelper ();
			meshHelper.LineMesh((Line)node.edges[k]);
			if(node.edges[k].mesh!=null){
				meshHelper.UpdateMesh(node.edges[k].mesh,PickMaterial(node.edges[k],traffic));
			}
			else
				node.edges[k].mesh=meshHelper.CreateMesh(PickMaterial(node.edges[k],traffic), "streets");
		}

	}
}

