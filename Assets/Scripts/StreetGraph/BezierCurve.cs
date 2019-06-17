using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BezierCurve : Edge {

	public int size = 5;
	public Vector3 handle1;
	public Vector3 handle2;
	public int startSegment,endSegment;
	public float[] distanceSample; 
	//public Line[] segments = new Line[size];
	
	public BezierCurve(Node start, Node finish, int traffic,Vector3 h1,Vector2 h2):base(){
		this.direction = GetDirection (0.1f);
		this.start = start;
		this.finish = finish;
		this.traffic = traffic;
		this.handle1 = h1;
		this.handle2 = h2;
		this.meshStart = new[]{this.GetSidePoint (0, 0f),this.GetSidePoint (1, 0f),Vector3.zero};
		this.meshFinish = new[]{this.GetSidePoint (0,1f),this.GetSidePoint (1, 1f),Vector3.zero};
		this.startSegment = 0;
		this.endSegment = this.size;
		this.distanceSample = new float[size+1];

		FindLenght ();
		cost = StreetCost ();
		//segment ();

	}

	protected override void FindLenght(){
		calculateSampleArray ();
	}

	void calculateSampleArray()
	{
		distanceSample[0] = 0f;
		Vector3 prev = start.position;
		for (int i = 1; i <= size; i++) {
			float t = ((float)i) / (size);
			Vector3 pt = this.GetPoint (t);
			float diff = (prev - pt).magnitude;
			length += diff;
			distanceSample [i] = length;
			prev = pt;
		}
	}
	/*void segment(){
		Quaternion rotation = GetRotation(0);
		Vector3 left = rotation * Vector3.left;
		Vector3 right = rotation * Vector3.right;

		for (int i = 0; i <= size; i++)
		{
			segments[i] = new Line(this.start,this.finish);
			segments[i].startVs[0]= left;
			segments[i].startVs[1]= right;

			float t = (float)i / (float)size;
			rotation = GetRotation(t);
			left = rotation * Vector3.left;
			right = rotation * Vector3.right;

			segments[i].finishVs[0]= left;
			segments[i].finishVs[1]= right;
		}
	}*/

	public Vector3 GetSidePoint(int direction, float t){ //0 == left, 1 == right 
		Vector3 temp = new Vector3();
		temp = this.GetPoint(t);
		Quaternion rotation = this.GetRotation(t);
		if(direction == 1)
			return temp + rotation * Vector3.left;
		else
			return temp + rotation * Vector3.right;		
	}

	public override Vector3 GetDirectionAtNode(Node n){
	
		if (n == this.start)
			return this.direction;
		else
			return GetDirection (size-0.1f);
	}

	public Quaternion GetRotation(float t)
	{
		if (t == 0)
			t = 0.1f;
		return Quaternion.LookRotation(GetDirection(t), Vector3.up);
	}
	
	public Vector3 GetPoint(float t)
	{
		return Bezier.GetPoint(start.position, handle1, handle2, finish.position, t);
	}
	
	public Vector3 GetVelocity(float t)
	{
		return Bezier.GetFirstDerivative(start.position, handle1, handle2, finish.position, t);
	}
	
	public Vector3 GetDirection(float t)
	{
		return GetVelocity(t).normalized;
	}

    public override void Grow()
    {
        throw new System.NotImplementedException();
    }
}
