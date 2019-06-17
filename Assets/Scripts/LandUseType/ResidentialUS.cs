
using System;
using System.Collections.Generic;
using UnityEngine;

public class ResidentialUS : LandUseType
{
    public ResidentialUS()
    {
        luv = 0f;
        localNeed = 0f;
        name = "residential";
    }

    public override float CalculateLocalNeed(Block block)
    {
        List<Block> neighbors = block.GetNeighbors();
        float clustering = 0f;
        float industrialDistance = 0f;
        float traffic = 0f;

        if (neighbors.Count > 0)
        {
            for (int i = 0; i < neighbors.Count; i++)
            {
                if (neighbors[i].lut != null)
                {
                    if (neighbors[i].lut.getName() == "residential")     //clustering 
                        clustering++;
                    else if (neighbors[i].lut.getName() == "industrial")//not next to industrial
                        industrialDistance++;
                }
            }
            clustering = clustering / neighbors.Count;
            industrialDistance = 1 - (industrialDistance / neighbors.Count);
        }

        for (int j = 0; j < block.streets.Count; j++)//not surrounded by well trafficed streets
            traffic = block.streets[j].traffic / block.streets[j].capacity;

        traffic = 1 - (traffic / block.streets.Count);

        localNeed = 0.2f * industrialDistance + 0.4f * clustering + 0.4f * traffic;
        return localNeed;
    }

    public override int InhabitantGrowth(Block block, int currentInhabitants)
    {
        if (localNeed <= 0.3f)
            return currentInhabitants;
        else
            return currentInhabitants + (int)(currentInhabitants * Math.Min(0.25f, localNeed));
    }

    public override float TripAttractiveness(float distance, string lut, int inhabitants)
    {
       // int inhabitants = 1000;
        distance = distance == 0 ? 1 : distance;
        float dist = 1 / distance;
        float destination = 0f;

        switch (lut)
        {
            case "commercial":
                destination = 0.7f;
                break;
            case "industrial":
                destination = 0.5f;
                break;
            case "residential":
                destination = 0.4f;
                break;
            case "core":
                destination = 1f;
                break;
        }
        return destination * inhabitants * dist;
    }
}