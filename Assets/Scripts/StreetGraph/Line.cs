using UnityEngine;
using System.Collections;
using System;

public class Line:Edge{

	public Vector3[] startVs;
	public Vector3[] finishVs;
	public Vector3 offset;
	public Quaternion rotation;
	
	public Line(Node start, Node finish, int traffic):base(){
		this.start = start;
		this.finish = finish;
		this.traffic = traffic;

		this.direction = (this.finish.position - this.start.position).normalized;
		rotation = Quaternion.LookRotation(direction,Vector3.up);
		offset = Vector3.Cross (Vector3.up, direction);
		
		startVs = new []{this.start.position + rotation*Vector3.right, this.start.position + rotation*Vector3.left}; // above start position
		finishVs = new[]{this.finish.position + rotation*Vector3.right, this.finish.position + rotation*Vector3.left}; // above finish position

		meshStart = new[]{startVs[0],startVs[1],Vector3.zero};
		meshFinish = new[]{finishVs[0],finishVs[1],Vector3.zero};

		FindLenght ();
		cost = StreetCost ();
	}

	protected override void FindLenght(){
		length = (float)Math.Sqrt ((start.position.x - finish.position.x)* (start.position.x - finish.position.x) + (start.position.z - finish.position.z) * (start.position.z - finish.position.z));
	}

	public override Vector3 GetDirectionAtNode(Node node){
		if (this.start == node)
			return -1 * this.direction;
		else
			return this.direction;
	}

    public override void Grow()
    {
        if(size < 3)
        {
            size++;

            if (size == 2)
                capacity = 100;
            else
                capacity = 1000;
        }
        return;
    }
}
