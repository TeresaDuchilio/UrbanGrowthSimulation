using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LaterGrowth
{
    City city;
    BlockHelper blockHelper;
    Industrial industrial;
    Commercial commercial;
    Residential residential;
    float maxDist;
    Node blockCandidate;
    bool found;
    List<Node> unvistedNodes;
    float minStreetLength;
    float maxStreetLength;
    bool traffic;

    public LaterGrowth()
    {
        city = GameObject.Find("City").GetComponent<City>();
        industrial = new Industrial();
        commercial = new Commercial();
        residential = new Residential();
        blockHelper = new BlockHelper(city,industrial,commercial,residential);
        maxDist = 0.1f;
        minStreetLength = 9f;
        maxStreetLength = 15f;
    }

    public void SetTraffic(bool value)
    {
        this.traffic = value;
    }

    public IEnumerator Grow()
    {
        //select nodes
        var selected = SelectNodes();

        foreach (Node n in selected)
        {
            StreetExpansion(n);

            //find new blocks
            if(blockCandidate != null)
                blockHelper.FindBlocks(blockCandidate, 5, 3);
            //assing landusetype to new blocks & add marker
            for (int j = 0; blockHelper.newBlocks.Count > j; j++)
            {
                blockHelper.AssingnLUT(blockHelper.newBlocks[j]);
                blockHelper.newBlocks[j].UpdateBuilding();
            }
            //reevaluate lut of some blocks
            blockHelper.ReevaluateLUT();

            //traffic simulation
            for (int l = 0; blockHelper.newBlocks.Count > l; l++)
            { //find new trips
                blockHelper.FindTrips(blockHelper.newBlocks[l]);
            }
            blockHelper.newBlocks.Clear();

            // recalculate trips for some blocks
            blockHelper.RecalculateTrips();

            //update traffic
            for (int m = 0; m < city.blocks.Count; m++)
            {
                blockHelper.UpdateTraffic(city.blocks[m]);
            }

            //update inhabitants
            for (int k = 0; city.blocks.Count > k; k++)
                blockHelper.UpdateInhabitants(city.blocks[k]);


        }
        yield return 0;
    }

    public List<Node> SelectNodes()
    {
        var selected = new List<Node>();

        foreach (Node n in city.streetGraph.expandable)
        {
            //get node traffic
            var traffic = n.getTrafficValue();
            //get node capacity
            var capacity = n.getCapacity();
            //decide whether to add it or not 
            var dif = ((float)traffic / capacity);  

            if (UnityEngine.Random.Range(0f, 1f) <= dif)
                selected.Add(n);
        }
        return selected;
    }

    public void StreetExpansion(Node node)
    {

            Node tmp = new Node(Vector3.up);
            int count = city.streetGraph.expandable.Count;

            //connectable to existing node?
            for (int k = 0; k < count; k++)
            {
                if (Vector3.Distance(city.streetGraph.expandable[k].position, node.position) <= 15)
                {
                    if (IsValid(city.streetGraph.expandable[k], node))
                    {
                        tmp = city.streetGraph.expandable[k];
                        k = count;
                    }
                }
            }
            //create new node
            float angle = 0f;
            bool found = false;
            if (tmp.position == Vector3.up)
            {
                while (!found)
                {
                    int edgeCount = node.edges.Count;
                    if (edgeCount != 0)
                    {
                        angle += UnityEngine.Random.Range(80, 100);//random number
                        float lenght = UnityEngine.Random.Range(10, maxStreetLength); //random length
                        Vector3 point = node.position + node.edges[edgeCount - 1].GetDirectionAtNode(node) * lenght * (-1);
                        Vector3 pivot = node.position;
                        tmp.position = RotatePoint(point, pivot, new Vector3(0, angle, 0));

                    }
                    else
                        tmp.position = node.position + new Vector3(0f, 0f, 10f);

                    //tmp valid?
                    if (!found && IsValid(tmp, node))
                        found = true;
                    //no valid position found
                    if (angle >= 360)
                    {
                        Strike(node);
                        return;
                    }
                }

                //too close to exsisting node?
                for (int k = 0; k < count; k++)
                {
                    if (Vector3.Distance(city.streetGraph.expandable[k].position, tmp.position) < 9)
                    {
                        if (IsValid(city.streetGraph.expandable[k], node))
                        {
                            tmp = city.streetGraph.expandable[k];
                            k = count;
                            found = false;
                        }
                    }
                }

                //too close to exsisting street?
                if (found)
                {
                    //check distance to other streets
                    for (int i = 0; city.streetGraph.streets.Count > i; i++)
                    {
                        if (DistancePointLineSegment(city.streetGraph.streets[i].start.position, city.streetGraph.streets[i].finish.position, tmp.position) <= minStreetLength)
                        {
                            Strike(node);
                            return;
                        }
                    }

                    if (tmp.position.magnitude > maxDist)
                        maxDist = tmp.position.magnitude;

                    city.streetGraph.corners.Add(tmp);
                    city.streetGraph.expandable.Add(tmp);
                }
            }
            blockCandidate = tmp;
            //add street
            city.streetGraph.AddLine(node, tmp, 1);
            //update mesh
            CalculateNewMesh(node);
            CalculateNewMesh(tmp);

            if (node.isFull())
                city.streetGraph.expandable.Remove(node);
            if (tmp.isFull())
            {
                city.streetGraph.expandable.Remove(tmp);
            }
    }

    void Strike(Node node)
    {
        node.strike++;
        if (node.strike > 5)
        {
            if (node.edges.Count == 1)
            {//remove wrong expansion
                GameObject.Destroy(node.edges[0].mesh);
                city.streetGraph.streets.Remove(node.edges[0]);
                city.streetGraph.corners.Remove(node);
                CalculateNewMesh(node.edges[0].GetNeighbor(node));
            }

            city.streetGraph.expandable.Remove(node);
        }
    }

    void CalculateNewMesh(Node node)
    {

        city.streetGraph.Crossing(node);

        MeshHelper meshHelper = new MeshHelper();
        //corner
        meshHelper.CornerMesh(node);
        if (node.mesh != null)
            meshHelper.UpdateMesh(node.mesh, city.streetGraph.materials[1]);
        else
            node.mesh = meshHelper.CreateMesh(city.streetGraph.materials[1], "corners");

        //streets
        for (int k = 0; k < node.edges.Count; k++)
        {
            meshHelper = new MeshHelper();
            meshHelper.LineMesh((Line)node.edges[k]);
            if (node.edges[k].mesh != null)
            {
                meshHelper.UpdateMesh(node.edges[k].mesh, city.streetGraph.PickMaterial(node.edges[k],traffic));
            }
            else
                node.edges[k].mesh = meshHelper.CreateMesh(city.streetGraph.PickMaterial(node.edges[k],traffic), "streets");
        }

    }

    bool IsValid(Node newNode, Node oldNode)
    {

        //Nodes not already connected
        if (oldNode.position == newNode.position || oldNode.neighbours.Any(i => i.position == newNode.position))
            return false;
        //Angle to neighbouring streets big enough
        if (!AnglesValid(newNode, oldNode))
            return false;
        if (!AnglesValid(oldNode, newNode))
            return false;

        //intersection?
        for (int i = 0; i < city.streetGraph.streets.Count; i++)
        {
            if (newNode != city.streetGraph.streets[i].start && newNode != city.streetGraph.streets[i].finish)
            {
                if (oldNode != city.streetGraph.streets[i].start && oldNode != city.streetGraph.streets[i].finish)
                {
                    //intersection
                    if (FasterLineSegmentIntersection(newNode.position, oldNode.position, city.streetGraph.streets[i].start.position, city.streetGraph.streets[i].finish.position))
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    public bool AnglesValid(Node a, Node b)
    {
        for (int k = 0; k < a.edges.Count; k++)
        {
            float angle = Vector3.Angle(a.edges[k].GetDirectionAtNode(a), a.position - b.position);
            Vector3 cross = Vector3.Cross(a.edges[k].GetDirectionAtNode(a), a.position - b.position);

            if (cross.y < 0)
                angle = 360 - angle;
            if (angle > 180)
                angle = 360 - angle;
            if (angle < 50f)
                return false;
        }
        return true;
    }

    // Return minimum distance between line segment vw and point p
    float DistancePointLineSegment(Vector3 v, Vector3 w, Vector3 p)
    {
        float len2 = (v - w).magnitude;
        len2 = len2 * len2;
        if (len2 == 0.0) return Vector3.Distance(p, v);   // v == w

        float t = Math.Max(0, Math.Min(1, Vector3.Dot(p - v, w - v) / len2));
        Vector3 projection = v + t * (w - v);
        return Vector3.Distance(p, projection);
    }

    public Vector3 RotatePoint(Vector3 point, Vector3 pivot, Vector3 angles)
    {
        Vector3 direction = point - pivot;
        direction = Quaternion.Euler(angles) * direction;
        point = direction + pivot;
        return point;
    }

    //Based on Faster Line Segment Intersection	Franklin Antonio, Graphics Gems III, 1992, pp. 199-202
    static bool FasterLineSegmentIntersection(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {

        Vector3 a = p2 - p1;
        Vector3 b = p3 - p4;
        Vector3 c = p1 - p3;

        float alphaNumerator = b.z * c.x - b.x * c.z;
        float alphaDenominator = a.z * b.x - a.x * b.z;
        float betaNumerator = a.x * c.z - a.z * c.x;
        float betaDenominator = alphaDenominator;

        bool doIntersect = true;

        if (alphaDenominator == 0 || betaDenominator == 0)
        {
            doIntersect = false;
        }
        else
        {

            if (alphaDenominator > 0)
            {
                if (alphaNumerator < 0 || alphaNumerator > alphaDenominator)
                {
                    doIntersect = false;
                }
            }
            else if (alphaNumerator > 0 || alphaNumerator < alphaDenominator)
            {
                doIntersect = false;
            }

            if (doIntersect && betaDenominator > 0)
            {
                if (betaNumerator < 0 || betaNumerator > betaDenominator)
                {
                    doIntersect = false;
                }
            }
            else if (betaNumerator > 0 || betaNumerator < betaDenominator)
            {
                doIntersect = false;
            }
        }

        return doIntersect;
    }
}
