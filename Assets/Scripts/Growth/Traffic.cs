using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Traffic
{
    public static void FindDistances(Node start, List<Node> allCorners)
    {
        var unvistedNodes = new List<Node>(allCorners);
        unvistedNodes.ForEach(x =>
        {
            x.visited = false;
            x.tDistance = int.MaxValue;
        });

        start.tDistance = 0;
        Node next = new Node(Vector3.up);

        var current = start;

        while (unvistedNodes.Count > 0)
        {
            foreach (Edge path in current.edges.Where(x => !x.GetNeighbor(current).visited))
            {
                var newTDistance = current.tDistance + path.cost;
                if (newTDistance < path.GetNeighbor(current).tDistance)
                {
                    path.GetNeighbor(current).tDistance = newTDistance;
                }
            }

            current.visited = true;
            unvistedNodes.Remove(current);
            unvistedNodes.Sort((x, y) => x.tDistance.CompareTo(y.tDistance));

            if (unvistedNodes.Count != 0)
                current = unvistedNodes[0];
        }
    }

    public static List<Edge> GetPath(Node start, Node finish)
    {
        var current = finish;
        var path = new List<Edge>();
        var currentTDistance = finish.tDistance;
        bool searching = true;

        while (searching)
        {
            if (current == start)
            {
                int n = Random.Range(0, current.neighbours.Count - 1);
                Edge edge = current.edges.Where(x => current.neighbours[n].edges.Contains(x)).FirstOrDefault();
                path.Add(edge);
                break;
            }

            foreach (Edge edge in current.edges.Where(x => x.GetNeighbor(current).visited))
            {
                if (edge.GetNeighbor(current).tDistance - (currentTDistance - edge.cost) <= 0.001)
                {
                    current = edge.GetNeighbor(current);
                    path.Add(edge);
                    currentTDistance -= edge.cost;
                    break;
                }
            }
			break;
        }
        path.Reverse();
        return path;
    }
}
