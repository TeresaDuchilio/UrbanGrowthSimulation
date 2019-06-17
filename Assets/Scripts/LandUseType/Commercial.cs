using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Commercial:LandUseType
{
	public Commercial()
    {
		luv = 0f;
		localNeed = 0f;
		name = "commercial";
	}
	
	//return local need of block
	public override float CalculateLocalNeed(Block block)
    {
		List <Block> neighbors = block.GetNeighbors ();
		float clustering = 0f;
		float residnetialDistance = 0f;
		float traffic = 0f;
		float centerDist = 0f;

		if (neighbors.Count > 0) {
			for (int i=0; i<neighbors.Count; i++) {
                if (neighbors[i].lut != null)
                {
                    if (neighbors[i].lut.getName() == "commercial")     //clustering
                        clustering++;
                    else if (neighbors[i].lut.getName() == "residential")//next to residential
                        residnetialDistance++;
                }
			}
			clustering = clustering / neighbors.Count;
			residnetialDistance = residnetialDistance / neighbors.Count;
		}

		for (int j =0; j< block.streets.Count; j++)//surrounded by well trafficed streets
			traffic = block.streets [j].traffic / block.streets [j].capacity;

		traffic = traffic / block.streets.Count;

		float tmp = block.nodes[0].position.magnitude; //in the city center
		centerDist = Mathf.Exp (-1 / centreSize * tmp);

		localNeed = 0.2f * residnetialDistance + 0.1f * clustering + 0.3f * traffic + 0.4f * centerDist;
		return localNeed;
	}

	public override float TripAttractiveness (float distance, string lut, int inhabitants)
    {
		float dist = Mathf.Exp (-1 /distance);
		float destination = 0f;

		switch (lut) {
		case "commercial":
			destination = 0.5f;
				break;
		case "industrial":
			destination = 0.5f;
			break;
		case "residential":
			destination = 0.9f;
			break;
        case "core":
            destination = 1f;
            break;

        }

		return 0.5f * dist + 0.5f * destination;
	}

	public override int InhabitantGrowth (Block block, int currentInhabitants)
    {
		int traffic = 0;
		//depending on traffic
		for (int i = 0; i< block.streets.Count; i++) {
			traffic += block.streets[i].traffic;
		}
		return traffic;
	}
}