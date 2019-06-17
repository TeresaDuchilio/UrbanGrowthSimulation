using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Node {

	public List<Edge> edges;
	public List<Node> neighbours;
	public Vector3 position;
	public List <Vector3> meshPs;
	public GameObject mesh;
	public int strike;
	public float tDistance;
	public bool visited;

	Vector3 tmps,tmpf;

	public Node(Vector3 pos){
		this.edges = new List<Edge>(4);
		this.neighbours = new List<Node>(4);
		this.position = pos;
		meshPs = new List<Vector3>();
		strike = 0;
	}

	public bool isFull(){
		if (edges.Count == 4)
			return true;
		else
			return false;
	}
	public int getTrafficValue(){
        int value = 0;
        int c = 0;
		foreach (Edge edge in this.edges) {
            value += edge.traffic;
		}
        return value;
	}
	public float getCapacity(){
		float value = 0;
		foreach (Edge edge in this.edges) {
			value += edge.capacity;
		}
		return value;
	}
	public Vector3 edgeDirection(Edge edge){
		if (edge is BezierCurve) 
			return ((BezierCurve)edge).GetDirectionAtNode (this);
		else 
			return ((Line)edge).GetDirectionAtNode(this);
	}

	public void addEdge(Edge edge){

		int i = this.edges.Count;
		//Insert in order
		if (i != 0) {
			Vector3 d1,d2; //directions
			int index = i;
			//float angle = 360f;
			while(i>0){


				d1 = edge.GetDirectionAtNode(this); //everything is a mess :(
				d2 = edges[i-1].GetDirectionAtNode(this);
				float temp =Vector3.Cross(d1, d2).y;
				//if(Vector3.Cross(d1, d2).y > 0){//mit epsilon
				if(Mathf.Abs(temp) > 0.001f && !(temp <= (-0.001f))){ //prolem because of start/end
					index = i-1;
				}

				i--;
			}
			edges.Insert(index,edge);
		}
			
		else 
			edges.Add(edge);
	}

	public void calculateMesh(){

		int count = this.edges.Count;
		if(count > 1){

			for (int current = 0; current < count; current++) { //for each edge
				int left=0;
				int right=0; // intersecting edges
				int side1,side2;
				Vector3[] points = new Vector3[3];
				Vector3[] temp = new Vector3[2];

				if(count == 2) //two edges
				{
					side1 = edges[current].start==this ? current:1-current;
					side2 = edges[1-current].start==this ? 1-current:current;
					points =  meshPoints(current,1-current,side1,side2,true);
				}
				else{
					if(current == count-1)
						left = 0;
					else
						left = current+1;
					
					if(current == 0)
						right = count-1;
					else
						right = current-1;

	     			//which angle is smaller
					if(Vector3.Angle(edgeDirection(edges[current]),edgeDirection(edges[left]))<=
					   Vector3.Angle(edgeDirection(edges[current]),edgeDirection(edges[right]))){
						side1 = edges[current].start==this ? 0:1;
						side2 = edges[left].start==this? 1:0;


						// n1 dann n2
						temp = meshPoints(current,left,side1,side2,true);
						points [0] = temp [0];
						points [1] = temp [1];

						side1 = 1-side1;
						side2 = edges[right].start==this? 0:1;
						temp = meshPoints(current,right,side1,side2,false);

						if(edges[current].start==this)
							points [2] = temp [1];
						else
							points [2] = temp [0];
						
					}
					else{

						side1 = edges[current].start==this ? 1:0;
						side2 = edges[right].start==this? 0:1;


						//n2 dann n1						
						temp = meshPoints(current,right,side1,side2,true);
						points [0] = temp [0];
						points [1] = temp [1];

						side1 = 1-side1;
						side2 = edges[left].start==this? 1:0;

						temp =  meshPoints(current,left,side1,side2,false);
						if(edges[current].start==this)
							points [2] = temp [0];
						else
							points [2] = temp [1];
					}
                    
					if(!containsVector(points[1-side1]))
						this.meshPs.Add (points[1-side1]);

					if(!containsVector(points[2]))
						this.meshPs.Add (points[2]);

				}
				
				if(this.edges[current].start != this)
					(this.edges[current]).meshFinish = points;
				else
					(this.edges[current]).meshStart = points;

			}

			//correct orientation if necessary
			if(meshPs.Count >=3){
				float k = (meshPs[1].z - meshPs[0].z)*(meshPs[2].x - meshPs[1].x)-(meshPs[1].x - meshPs[0].x) * (meshPs[2].z - meshPs[1].z);

				if(k > 0){
					//swap
					Vector3 temp = meshPs[0];
					meshPs[0] = meshPs[1];
					meshPs[1] = temp;
				}
			}
		}

	}

	public bool containsVector(Vector3 x){
		foreach (Vector3 v in meshPs)
		{
			if (v == x)
				return true;
		}
		return false;
	}

	public Vector3 intersection (Vector3 s1, Vector3 f1, Vector3 s2, Vector3 f2){
		// Get A,B,C of first line
		float A1 = f1.z - s1.z;
		float B1 = s1.x - f1.x;
		float C1 = A1 * s1.x + B1 * s1.z;
		
		// Get A,B,C of second line
		float A2 = f2.z - s2.z;
		float B2 = s2.x - f2.x;
		float C2 = A2 * s2.x + B2 * s2.z;
		
		// Get delta and check if the lines are parallel
		float d = A1 * B2 - A2 * B1;

		if (d <= 0.001f && d >= -0.001f)
			throw new System.Exception ("Lines are parallel");
		
		return new Vector3 ((B2 * C1 - B1 * C2)/d, 0.0f, (A1 * C2 - A2 * C1)/d);
	}
	
	
	
	public Vector3[] meshPoints(int current, int intersect, int side1, int side2,bool update){
		//calculate intersection
		Vector3[] point = new Vector3[2];
		Vector3[] lineVs1 = new Vector3[2];
		Vector3[] lineVs2 = new Vector3[2];
		bool s1 = (this.edges[current].start == this);
		bool s2 = (this.edges[intersect].start == this);
		Vector3 tmp = new Vector3();

		if (this.edges [current] is Line)
		if (this.edges [intersect] is Line)
			tmp = meshPoints (((Line)this.edges [current]), ((Line)this.edges [intersect]), side1, side2);
		else
			tmp = meshPoints (((Line)this.edges [current]), ((BezierCurve)this.edges [intersect]), side1, side2);
		else
			if (this.edges [intersect] is Line)
			tmp = meshPoints (((BezierCurve)this.edges [current]), ((Line)this.edges [intersect]).startVs [side2], ((Line)this.edges [intersect]).finishVs [side2], side1, update);
		else
			tmp = meshPoints (((BezierCurve)this.edges [current]), ((BezierCurve)this.edges [intersect]), side1, side2);


		if (tmp == Vector3.up) {
			if(s1){
				tmp = tmps;
			}
			else{
				tmp = tmpf;
			}
		}


		float tmpt = 0f;
		if (this.edges [current] is BezierCurve) {
			if(s1)
				tmpt = (float)((BezierCurve)this.edges[current]).startSegment / ((BezierCurve)this.edges[current]).size;
			else
				tmpt = (float)((BezierCurve)this.edges[current]).endSegment / ((BezierCurve)this.edges[current]).size;

		}

		if (side1==1) {
			point[1] = tmp;
			if (this.edges [current] is Line)
				point [0] = tmp + ((Line)this.edges [current]).offset * 2;
			else{
				Vector3 r = ((BezierCurve)this.edges[current]).GetRotation(tmpt)*Vector3.right;
				point[0] = point[1]+2*r;
			}
		}
		else{
			point[0] = tmp;
			if (this.edges[current] is Line)
				point[1] = tmp - ((Line)this.edges[current]).offset * 2;
			else{
				Vector3 r = ((BezierCurve)this.edges[current]).GetRotation(tmpt)*Vector3.left;
				point[1] = point[0]+2*r;
			}
		}

		return point;
	}

	public Vector3 meshPoints(Line street1, Line street2, int side1, int side2){
		Vector3 point;
		try{
			point = intersection (street1.startVs [side1], street1.finishVs [side1], street2.startVs [side2], street2.finishVs [side2]);
		}
		catch{
			if(street1.start == this)
				point = street1.startVs[side1];
			else
				point = street1.finishVs[side1];

		}
		return point;
	}

	public Vector3 meshPoints(BezierCurve street1, Vector3 start, Vector3 finish, int side1,bool updateSegment){
		//calculate intersection
		bool found = false;
		//Vector3 tmps= new Vector3();
		//Vector3 tmpf= new Vector3();
		Vector3 point = new Vector3();
		Vector3[] lineVs1 = new Vector3[2];
		Vector3[] lineVs2 = new Vector3[2];

		lineVs1[0] = start;
		lineVs1[1] = finish;

		float offset = (float) 1 / street1.size;

		for (int i = 0; i< street1.size; i++) {
			lineVs2 [0] = street1.GetSidePoint (side1, (float)i/street1.size);
			lineVs2 [1] = street1.GetSidePoint (side1, (float)i/street1.size+offset);

			try{
				point = intersection (lineVs2 [0], lineVs2 [1], lineVs1 [0], lineVs1 [1]);
			}
			catch{
				if(street1.start == this)
					point = lineVs1[0];
				else
					point = lineVs1[1];
			}

			if(onLineSegment(lineVs1[0],lineVs1[1],point)&&onLineSegment(lineVs2[0],lineVs2[1],point)){

				if(updateSegment){
					if(i==0)
						i++;
					if(street1.start == this && street1.startSegment < i)
						street1.startSegment = i;
					else if(street1.finish == this && street1.endSegment > i)
						street1.endSegment = i;
				}
				i = street1.size;
				found = true;
			}
			else{
				if(i==1)
					tmps = point;
				if(i==9)
					tmpf = point;
			}
		}
		if (!found) {
			/*if(street1.start == this){
				point = tmps;
			}
			else{
				point = tmpf;
				street1.endSegment = street1.size - 1;
			}*/
			return Vector3.up;
		}
	return point;
	}

	public Vector3 meshPoints(Line street1, BezierCurve street2, int side1, int side2){
		//calculate intersection
		bool found = false;
		//Vector3 tmps= new Vector3();
		//Vector3 tmpf= new Vector3();
		Vector3 point = Vector3.up;
		Vector3[] lineVs1 = new Vector3[2];
		Vector3[] lineVs2 = new Vector3[2];
		
		lineVs1[0] = street1.startVs [side1];
		lineVs1[1] = street1.finishVs [side1];
		float offset = (float) 1 / street2.size;
		
		for (int i = 1; i< street2.size; i++) {
			lineVs2 [0] = street2.GetSidePoint (side2, (float)i/street2.size);
			lineVs2 [1] = street2.GetSidePoint (side2, (float)i/street2.size+offset);
			try{
				point = intersection (lineVs1 [0], lineVs1 [1], lineVs2 [0], lineVs2 [1]);
			}
			catch{
				if(street1.start == this)
					point = lineVs1[0];
				else
					point = lineVs1[1];
			}
			if(onLineSegment(lineVs2[0],lineVs2[1],point)){
				i = street2.size;
				found = true;
			}
			else{
				if(i==1)
					tmps = point;
				if(i==9)
					tmpf = point;
			}
		}
		if (!found) {
			/*if(street1.start == this)
				point = tmps;
			else
				point = tmpf;*/
			return Vector3.up;
		}
		return point;
	}
	public Vector3 meshPoints(BezierCurve street1, BezierCurve street2, int side1, int side2){
		Vector3 point = Vector3.up;
		for (int i=0; i<street2.size; i++) {

			point = meshPoints(street1,street2.GetSidePoint(side2,(float)i/street2.size),street2.GetSidePoint(side2,(float)(i+1)/street2.size),side1,true);
			if(point!=Vector3.up)
				break;
		}
		return point;
		
	}
	public bool onLineSegment(Vector3 a, Vector3 b, Vector3 c){
		double ab = Math.Sqrt ((a.x - b.x)* (a.x - b.x) + (a.z - b.z) * (a.z - b.z));
		double ac = Math.Sqrt ((a.x - c.x) * (a.x - c.x) + (a.z - c.z) * (a.z - c.z));
		double bc = Math.Sqrt ((b.x - c.x) * (b.x - c.x) + (b.z - c.z) * (b.z - c.z));

		if (Math.Abs (ac + bc - ab) < 0.1) // epsilon
			return true;
		else
			return false;
	}
}
