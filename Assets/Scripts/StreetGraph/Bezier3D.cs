using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Bezier3D : MonoBehaviour
{
	int size = 10; // higher number means smoother but also more verts/tris

	public Vector3 start = new Vector3(0, 0, 0);
	public Vector3 end = new Vector3(1, 0, 0);
	public Vector3 handle1 = new Vector3(0, 0, 0);
	public Vector3 handle2 = new Vector3(1, 0, 0);
	public Line[] segments = new Line[10];

	private void Start()
	{
		Mesh mesh = new Mesh();
		List<Vector3> vertices = new List<Vector3>();
		List<int> triangles = new List<int>();
		List<Vector3> normals = new List<Vector3>();
		
		Vector3 Start = GetPoint(0f);
		Quaternion rotation = GetRotation(0);
		Vector3 left = rotation * Vector3.left;
		Vector3 right = rotation * Vector3.right;
		Vector3 up = rotation * Vector3.up;
		vertices.Add(Start + right);
		vertices.Add(Start + left);
		normals.Add(up);
		normals.Add(up);
		int triIndex = 0;
		

		for (int i = 0; i <= size; i++)
		{
			float t = (float)i / (float)size;
			Vector3 End = GetPoint(t);
			rotation = GetRotation(t);
			
			left = rotation * Vector3.left;
			right = rotation * Vector3.right;
			up = rotation * Vector3.up;
			
			vertices.Add(End + right);
			vertices.Add(End + left);
			normals.Add(up);
			normals.Add(up);
			
			triangles.Add(triIndex);
			triangles.Add(triIndex + 1);
			triangles.Add(triIndex + 2);
			
			triangles.Add(triIndex + 2);
			triangles.Add(triIndex + 1);
			triangles.Add(triIndex + 3);
			
			triIndex += 2;
			
			Start = End;
		}
		
		mesh.SetVertices(vertices);
		mesh.SetNormals(normals);
		mesh.SetTriangles(triangles, 0);
		GetComponent<MeshFilter>().mesh = mesh;
	}

	private Quaternion GetRotation(float t)
	{
		return Quaternion.LookRotation(GetDirection(t), Vector3.up);
	}

	public Vector3 GetPoint(float t)
	{
		return Bezier.GetPoint(start, handle1, handle2, end, t);
	}
	
	public Vector3 GetVelocity(float t)
	{
		return Bezier.GetFirstDerivative(start, handle1, handle2, end, t);
	}
	
	public Vector3 GetDirection(float t)
	{
		return GetVelocity(t).normalized;
	}
}