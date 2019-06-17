using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeshHelper{

	public List<Vector3> vertices = new List<Vector3>();
	
	public List<Vector3> normals = new List<Vector3>();
	
	public List<Vector2> uvs = new List<Vector2>();
	
	public List<int> indices = new List<int>();

	public void AddTriangle(int index0, int index1, int index2)
	{
		indices.Add(index0);
		indices.Add(index1);
		indices.Add(index2);
	}
	
	public GameObject CreateMesh(Material material, string parentTag)
	{
		Mesh mesh = new Mesh();

		mesh.SetVertices(vertices);
		mesh.SetNormals(normals);
		mesh.SetUVs(0,uvs);
		mesh.triangles = indices.ToArray();

		var gameObject = new GameObject();
		var meshFilter = gameObject.AddComponent<MeshFilter>();
		gameObject.AddComponent<MeshRenderer>();
		gameObject.GetComponent<MeshRenderer> ().material = material;
		meshFilter.sharedMesh = mesh;
		gameObject.transform.parent = GameObject.FindWithTag (parentTag).transform;
		return gameObject;
	}

	public void UpdateMesh(GameObject gameObject,Material material){

		var meshFilter = gameObject.GetComponent<MeshFilter>();
		gameObject.GetComponent<MeshRenderer> ().material = material;
		meshFilter.sharedMesh.Clear ();

		meshFilter.sharedMesh.SetVertices(vertices);
		meshFilter.sharedMesh.SetNormals(normals);
		meshFilter.sharedMesh.SetUVs(0,uvs);
		meshFilter.sharedMesh.triangles = indices.ToArray();

	}

    public void UpdateMaterial(GameObject gameObject, Material material)
    {
        gameObject.GetComponent<MeshRenderer>().material = material;
    }

	//Corner Mesh
	public void CornerMesh(Node corner){

		int vcount = 0;
		if(corner.edges.Count > 1){
			if(corner.edges.Count == 2){
				if(corner.edges[0].finish == corner)
					vertices.Add(corner.edges[0].meshFinish[0]);
				else
					vertices.Add(corner.edges[0].meshStart[1]);

				if(corner.edges[1].finish == corner){
					vertices.Add(corner.edges[1].meshFinish[0]);
					vertices.Add(corner.edges[1].meshFinish[1]);
				}
				else{
					vertices.Add(corner.edges[1].meshStart[1]);
					vertices.Add(corner.edges[1].meshStart[0]);
				}

				uvs.Add(new Vector2(0.0f, 0.0f));
				uvs.Add(new Vector2(0.0f, 1.0f));
				uvs.Add(new Vector2(1.0f, 0.5f));

				normals.Add(Vector3.up);
				normals.Add(Vector3.up);
				normals.Add(Vector3.up);

				AddTriangle(0, 1,2);
			}
			else{
				for (int current = 0; current < corner.edges.Count; current++){

					if(corner.edges[current].start == corner){
						vertices.Add(corner.edges[current].meshStart[1]);
						vertices.Add(corner.edges[current].meshStart[0]);
						vertices.Add(corner.edges[current].meshStart[2]);
					}
					else{
						vertices.Add(corner.edges[current].meshFinish[0]);
						vertices.Add(corner.edges[current].meshFinish[1]);
						vertices.Add(corner.edges[current].meshFinish[2]);
					}

					uvs.Add(new Vector2(0.0f, 0.0f));
					uvs.Add(new Vector2(0.0f, 1.0f));
					uvs.Add(new Vector2(1.0f, 0.5f));

					normals.Add(Vector3.up);
					normals.Add(Vector3.up);
					normals.Add(Vector3.up);
					AddTriangle(vcount+ 0, vcount +1,vcount + 2);
					vcount += 3;
				}
				if(corner.meshPs.Count >0){
					vertices.Add(corner.meshPs[0]);
					uvs.Add(new Vector2(0.0f, 0.0f));
					normals.Add(Vector3.up);
					
					vertices.Add(corner.meshPs[1]);
					uvs.Add(new Vector2(0.0f, 1.0f));
					normals.Add(Vector3.up);
					
					vertices.Add(corner.meshPs[2]);
					uvs.Add(new Vector2(1.0f, 0.0f));
					normals.Add(Vector3.up);

					AddTriangle(vcount +2, vcount +1, vcount +0);
					if(corner.meshPs.Count>=4){
						vertices.Add(corner.meshPs[3]);
						uvs.Add(new Vector2(1.0f, 1.0f));
						normals.Add(Vector3.up);
						
						AddTriangle(vcount +3, vcount +1, vcount +2);
					}
					vcount = 0;
				}
			}
		}

	}
	//Line Mesh

	public void LineMesh(Line street){
		vertices.Add(street.meshStart[0]);
		uvs.Add(new Vector2(0.0f, 0.0f));
		normals.Add(Vector3.up);
		
		vertices.Add(street.meshStart[1]);
		uvs.Add(new Vector2(0.0f, 1.0f));
		normals.Add(Vector3.up);
		
		vertices.Add(street.meshFinish[1]);
		uvs.Add(new Vector2(street.length, 1.0f));
		normals.Add(Vector3.up);
		
		vertices.Add(street.meshFinish[0]);
		uvs.Add(new Vector2(street.length, 0.0f));
		normals.Add(Vector3.up);
		
		AddTriangle(0, 1, 2);
		AddTriangle(0,2,3);
	}

	//BezierMesh
	public void BezierMesh(BezierCurve street){
		int size = street.size;
		Quaternion rotation;
		Vector3 left;
		Vector3 right;
		Vector3 up = street.GetRotation((float)street.startSegment/size)*Vector3.up;
		vertices.Add(street.meshStart[0]);
		vertices.Add(street.meshStart[1]);
		normals.Add(up);
		normals.Add(up);

		uvs.Add(new Vector2(street.distanceSample[street.startSegment],0));
		uvs.Add(new Vector2(street.distanceSample[street.startSegment], 1));
		int vcount = 0;

		for (int i = street.startSegment+1; i < street.endSegment; i++)
		{
			float t = (float)i / (float)size;
			Vector3 End = street.GetPoint(t);
			rotation = street.GetRotation(t);
			
			left = rotation * Vector3.left;
			right = rotation * Vector3.right;
			up = rotation * Vector3.up;
		
			vertices.Add(End + right);
			vertices.Add(End + left);
			normals.Add(up);
			normals.Add(up);
			uvs.Add(new Vector2(street.distanceSample[i], 0));
			uvs.Add(new Vector2(street.distanceSample[i], 1));

			AddTriangle(vcount,vcount+1,vcount+2);
			AddTriangle(vcount+2,vcount+1,vcount+3);
			
			vcount += 2;

		}
		vertices.Add(street.meshFinish[0]);
		vertices.Add(street.meshFinish[1]);
		normals.Add(up);
		normals.Add(up);
		uvs.Add(new Vector2(street.distanceSample[street.endSegment], 0));
		uvs.Add(new Vector2(street.distanceSample[street.endSegment], 1));
		AddTriangle(vcount,vcount+1,vcount+2);
		AddTriangle (vcount + 2, vcount + 1, vcount + 3);
	}

	public void BuildingMesh(List<Vector3> foundation, float height){
		int vcount = 0;
		//roof
		for (int i =0; i < foundation.Count; i++){ 
			vertices.Add(new Vector3(foundation[i].x,height,foundation[i].z));
			normals.Add(Vector3.up);
			vertices.Add(new Vector3(foundation[i].x,height,foundation[i].z));
			normals.Add(Vector3.zero);
			vertices.Add(new Vector3(foundation[i].x,height,foundation[i].z));
			normals.Add(Vector3.zero);
			vcount+=3;
		}

		int[] vs = Triangulate (foundation);

		for (int k =0; k< vs.Length; k=k+3) {
			AddTriangle (vs [k]*3, vs [k + 1]*3, vs [k + 2]*3);
		}


		//walls
		vertices.Add(foundation[0]);
		normals.Add(Vector3.zero);
		int tmp = 3;
		for (int j =1 ; j < foundation.Count; j++) {
			vertices.Add(foundation[j]);
			normals.Add(Vector3.zero);
			AddTriangle(vcount + j-1,vcount + j,(j-1)*3);
			AddTriangle(vcount +j,j*3,(j-1)*3);
		}

		AddTriangle (vcount + foundation.Count -1 , vcount, 0);
		AddTriangle (vcount + foundation.Count - 1, 0, vcount - 3);
	}



	public int[] Triangulate(List<Vector3> points) {
		List<int> indices = new List<int>();
		
		int n = points.Count;
		if (n < 3)
			return indices.ToArray();
		
		int[] V = new int[n];
		if (Area(points) > 0) {
			for (int v = 0; v < n; v++)
				V[v] = v;
		}
		else {
			for (int v = 0; v < n; v++)
				V[v] = (n - 1) - v;
		}
		
		int nv = n;
		int count = 2 * nv;
		for (int m = 0, v = nv - 1; nv > 2; ) {
			if ((count--) <= 0)
				return indices.ToArray();
			
			int u = v;
			if (nv <= u)
				u = 0;
			v = u + 1;
			if (nv <= v)
				v = 0;
			int w = v + 1;
			if (nv <= w)
				w = 0;
			
			if (Snip(u, v, w, nv, V, points)) {
				int a, b, c, s, t;
				a = V[u];
				b = V[v];
				c = V[w];
				indices.Add(a);
				indices.Add(b);
				indices.Add(c);
				m++;
				for (s = v, t = v + 1; t < nv; s++, t++)
					V[s] = V[t];
				nv--;
				count = 2 * nv;
			}
		}
		
		indices.Reverse();
		return indices.ToArray();
	}
	
	private float Area (List<Vector3> points) {
		int n = points.Count;
		float A = 0.0f;
		for (int p = n - 1, q = 0; q < n; p = q++) {
			Vector3 pval = points[p];
			Vector3 qval = points[q];
			A += pval.x * qval.z - qval.x * pval.z;
		}
		return (A * 0.5f);
	}
	
	private bool Snip (int u, int v, int w, int n, int[] V, List<Vector3> points) {
		int p;
		Vector3 A = points[V[u]];
		Vector3 B = points[V[v]];
		Vector3 C = points[V[w]];
		if (Mathf.Epsilon > (((B.x - A.x) * (C.z - A.z)) - ((B.z - A.z) * (C.x - A.x))))
			return false;
		for (p = 0; p < n; p++) {
			if ((p == u) || (p == v) || (p == w))
				continue;
			Vector2 P = points[V[p]];
			if (InsideTriangle(A, B, C, P))
				return false;
		}
		return true;
	}
	
	private bool InsideTriangle (Vector3 A, Vector3 B, Vector3 C, Vector3 P) {
		float ax, az, bx, bz, cx, cz, apx, apz, bpx, bpz, cpx, cpz;
		float cCROSSap, bCROSScp, aCROSSbp;
		
		ax = C.x - B.x; az = C.z - B.z;
		bx = A.x - C.x; bz = A.z - C.z;
		cx = B.x - A.x; cz = B.z - A.z;
		apx = P.x - A.x; apz = P.z - A.z;
		bpx = P.x - B.x; bpz = P.z - B.z;
		cpx = P.x - C.x; cpz = P.z - C.z;
		
		aCROSSbp = ax * bpz - az * bpx;
		cCROSSap = cx * apz - cz * apx;
		bCROSScp = bx * cpz - bz * cpx;
		
		return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
	}

}
