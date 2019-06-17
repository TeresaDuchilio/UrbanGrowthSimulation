using UnityEngine;
using System.Collections;

public class Core : LandUseType
{
    public Core()
    {
        luv = 0f;
        localNeed = 0f;
        name = "core";
    }

    public override float CalculateLocalNeed(Block block)
    {
        throw new System.NotImplementedException();
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
        float dist = Mathf.Exp(-1 / distance);
        float destination = 0f;

        switch (lut)
        {
            case "commercial":
                destination = 0.7f;
                break;
            case "industrial":
                destination = 0.4f;
                break;
            case "residential":
                destination = 0.7f;
                break;

        }

        return 0.5f * dist + 0.5f * destination;
    }
}
