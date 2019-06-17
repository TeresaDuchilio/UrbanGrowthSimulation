
using System.Collections.Generic;
using UnityEngine;

public class IndustrialUS : LandUseType
{
    public IndustrialUS()
    {
        luv = 0f;
        localNeed = 0f;
        name = "industrial";
    }

    public override float CalculateLocalNeed(Block block)
    {
        List<Block> neighbors = block.GetNeighbors();
        float clustering = 0f;
        float residentialDistance = 0f;
        float centerDist = 0f;

        if (neighbors.Count > 0)
        {
            for (int i = 0; i < neighbors.Count; i++)
            {
                if (neighbors[i].lut != null)
                {
                    if (neighbors[i].lut.getName() == "industrial")     //clustering
                        clustering++;
                    else if (neighbors[i].lut.getName() == "residential")//not next to residential
                        residentialDistance--;
                }
            }
            clustering = clustering / neighbors.Count;
            residentialDistance = residentialDistance / neighbors.Count;

        }

        float tmp = block.nodes[0].position.magnitude; //not in the city center
        centerDist = 1 - Mathf.Exp(-1 / centreSize * tmp);

        localNeed = 0.1f * residentialDistance + 0.4f * clustering + 0.5f * centerDist;
        return localNeed;
    }

    public override int InhabitantGrowth(Block block, int currentInhabitants)
    {
        int traffic = 0;
        //depending on traffic
        for (int i = 0; i < block.streets.Count; i++)
        {
            traffic += block.streets[i].traffic;
        }
        return traffic;
    }

    public override float TripAttractiveness(float distance, string lut, int inhabitants)
    {

        distance = distance == 0 ? 1 : distance;
        float dist = 1 / distance;
        float destination = 0f;
        //int inhabitants = 1000;
        
        switch (lut)
        {
            case "commercial":
                destination = 0.5f;
                break;
            case "industrial":
                destination = 0.2f;
                break;
            case "residential":
                destination = 0.6f;
                break;
            case "core":
                destination = 1f;
                break;

        }

        return destination * inhabitants * dist;
    }
}